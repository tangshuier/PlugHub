using System;
using System.IO;
using System.Collections.Generic;

namespace WPFPluginToolbox.Services
{
    /// <summary>
    /// 文件操作服务，提供文件读写、检索和创建功能
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件内容</returns>
        public string ReadFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }
                else
                {
                    throw new FileNotFoundException($"文件不存在: {filePath}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"读取文件失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 写入文件内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="content">要写入的内容</param>
        public void WriteFile(string filePath, string content)
        {
            try
            {
                // 确保目录存在
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"写入文件失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 创建新文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否创建成功</returns>
        public bool CreateFile(string filePath)
        {
            try
            {
                // 确保目录存在
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                if (!File.Exists(filePath))
                {
                    using (File.Create(filePath)) { }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"创建文件失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 检索文件
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <param name="searchPattern">搜索模式</param>
        /// <param name="searchOption">搜索选项</param>
        /// <returns>匹配的文件列表</returns>
        public List<string> SearchFiles(string directoryPath, string searchPattern, SearchOption searchOption)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    var files = Directory.GetFiles(directoryPath, searchPattern, searchOption);
                    return new List<string>(files);
                }
                else
                {
                    throw new DirectoryNotFoundException($"目录不存在: {directoryPath}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"检索文件失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件是否存在</returns>
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
        
        /// <summary>
        /// 检查目录是否存在
        /// </summary>
        /// <param name="directoryPath">目录路径</param>
        /// <returns>目录是否存在</returns>
        public bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
        
        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件信息</returns>
        public FileInfo GetFileInfo(string filePath)
        {
            if (File.Exists(filePath))
            {
                return new FileInfo(filePath);
            }
            else
            {
                throw new FileNotFoundException($"文件不存在: {filePath}");
            }
        }
    }
}