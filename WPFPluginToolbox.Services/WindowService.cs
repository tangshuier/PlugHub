using System;
using System.Collections.Generic;
using System.Windows;

namespace WPFPluginToolbox.Services
{
    /// <summary>
    /// 窗口服务，提供窗口创建和管理功能
    /// </summary>
    public class WindowService
    {
        private readonly List<Window> _createdWindows = new List<Window>();
        
        /// <summary>
        /// 创建新窗口
        /// </summary>
        /// <param name="title">窗口标题</param>
        /// <param name="content">窗口内容</param>
        /// <returns>创建的窗口实例</returns>
        public Window CreateWindow(string title, object content)
        {
            try
            {
                var window = new Window
                {
                    Title = title,
                    Content = content,
                    Width = 800,
                    Height = 600,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };
                
                // 添加窗口关闭事件处理
                window.Closed += Window_Closed;
                
                // 添加到已创建窗口列表
                _createdWindows.Add(window);
                
                return window;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"创建窗口失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 创建模态窗口
        /// </summary>
        /// <param name="title">窗口标题</param>
        /// <param name="content">窗口内容</param>
        /// <returns>窗口对话框结果</returns>
        public bool? ShowModalWindow(string title, object content)
        {
            var window = CreateWindow(title, content);
            return window.ShowDialog();
        }
        
        /// <summary>
        /// 显示非模态窗口
        /// </summary>
        /// <param name="title">窗口标题</param>
        /// <param name="content">窗口内容</param>
        /// <returns>创建的窗口实例</returns>
        public Window ShowNonModalWindow(string title, object content)
        {
            var window = CreateWindow(title, content);
            window.Show();
            return window;
        }
        
        /// <summary>
        /// 关闭所有创建的窗口
        /// </summary>
        public void CloseAllWindows()
        {
            // 创建副本以避免修改正在迭代的集合
            var windowsToClose = new List<Window>(_createdWindows);
            foreach (var window in windowsToClose)
            {
                window.Close();
            }
        }
        
        /// <summary>
        /// 获取所有创建的窗口
        /// </summary>
        /// <returns>创建的窗口列表</returns>
        public List<Window> GetAllCreatedWindows()
        {
            return new List<Window>(_createdWindows);
        }
        
        /// <summary>
        /// 窗口关闭事件处理程序
        /// </summary>
        /// <param name="sender">事件发送者</param>
        /// <param name="e">事件参数</param>
        private void Window_Closed(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                // 从已创建窗口列表中移除
                _createdWindows.Remove(window);
                
                // 移除事件处理程序
                window.Closed -= Window_Closed;
            }
        }
        
        /// <summary>
        /// 创建消息框
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="title">消息框标题</param>
        /// <param name="button">消息框按钮</param>
        /// <param name="icon">消息框图标</param>
        /// <returns>消息框结果</returns>
        public MessageBoxResult ShowMessageBox(string message, string title = "消息", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
        {
            return MessageBox.Show(message, title, button, icon);
        }
    }
}