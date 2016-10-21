using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Internal.Data
{
  public interface IFakeDbSet : IEnumerable
  {
    bool Contains(object o);
    void Add(object newRow);
    void Clear();
    Type ElementType { get; }
  }

  // https://gist.github.com/taschmidt/9663503
  public class FakeDbSet<T> : IFakeDbSet, IDbSet<T>, IDbAsyncEnumerable<T> where T : class
  {
    readonly ObservableCollection<T> _data;
    readonly IQueryable _queryable;

    public FakeDbSet()
    {
      _data = new ObservableCollection<T>();
      _queryable = _data.AsQueryable();
    }

    public virtual T Find(params object[] keyValues)
    {
      throw new NotImplementedException("Derive from FakeDbSet<T> and override Find");
    }

    public Task<T> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
    {
      throw new NotImplementedException();
    }

    public T Add(T item)
    {
      _data.Add(item);
      return item;
    }

    public T Remove(T item)
    {
      _data.Remove(item);
      return item;
    }

    public T Attach(T item)
    {
      _data.Add(item);
      return item;
    }

    public T Detach(T item)
    {
      _data.Remove(item);
      return item;
    }

    public T Create()
    {
      return Activator.CreateInstance<T>();
    }

    public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
    {
      return Activator.CreateInstance<TDerivedEntity>();
    }

    public ObservableCollection<T> Local
    {
      get { return _data; }
    }

    Type IFakeDbSet.ElementType
    {
      get { return _queryable.ElementType; }
    }

    void IFakeDbSet.Clear()
    {
      _data.Clear();
    }

    

    Type IQueryable.ElementType
    {
      get { return _queryable.ElementType; }
    }

    Expression IQueryable.Expression
    {
      get { return _queryable.Expression; }
    }

    IQueryProvider IQueryable.Provider
    {
      get { return new AsyncQueryProviderWrapper<T>(_queryable.Provider); }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _data.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _data.GetEnumerator();
    }

    public int Count
    {
      get { return _data.Count; }
    }

    public IDbAsyncEnumerator<T> GetAsyncEnumerator()
    {
      return new AsyncEnumeratorWrapper<T>(_data.GetEnumerator());
    }

    IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
    {
      return GetAsyncEnumerator();
    }

    bool IFakeDbSet.Contains(object o)
    {
      return _data.Contains(o);
    }

    void IFakeDbSet.Add(object newRow)
    {
      _data.Add((T)newRow);
    }
  }

  internal class AsyncQueryProviderWrapper<T> : IDbAsyncQueryProvider
  {
    private readonly IQueryProvider _inner;

    internal AsyncQueryProviderWrapper(IQueryProvider inner)
    {
      _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
      return new AsyncEnumerableQuery<T>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
      return new AsyncEnumerableQuery<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
      return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
      return _inner.Execute<TResult>(expression);
    }

    public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
    {
      return Task.FromResult(Execute(expression));
    }

    public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
    {
      return Task.FromResult(Execute<TResult>(expression));
    }
  }

  public class AsyncEnumerableQuery<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>, IQueryable
  {
    public AsyncEnumerableQuery(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    public AsyncEnumerableQuery(Expression expression) : base(expression)
    {
    }

    public IDbAsyncEnumerator<T> GetAsyncEnumerator()
    {
      return new AsyncEnumeratorWrapper<T>(this.AsEnumerable().GetEnumerator());
    }

    IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
    {
      return GetAsyncEnumerator();
    }

    IQueryProvider IQueryable.Provider
    {
      get { return new AsyncQueryProviderWrapper<T>(this); }
    }
  }

  public class AsyncEnumeratorWrapper<T> : IDbAsyncEnumerator<T>
  {
    private readonly IEnumerator<T> _inner;

    public AsyncEnumeratorWrapper(IEnumerator<T> inner)
    {
      _inner = inner;
    }

    public void Dispose()
    {
      _inner.Dispose();
    }

    public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
    {
      return Task.FromResult(_inner.MoveNext());
    }

    public T Current
    {
      get { return _inner.Current; }
    }

    object IDbAsyncEnumerator.Current
    {
      get { return Current; }
    }
  }
}