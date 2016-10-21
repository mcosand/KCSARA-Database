using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Kcsar.Database.Model;
using Moq;

namespace Internal.Data
{
  public abstract class FakeDbContext<TDbContextInterface>
    where TDbContextInterface : class, IDbContext
  {
    // ReSharper disable StaticMemberInGenericType
    private static readonly object LearnLock = new object();
    private static readonly Dictionary<Type, List<PropertyInfo>> CollectionProperties = new Dictionary<Type, List<PropertyInfo>>();
    private static readonly Dictionary<Type, List<PropertyInfo>> NavigationProperties = new Dictionary<Type, List<PropertyInfo>>();
    // ReSharper restore StaticMemberInGenericType

    private static readonly List<ManyManyMap> ManyToManyAssociations = new List<ManyManyMap>
    {
      //   ManyManyMap.Create<TagRow, AccountRow>(t => t.Accounts, a => a.Tags)
    };

    private readonly HashSet<object> _knownEntities = new HashSet<object>();
    private readonly List<IFakeDbSet> _tableList = new List<IFakeDbSet>();

    public Mock<TDbContextInterface> Mock { get; private set; }

    protected bool Debug { get; set; }

    /// <summary>Default Constructor</summary>
    protected void Init()
    {
      Mock = new Mock<TDbContextInterface>(MockBehavior.Strict);

      LearnTables();
      LearnProperties();
      foreach (var manyMap in ManyToManyAssociations)
      {
        CollectionProperties[manyMap.LeftType].Add(manyMap.LeftProperty);
        CollectionProperties[manyMap.RightType].Add(manyMap.RightProperty);
      }
      SetupMock();
    }

    public void ResolveEntities()
    {
      DebugLine("###### Starting ResolveEntities");

      // Walk tables and make sure the top-level tables contain all nested row objects.
      WalkTables(
        row =>
        {
          DebugLine("Checking if {0} is known entity", row);
          if (!_knownEntities.Contains(row))
          {
            DebugLine("New known entity {0}", row);
            _knownEntities.Add(row);
          }
        },
        (row, collectionItemProperty, collectionItem) =>
        {
          var foreignTable = GetTableForType(collectionItem.GetType());
          if (!foreignTable.Contains(collectionItem))
          {
            if (_knownEntities.Contains(collectionItem))
            {
              return false;
            }

            foreignTable.Add(collectionItem);
            _knownEntities.Add(collectionItem);
          }
          return true;
        },
        null
      );

      // Walk tables and make sure foreign keys are consistent with the ID properties
      WalkTables(
        null,
        null,
        (row, navItemProperty, navKeyProperty) =>
        {
          if (navKeyProperty != null)
          {
            var navKeyObject = navKeyProperty.GetValue(row);
            long? navKey = null;
            if (navKeyObject != null)
            {
              navKey = (navKeyObject is int?)
                          ? ((int?)navKeyObject).Value
                          : (navKeyObject is long?)
                            ? (long?)navKeyObject
                            : (navKeyObject is byte?)
                              ? (byte?)navKeyObject
                              : (short?)navKeyObject;
            }

            var navItem = navItemProperty.GetValue(row);
            if (navKey.HasValue && navItem == null)
            {
              var foreignTable = GetTableForType(navItemProperty.PropertyType);
              if (foreignTable != null)
              {
                var referencedItem = foreignTable.Cast<object>().Where(f => GetId(f) == navKey).SingleOrDefault();
                if (referencedItem != null)
                {
                  navItemProperty.SetValue(row, referencedItem);
                }
              }
            }
          }
        }
        );

      // clean up deleted items
      // - Get a list of all previously known entities
      // - WalkTables to remove entries from this list if we still care about them
      // - Remove entries we don't care about.
      var entitiesToRemove = new List<object>(_knownEntities);
      WalkTables(
        row => entitiesToRemove.Remove(row),
        null,
        null
        );
      foreach (var item in entitiesToRemove)
      {
        _knownEntities.Remove(item);
      }
    }

    protected virtual void SetupMock()
    {
      Mock.Setup(f => f.Dispose());
      Mock.Setup(f => f.SaveChangesAsync()).Callback(ResolveEntities).Returns(Task.FromResult(1));

      //Mock.SetupGet(f => f.CommandTimeout).Returns(60);
      //Mock.SetupSet(f => f.CommandTimeout = It.IsAny<int>());
    }

    private void LearnTables()
    {
      var tableProperties = GetType()
        .GetProperties()
        .Where(f => f.PropertyType.IsGenericType && f.PropertyType.GetGenericTypeDefinition() == typeof(FakeDbSet<>))
        .ToList();

      _tableList.AddRange(tableProperties.Select(f => (IFakeDbSet)f.GetValue(this)));
    }

    private void LearnProperties()
    {
      foreach (var table in _tableList)
      {
        if (CollectionProperties.ContainsKey(table.ElementType))
        {
          continue;
        }

        lock (LearnLock)
        {
          if (CollectionProperties.ContainsKey(table.ElementType))
          {
            continue;
          }

          CollectionProperties.Add(
            table.ElementType,
            table.ElementType.GetProperties()
            .Where(f => f.GetGetMethod().IsVirtual
                        && f.PropertyType.IsGenericType
                        && f.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)).ToList()
            );

          NavigationProperties.Add(
            table.ElementType,
            table.ElementType.GetProperties()
              .Where(f => f.GetGetMethod().IsVirtual && f.GetCustomAttribute<ForeignKeyAttribute>() != null)
              .ToList());

          DebugLine(
            "Collection properties for {0}:\n  {1}",
            table.ElementType.Name,
            string.Join("\n  ",
              CollectionProperties[table.ElementType].Select(f => f.Name)));
          DebugLine(
            "Navigation properties for {0}:\n  {1}",
            table.ElementType.Name,
            string.Join("\n  ",
              NavigationProperties[table.ElementType].Select(f => f.Name)));
        }
      }
    }

    private IFakeDbSet GetTableForType(Type rowType)
    {
      do
      {
        // new variable to appease Resharper
        var rType = rowType;
        var foreignTable = GetType().GetProperties()
          .Where(f => f.PropertyType == typeof(FakeDbSet<>).MakeGenericType(rType))
          .Select(f => (IFakeDbSet)f.GetValue(this))
          .FirstOrDefault();

        if (foreignTable != null)
        {
          return foreignTable;
        }

        rowType = rowType?.BaseType;
      } while (rowType != typeof(object));
      return null;
    }

    private void WalkTables(Action<object> rowAction, Func<object, PropertyInfo, object, bool> collectionItemAction, Action<object, PropertyInfo, PropertyInfo> navigationItemAction)
    {
      foreach (var table in _tableList)
      {
        foreach (var row in table)
        {
          ManageRow(row, rowAction, collectionItemAction, navigationItemAction);
        }
      }
    }

    private static IEnumerable<PropertyInfo> GetProperties(Dictionary<Type, List<PropertyInfo>> dict, Type rowType)
    {
      do
      {
        List<PropertyInfo> list;
        // ReSharper disable once AssignNullToNotNullAttribute
        if (dict.TryGetValue(rowType, out list))
        {
          return list;
        }
        rowType = rowType.BaseType;
      } while (rowType != typeof(object));
      throw new KeyNotFoundException();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="row"></param>
    /// <param name="rowAction"></param>
    /// <param name="collectionItemAction">
    ///   <para>A callback for processing items in this rows collections.</para>
    ///   <para>Should return true if the row is interesting (should be kept).
    ///         Return false if the row should be removed.</para>
    /// </param>
    /// <param name="navigationItemAction"></param>
    private void ManageRow(object row, Action<object> rowAction, Func<object, PropertyInfo, object, bool> collectionItemAction, Action<object, PropertyInfo, PropertyInfo> navigationItemAction)
    {
      DebugLine("ManageRow: {0}", row);
      var rowType = row.GetType();
      if (rowAction != null)
      {
        rowAction(row);
      }

      if (collectionItemAction != null)
      {
        foreach (var property in GetProperties(CollectionProperties, rowType))
        {
          var list = (IList)property.GetValue(row);
          var itemsToRemove = new List<object>();
          var foreignTable = GetTableForType(list.GetType().GetGenericArguments()[0]);
          foreach (object foreignValue in list)
          {
            if (foreignTable == null)
            {
              throw new InvalidOperationException("This mock database doesn't have a table for type: " + foreignValue.GetType().Name);
            }

            if (foreignValue == null)
            {
              continue;
            }

            if (collectionItemAction(row, property, foreignValue))
            {
              if (!CheckManyManyBackReference(row, property, foreignValue))
              {
                ManageRow(foreignValue, rowAction, collectionItemAction, navigationItemAction);
              }
            }
            else
            {
              itemsToRemove.Add(foreignValue);
            }
          }
          foreach (var item in itemsToRemove)
          {
            list.Remove(item);
          }
        }
      }
      if (navigationItemAction != null)
      {
        foreach (var property in GetProperties(NavigationProperties, rowType))
        {
          var foreignKeyAttribute = property.GetCustomAttribute<ForeignKeyAttribute>();
          navigationItemAction(row, property, (foreignKeyAttribute == null) ? null : rowType.GetProperty(foreignKeyAttribute.Name));
        }
      }
    }

    private static bool CheckManyManyBackReference(object row, PropertyInfo property, object foreignValue)
    {
      bool alreadyKnownLink = false;

      var manyMap = ManyToManyAssociations.SingleOrDefault(f => f.LeftProperty == property);
      if (manyMap != null)
      {
        var backReferences = (IList)manyMap.RightProperty.GetValue(foreignValue);
        if (backReferences == null)
        {
          throw new NotImplementedException(
            string.Format("Please initialize collection {0}.{1}.",
              manyMap.RightProperty.DeclaringType?.Name,
              manyMap.RightProperty.Name));
        }
        alreadyKnownLink = backReferences.Contains(row);
        if (!alreadyKnownLink)
        {
          backReferences.Add(row);
        }
      }

      manyMap = ManyToManyAssociations.SingleOrDefault(f => f.RightProperty == property);
      if (manyMap != null)
      {
        var backReferences = (IList)manyMap.LeftProperty.GetValue(foreignValue);
        if (backReferences == null)
        {
          throw new NotImplementedException(
            string.Format("Please initialize collection {0}.{1}.",
              manyMap.LeftProperty.DeclaringType?.Name,
              manyMap.LeftProperty.Name));
        }
        alreadyKnownLink = backReferences.Contains(row);
        if (!alreadyKnownLink)
        {
          backReferences.Add(row);
          alreadyKnownLink = true;
        }
      }

      return alreadyKnownLink;
    }

    private void DebugLine(string format, params object[] args)
    {
      if (Debug)
      {
        Console.WriteLine(format, args);
      }
    }

    private static long GetId(object row)
    {
      return Convert.ToInt64(row.GetType().GetProperty("Id").GetValue(row));
    }

    private class ManyManyMap
    {
      public Type LeftType { get; set; }
      public Type RightType { get; set; }
      public PropertyInfo LeftProperty { get; set; }
      public PropertyInfo RightProperty { get; set; }

      private ManyManyMap()
      {
      }

      public static ManyManyMap Create<TLeft, TRight>(
        Expression<Func<TLeft, ICollection<TRight>>> leftProperty,
        Expression<Func<TRight, ICollection<TLeft>>> rightProperty)
      {
        return new ManyManyMap
        {
          LeftType = typeof(TLeft),
          RightType = typeof(TRight),
          LeftProperty = (PropertyInfo)((MemberExpression)leftProperty.Body).Member,
          RightProperty = (PropertyInfo)((MemberExpression)rightProperty.Body).Member
        };
      }
    }
  }
}
