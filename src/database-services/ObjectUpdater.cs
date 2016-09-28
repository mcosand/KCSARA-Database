using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kcsar.Database.Model;

namespace Sar.Database.Services
{
  public class ObjectUpdater
  {
    public static async Task<ObjectUpdater<T>> CreateUpdater<T>(IDbSet<T> dbSet, Guid id, Action<T> initNew) where T : ModelObject, new()
    {
      var existing = await dbSet.FirstOrDefaultAsync(f => f.Id == id);
      if (existing == null)
      {
        existing = new T();
        initNew(existing);
        dbSet.Add(existing);
      }
      
      return new ObjectUpdater<T>(existing);
    }
  }

  public class ObjectUpdater<T> where T : ModelObject
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

    public async Task<bool> Persist(IKcsarContext db)
    {
      if (_changes.Count == 0) return false;

      await db.SaveChangesAsync();
      return true;
    }
  }
}
