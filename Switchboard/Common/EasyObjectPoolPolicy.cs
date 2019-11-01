using System;
using Microsoft.Extensions.ObjectPool;

namespace Switchboard.Common
{
    public class EasyObjectPoolPolicy<T> : IPooledObjectPolicy<T>
    {
        private static readonly Func<T, bool> DefaultReturnFunc = _ => true;
        private readonly Func<T> _createFunc;

        private readonly Func<T, bool> _returnFunc;

        public EasyObjectPoolPolicy(Func<T> createFunc, Func<T, bool> returnFunc)
        {
            _createFunc = createFunc;
            _returnFunc = returnFunc;
        }

        public EasyObjectPoolPolicy(Func<T> createFunc) : this(createFunc, DefaultReturnFunc)
        {
        }

        public T Create()
        {
            return _createFunc();
        }

        public bool Return(T obj)
        {
            return _returnFunc(obj);
        }
    }
}