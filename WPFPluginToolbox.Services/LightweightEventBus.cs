using System;
using System.Collections.Generic;
using System.Threading;

namespace WPFPluginToolbox.Services
{
    /// <summary>
    /// 轻量级事件总线，用于插件间事件通信
    /// </summary>
    public class LightweightEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _eventHandlers = new();
        private readonly ReaderWriterLockSlim _lock = new();

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="event">要发布的事件</param>
        public void Publish<TEvent>(TEvent @event) where TEvent : class
        {
            _lock.EnterReadLock();
            try
            {
                var eventType = typeof(TEvent);
                if (_eventHandlers.TryGetValue(eventType, out var handlers))
                {
                    // 复制列表以避免并发修改问题
                    var handlersCopy = new List<Delegate>(handlers);
                    _lock.ExitReadLock();

                    // 在锁外执行事件处理，避免死锁
                    foreach (var handler in handlersCopy)
                    {
                        if (handler is Action<TEvent> typedHandler)
                        {
                            typedHandler(@event);
                        }
                    }
                    return;
                }
            }
            finally
            {
                if (_lock.IsReadLockHeld)
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">事件处理程序</param>
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            _lock.EnterWriteLock();
            try
            {
                var eventType = typeof(TEvent);
                if (!_eventHandlers.ContainsKey(eventType))
                {
                    _eventHandlers[eventType] = new List<Delegate>();
                }
                _eventHandlers[eventType].Add(handler);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        /// <typeparam name="TEvent">事件类型</typeparam>
        /// <param name="handler">要取消的事件处理程序</param>
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            _lock.EnterWriteLock();
            try
            {
                var eventType = typeof(TEvent);
                if (_eventHandlers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(handler);
                    if (handlers.Count == 0)
                    {
                        _eventHandlers.Remove(eventType);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 取消所有订阅
        /// </summary>
        public void ClearAllSubscriptions()
        {
            _lock.EnterWriteLock();
            try
            {
                _eventHandlers.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
    
    /// <summary>
    /// 插件错误事件，用于插件异常通知
    /// </summary>
    public class PluginErrorEvent
    {
        /// <summary>
        /// 插件ID
        /// </summary>
        public string PluginId { get; }
        
        /// <summary>
        /// 错误消息
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// 异常对象
        /// </summary>
        public Exception? Exception { get; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <param name="message">错误消息</param>
        /// <param name="exception">异常对象</param>
        public PluginErrorEvent(string pluginId, string message, Exception? exception = null)
        {
            PluginId = pluginId;
            Message = message;
            Exception = exception;
        }
    }
    
    /// <summary>
    /// 配置变更事件，用于通知配置文件变更
    /// </summary>
    public class ConfigChangedEvent
    {
        /// <summary>
        /// 插件ID
        /// </summary>
        public string PluginId { get; }
        
        /// <summary>
        /// 变更类型
        /// </summary>
        public string ChangeType { get; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <param name="changeType">变更类型</param>
        public ConfigChangedEvent(string pluginId, string changeType)
        {
            PluginId = pluginId;
            ChangeType = changeType;
        }
    }
}