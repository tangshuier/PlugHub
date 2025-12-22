using System.IO;
using System.Text.Json;
using WPFPluginToolbox.Core;
using WPFPluginToolbox.Services.Models;

namespace WPFPluginToolbox.Services
{
    /// <summary>
    /// 设置服务类，用于处理设置的加载和保存
    /// </summary>
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            IgnoreReadOnlyProperties = true
        };
        
        public SettingsService()
        {
            // 获取设置文件路径
            string appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            _settingsFilePath = Path.Combine(appDataPath, "ToolboxSettings.json");
        }
        
        /// <summary>
        /// 获取设置
        /// </summary>
        /// <returns>工具箱设置</returns>
        public ToolboxSettings GetSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    return JsonSerializer.Deserialize<ToolboxSettings>(json) ?? GetDefaultSettings();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载设置失败: {ex.Message}");
                    return GetDefaultSettings();
                }
            }
            
            // 返回默认设置
            return GetDefaultSettings();
        }
        
        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="settings">要保存的设置</param>
        public void SaveSettings(ToolboxSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, _jsonSerializerOptions);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存设置失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 获取默认设置
        /// </summary>
        /// <returns>默认设置</returns>
        private static ToolboxSettings GetDefaultSettings()
        {
            return new ToolboxSettings
            {
                Theme = ToolboxTheme.Black,
                IsDebugWindowDefaultOpen = false
            };
        }
    }
}