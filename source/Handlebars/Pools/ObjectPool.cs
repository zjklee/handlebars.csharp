using System.Collections.Concurrent;
using HandlebarsDotNet.Runtime;

namespace HandlebarsDotNet.Pools
{
    internal static class ObjectPoolExtensions
    {
        public static DisposableContainer<T, InternalObjectPool<T, TPolicy>> Use<T, TPolicy>(this InternalObjectPool<T, TPolicy> objectPool) 
            where T : class where TPolicy : IInternalObjectPoolPolicy<T>
        {
            return new DisposableContainer<T, InternalObjectPool<T, TPolicy>>(
                objectPool.Get(), 
                objectPool, 
                (item, pool) => pool.Return(item)
            );
        }
    }

    internal class InternalObjectPool<T, TPolicy>
        where TPolicy: IInternalObjectPoolPolicy<T>
        where T: class
    {
        private readonly TPolicy _policy;
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public InternalObjectPool(TPolicy policy)
        {
            _policy = policy;
    
            for (var i = 0; i < 5; i++) Return(_policy.Create());
        }
    
        public T Get()
        {
            if (_queue.TryDequeue(out var item))
            {
                return item;
            }
    
            return _policy.Create();
        }
    
        public void Return(T obj)
        {
            if (!_policy.Return(obj)) return;
            _queue.Enqueue(obj);
        }
    }

    internal interface IInternalObjectPoolPolicy<T>
    {
        T Create();
        bool Return(T item);
    }
}