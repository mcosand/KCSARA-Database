/*
 * Copyright 2013-2014 Matthew Cosand
 * Adapted from https://gist.github.com/troufster/913659
 */
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Internal.Database.Model
{
  public class InMemoryDbSet<T> : IDbSet<T> where T : class
  {

    readonly HashSet<T> _set;
    public IQueryable<T> QueryableSet { get; private set; }


    public InMemoryDbSet() : this(Enumerable.Empty<T>()) { }

    public InMemoryDbSet(IEnumerable<T> entities)
    {
      _set = new HashSet<T>();

      foreach (var entity in entities)
      {
        _set.Add(entity);
      }

      QueryableSet = _set.AsQueryable();
    }

    public T Add(T entity)
    {
      _set.Add(entity);
      return entity;

    }

    public T Attach(T entity)
    {
      _set.Add(entity);
      return entity;
    }

    public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
    {
      throw new NotImplementedException();
    }

    public T Create()
    {
      throw new NotImplementedException();
    }

    public T Find(params object[] keyValues)
    {
      throw new NotImplementedException();
    }

    public System.Collections.ObjectModel.ObservableCollection<T> Local
    {
      get { throw new NotImplementedException(); }
    }

    public T Remove(T entity)
    {
      _set.Remove(entity);
      return entity;
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _set.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public Type ElementType
    {
      get { return QueryableSet.ElementType; }
    }

    public System.Linq.Expressions.Expression Expression
    {
      get { return QueryableSet.Expression; }
    }

    public IQueryProvider Provider
    {
      get { return QueryableSet.Provider; }
    }
  }
}
