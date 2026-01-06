using System.Collections.Generic;
using System.Threading;

namespace WPFPluginToolbox.Services
{
    /// <summary>
    /// 简单的数据共享服务，用于插件间数据传递
    /// </summary>
    public class SimpleDataShareService
    {
        private readonly Dictionary<string, object> _sharedData = new();
        private readonly ReaderWriterLockSlim _lock = new();

        /// <summary>
        /// 存储共享数据
        /// </summary>
        /// <param name="key">数据键名</param>
        /// <param name="data">要存储的数据</param>
        public void SetData(string key, object data)
        {
            _lock.EnterWriteLock();
            try
            {
                _sharedData[key] = data;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 获取共享数据（带类型转换）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">数据键名</param>
        /// <returns>转换后的数据，如果转换失败则返回默认值</returns>
        public T? GetData<T>(string key)
        {
            _lock.EnterReadLock();
            try
            {
                if (_sharedData.TryGetValue(key, out var data))
                {
                    return data is T t ? t : default;
                }
                return default;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 检查是否存在指定键的数据
        /// </summary>
        /// <param name="key">数据键名</param>
        /// <returns>是否存在数据</returns>
        public bool ContainsData(string key)
        {
            _lock.EnterReadLock();
            try
            {
                return _sharedData.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 删除指定键的数据
        /// </summary>
        /// <param name="key">数据键名</param>
        public void RemoveData(string key)
        {
            _lock.EnterWriteLock();
            try
            {
                _sharedData.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 清除所有共享数据
        /// </summary>
        public void ClearAllData()
        {
            _lock.EnterWriteLock();
            try
            {
                _sharedData.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}