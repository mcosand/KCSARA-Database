using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Sar.Database.Data
{
  public class ObjectUpdater
  {
    public static async Task<ObjectUpdater<T>> CreateUpdater<T>(IDbSet<T> dbSet, Guid id, Action<T> initNew) where T : class, IRowWithId, new()
    {
      var existing = await dbSet.FirstOrDefaultAsync(f => f.Id == id);
      if (existing == null)
      {
        existing = new T();
        initNew?.Invoke(existing);
        dbSet.Add(existing);
      }
      
      return new ObjectUpdater<T>(existing);
    }
  }

  public class ObjectUpdater<T>
  {
    private List<string> _changes = new List<string>();

    public T Instance { get; private set; }

    public ObjectUpdater(T instance)
    {
      Instance = instance;
    }

    public void Update<TProperty>(Expression<Func<T, TProperty>> property, TProperty newValue)
    {
      var propertyInfo = (property.Body as MemberExpression)?.Member as PropertyInfo;
      if (propertyInfo == null)
      {
        throw new ArgumentException("should be f => f.PropertyName", "property");
      }

      var existing = propertyInfo.GetValue(Instance);
      if (!Equals(existing, newValue))
      {
        _changes.Add($"{propertyInfo.Name}: [{existing}] => [{newValue}]");
        propertyInfo.SetValue(Instance, newValue);
      }
    }

    public async Task<bool> Persist(IDbContext db)
    {
      if (_changes.Count == 0) return false;

      await db.SaveChangesAsync();
      return true;
    }
  }
}
