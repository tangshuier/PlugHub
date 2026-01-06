using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace WPFPluginToolbox.Services
{
    /// <summary>
    /// 插件性能数据类
    /// </summary>
    public class PluginPerformanceData
    {
        /// <summary>
        /// 插件ID
        /// </summary>
        public string PluginId { get; set; }
        
        /// <summary>
        /// 插件名称
        /// </summary>
        public string PluginName { get; set; }
        
        /// <summary>
        /// 内存使用量（字节）
        /// </summary>
        public long MemoryUsage { get; set; }
        
        /// <summary>
        /// 操作执行次数
        /// </summary>
        public long OperationCount { get; set; }
        
        /// <summary>
        /// 最近一次操作的执行时间
        /// </summary>
        public TimeSpan LastOperationDuration { get; set; }
        
        /// <summary>
        /// 平均操作执行时间
        /// </summary>
        public TimeSpan AverageOperationDuration { get; set; }
        
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 性能监控服务，用于监控插件性能
    /// </summary>
    public class PerformanceMonitor
    {
        private readonly Dictionary<string, PluginPerformanceData> _performanceData = new();
        private readonly Dictionary<string, Stopwatch> _operationTimers = new();
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly Process _currentProcess;
        private readonly long _processId;

        /// <summary>
        /// 性能数据更新事件
        /// </summary>
        public event EventHandler<PluginPerformanceData>? PerformanceUpdated;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PerformanceMonitor()
        {
            _currentProcess = Process.GetCurrentProcess();
            _processId = _currentProcess.Id;
        }

        /// <summary>
        /// 开始监控插件性能
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <param name="pluginName">插件名称</param>
        public void StartMonitoring(string pluginId, string pluginName)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_performanceData.ContainsKey(pluginId))
                {
                    _performanceData[pluginId] = new PluginPerformanceData
                    {
                        PluginId = pluginId,
                        PluginName = pluginName,
                        MemoryUsage = 0,
                        OperationCount = 0,
                        LastOperationDuration = TimeSpan.Zero,
                        AverageOperationDuration = TimeSpan.Zero,
                        LastUpdateTime = DateTime.Now
                    };
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 停止监控插件性能
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        public void StopMonitoring(string pluginId)
        {
            _lock.EnterWriteLock();
            try
            {
                _performanceData.Remove(pluginId);
                _operationTimers.Remove(pluginId);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 开始计时操作
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        public void StartOperationTimer(string pluginId)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_operationTimers.ContainsKey(pluginId))
                {
                    _operationTimers[pluginId] = Stopwatch.StartNew();
                }
                else
                {
                    _operationTimers[pluginId].Restart();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 停止计时并记录操作
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        public void StopOperationTimer(string pluginId)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_operationTimers.TryGetValue(pluginId, out var timer))
                {
                    timer.Stop();
                    UpdatePerformanceData(pluginId, timer.Elapsed);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 更新性能数据
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <param name="operationDuration">操作执行时间</param>
        private void UpdatePerformanceData(string pluginId, TimeSpan operationDuration)
        {
            if (_performanceData.TryGetValue(pluginId, out var perfData))
            {
                perfData.OperationCount++;
                perfData.LastOperationDuration = operationDuration;
                
                // 更新平均操作时间
                var totalDuration = perfData.AverageOperationDuration.Ticks * (perfData.OperationCount - 1) + operationDuration.Ticks;
                perfData.AverageOperationDuration = TimeSpan.FromTicks(totalDuration / perfData.OperationCount);
                
                // 更新内存使用量
                _currentProcess.Refresh();
                perfData.MemoryUsage = _currentProcess.WorkingSet64;
                
                perfData.LastUpdateTime = DateTime.Now;
                
                // 触发性能更新事件
                PerformanceUpdated?.Invoke(this, perfData);
            }
        }

        /// <summary>
        /// 获取插件性能数据
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <returns>插件性能数据，如果未监控则返回null</returns>
        public PluginPerformanceData? GetPluginPerformance(string pluginId)
        {
            _lock.EnterReadLock();
            try
            {
                _performanceData.TryGetValue(pluginId, out var data);
                return data;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// 获取所有插件的性能数据
        /// </summary>
        /// <returns>所有插件的性能数据列表</returns>
        public List<PluginPerformanceData> GetAllPluginPerformance()
        {
            _lock.EnterReadLock();
            try
            {
                return new List<PluginPerformanceData>(_performanceData.Values);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}