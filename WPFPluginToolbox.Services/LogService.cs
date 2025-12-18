using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using WPFPluginToolbox.Core;

namespace WPFPluginToolbox.Services
{
    /// <summary>
    /// 日志服务，提供调试信息记录和显示功能
    /// </summary>
    public class LogService : INotifyPropertyChanged
    {
        private readonly ObservableCollection<LogEntry> _logEntries = [];
        private int _maxLogEntries = 1000;
        
        /// <summary>
        /// 日志条目集合
        /// </summary>
        public ObservableCollection<LogEntry> LogEntries
        {
            get { return _logEntries; }
        }
        
        /// <summary>
        /// 最大日志条目数
        /// </summary>
        public int MaxLogEntries
        {
            get { return _maxLogEntries; }
            set
            {
                if (_maxLogEntries != value)
                {
                    _maxLogEntries = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxLogEntries)));
                    
                    // 裁剪日志条目
                    TrimLogEntries();
                }
            }
        }
        
        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// 日志记录事件
        /// </summary>
        public event EventHandler<LogEntry>? LogRecorded;
        
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">调试信息</param>
        /// <param name="level">调试级别</param>
        public void LogDebugInfo(string message, DebugLevel level = DebugLevel.Info)
        {
            var logEntry = new LogEntry
            {
                Message = message,
                Level = level,
                Timestamp = DateTime.Now
            };
            
            // 添加到日志条目集合
            _logEntries.Add(logEntry);
            
            // 裁剪日志条目
            TrimLogEntries();
            
            // 触发日志记录事件
            LogRecorded?.Invoke(this, logEntry);
        }
        
        /// <summary>
        /// 记录信息级别的调试信息
        /// </summary>
        /// <param name="message">调试信息</param>
        public void Info(string message)
        {
            LogDebugInfo(message, DebugLevel.Info);
        }
        
        /// <summary>
        /// 记录警告级别的调试信息
        /// </summary>
        /// <param name="message">调试信息</param>
        public void Warning(string message)
        {
            LogDebugInfo(message, DebugLevel.Warning);
        }
        
        /// <summary>
        /// 记录错误级别的调试信息
        /// </summary>
        /// <param name="message">调试信息</param>
        public void Error(string message)
        {
            LogDebugInfo(message, DebugLevel.Error);
        }
        
        /// <summary>
        /// 记录调试级别的调试信息
        /// </summary>
        /// <param name="message">调试信息</param>
        public void Debug(string message)
        {
            LogDebugInfo(message, DebugLevel.Debug);
        }
        
        /// <summary>
        /// 清空所有日志条目
        /// </summary>
        public void ClearLogs()
        {
            _logEntries.Clear();
        }
        
        /// <summary>
        /// 裁剪日志条目，只保留指定数量的最新条目
        /// </summary>
        private void TrimLogEntries()
        {
            while (_logEntries.Count > _maxLogEntries)
            {
                _logEntries.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// 获取指定级别的日志条目
        /// </summary>
        /// <param name="level">调试级别</param>
        /// <returns>指定级别的日志条目列表</returns>
        public List<LogEntry> GetLogsByLevel(DebugLevel level)
        {
            var logs = new List<LogEntry>();
            foreach (var entry in _logEntries)
            {
                if (entry.Level == level)
                {
                    logs.Add(entry);
                }
            }
            return logs;
        }
    }
    
    /// <summary>
    /// 日志条目类
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// 日志消息
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// 调试级别
        /// </summary>
        public DebugLevel Level { get; set; }
        
        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 重写ToString方法，返回格式化的日志条目
        /// </summary>
        /// <returns>格式化的日志条目</returns>
        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] {Message}";
        }
    }
}