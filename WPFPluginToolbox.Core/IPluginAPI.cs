using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFPluginToolbox.Core
{
    /// <summary>
    /// 插件API接口，为插件提供访问工具箱功能的API
    /// </summary>
    public interface IPluginAPI
    {
        #region 基础信息
        
        /// <summary>
        /// 获取插件ID
        /// </summary>
        string PluginId { get; }
        
        /// <summary>
        /// 获取插件名称
        /// </summary>
        string PluginName { get; }
        
        /// <summary>
        /// 获取插件路径
        /// </summary>
        string PluginPath { get; }
        
        #endregion
        
        #region 日志记录
        
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="data">附加数据</param>
        void Debug(string message, object? data = null);
        
        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="data">附加数据</param>
        void Info(string message, object? data = null);
        
        /// <summary>
        /// 记录警告
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="data">附加数据</param>
        void Warn(string message, object? data = null);
        
        /// <summary>
        /// 记录错误
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        void Error(string message, Exception? exception = null);
        
        #endregion
        
        #region 文件操作
        
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件内容</returns>
        Task<string> ReadFileAsync(string path);
        
        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="content">文件内容</param>
        Task WriteFileAsync(string path, string content);
        
        /// <summary>
        /// 创建文件
        /// </summary>
        /// <param name="path">文件路径</param>
        Task CreateFileAsync(string path);
        
        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件是否存在</returns>
        bool FileExists(string path);
        
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path">文件路径</param>
        Task DeleteFileAsync(string path);
        
        /// <summary>
        /// 创建目录
        /// </summary>
        /// <param name="path">目录路径</param>
        Task CreateDirectoryAsync(string path);
        
        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <returns>目录是否存在</returns>
        bool DirectoryExists(string path);
        
        /// <summary>
        /// 检索文件
        /// </summary>
        /// <param name="directory">目录路径</param>
        /// <param name="pattern">搜索模式</param>
        /// <param name="recursive">是否递归搜索</param>
        /// <returns>匹配的文件列表</returns>
        IEnumerable<string> SearchFiles(string directory, string pattern, bool recursive = false);
        
        #endregion
        
        #region 窗口操作
        
        /// <summary>
        /// 创建窗口
        /// </summary>
        /// <param name="title">窗口标题</param>
        /// <param name="content">窗口内容</param>
        /// <param name="isModal">是否为模态窗口</param>
        /// <returns>创建的窗口</returns>
        Window CreateWindow(string title, UserControl content, bool isModal = false);
        
        /// <summary>
        /// 显示窗口
        /// </summary>
        /// <param name="window">要显示的窗口</param>
        void ShowWindow(Window window);
        
        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="window">要关闭的窗口</param>
        void CloseWindow(Window window);
        
        #endregion
        
        #region 插件操作
        
        /// <summary>
        /// 获取依赖插件
        /// </summary>
        /// <param name="pluginId">依赖插件ID</param>
        /// <returns>依赖插件实例，不存在则返回null</returns>
        IDependency? GetDependency(string pluginId);
        
        /// <summary>
        /// 检查是否存在指定依赖
        /// </summary>
        /// <param name="pluginId">依赖插件ID</param>
        /// <returns>是否存在依赖</returns>
        bool HasDependency(string pluginId);
        
        #endregion
        
        #region 配置操作
        
        /// <summary>
        /// 获取配置目录路径
        /// </summary>
        string ConfigDirectory { get; }
        
        /// <summary>
        /// 获取插件配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="defaultConfig">默认配置</param>
        /// <returns>配置对象</returns>
        T GetConfig<T>(T defaultConfig) where T : class;
        
        /// <summary>
        /// 保存插件配置
        /// </summary>
        /// <typeparam name="T">配置类型</typeparam>
        /// <param name="config">配置对象</param>
        /// <returns>任务</returns>
        Task SaveConfigAsync<T>(T config) where T : class;
        
        /// <summary>
        /// 检查是否存在配置文件
        /// </summary>
        /// <returns>是否存在配置文件</returns>
        bool HasConfig();
        
        #endregion
        
        #region 主题相关
        
        /// <summary>
        /// 获取当前主题
        /// </summary>
        ToolboxTheme CurrentTheme { get; }
        
        /// <summary>
        /// 获取当前主题的背景色
        /// </summary>
        Brush CurrentBackgroundBrush { get; }
        
        /// <summary>
        /// 获取当前主题的前景色
        /// </summary>
        Brush CurrentForegroundBrush { get; }
        
        /// <summary>
        /// 主题变更事件
        /// </summary>
        event EventHandler<ToolboxTheme>? ThemeChanged;
        
        /// <summary>
        /// 指示插件是否同步工具箱主题
        /// </summary>
        bool SyncToolboxTheme { get; set; }
        
        #endregion
    }
}
