# WPF插件开发手册

## 1. 概述

本手册详细介绍了如何使用WPF插件工具箱开发插件，包括插件类型、开发流程、API使用和最佳实践。WPF插件工具箱支持插件UI多标签页切换，插件可以同步工具箱的主题，提供了完整的API支持。

## 2. 插件类型

WPF插件工具箱支持两种类型的插件：

### 2.1 普通插件（Plugin）

- **用途**：提供独立功能的插件，带有用户界面
- **特点**：
  - 有独立的用户界面
  - 可被用户选择和使用
  - 实现`IPlugin`接口

### 2.2 依赖插件（Dependency）

- **用途**：为其他插件提供API支持
- **特点**：
  - 没有用户界面
  - 仅提供功能接口
  - 实现`IDependency`接口
  - 被其他插件依赖

## 3. 开发环境准备

### 3.1 开发工具

- **Visual Studio 2022** 或更高版本
- **.NET 9.0 SDK** 或更高版本

### 3.2 插件加载机制

WPF插件工具箱使用 **AssemblyLoadContext** 进行插件加载，具有以下优点：
- 更好的插件隔离
- 支持热插拔
- 避免依赖冲突
- 更高效的资源管理

### 3.3 引用依赖

在插件项目中添加以下引用：

- **WPFPluginToolbox.Core.dll**：包含核心接口定义

## 4. 插件开发流程

### 4.1 创建插件项目

1. **创建类库项目**：
   - 打开Visual Studio
   - 选择「创建新项目」
   - 选择「类库（.NET）」模板
   - 设置框架为「.NET 9.0-windows」
   - 命名项目并选择保存位置

2. **配置项目**：
   - 右键点击项目 → 属性 → 应用程序
   - 确保目标框架为「.NET 9.0-windows」
   - 勾选「使用Windows SDK版本为目标框架或更高版本」

3. **添加引用**：
   - 右键点击「引用」→ 「添加引用」
   - 点击「浏览」→ 选择 `WPFPluginToolbox.Core.dll`
   - 点击「确定」添加引用

### 4.2 实现插件接口

#### 4.2.1 实现普通插件（IPlugin）

```csharp
using System;
using System.Windows.Controls;
using WPFPluginToolbox.Core;

namespace MyPlugin
{
    public class MyPlugin : IPlugin
    {
        private IPluginAPI? _pluginApi;
        private MyPluginView? _mainView;
        
        // 插件基本信息
        public string Id { get; } = "MyPlugin";
        public string Name { get; } = "我的插件";
        public string Description { get; } = "这是一个示例插件";
        public string Version { get; } = "1.0.0";
        public PluginType Type { get; } = PluginType.Plugin;

        // 初始化方法
        public void Initialize(IPluginAPI pluginApi)
        {
            _pluginApi = pluginApi;
            _pluginApi.Info($"{Name} 初始化完成");
            
            // 初始化资源
            _mainView = new MyPluginView();
        }

        // 激活方法
        public void Activate()
        {
            _pluginApi?.Info($"{Name} 激活");
            // 启动插件功能
        }

        // 停用方法
        public void Deactivate()
        {
            _pluginApi?.Info($"{Name} 停用");
            // 暂停插件功能
        }

        // 释放资源
        public void Dispose()
        {
            _pluginApi?.Info($"{Name} 释放资源");
            // 释放资源
            _mainView?.Dispose();
        }

        // 返回主视图
        public UserControl GetMainView()
        {
            return _mainView ?? throw new InvalidOperationException("插件主视图未初始化");
        }
    }
}
```

#### 4.2.2 实现依赖插件（IDependency）

```csharp
using System;
using WPFPluginToolbox.Core;

namespace MyDependency
{
    public class MyDependency : IDependency
    {
        // 依赖基本信息
        public string Id { get; } = "MyDependency";
        public string Name { get; } = "我的依赖";
        public string Description { get; } = "这是一个示例依赖插件";
        public string Version { get; } = "1.0.0";

        // 初始化方法
        public void Initialize()
        {
            Console.WriteLine($"{Name} 初始化完成");
        }

        // 释放资源
        public void Dispose()
        {
            Console.WriteLine($"{Name} 释放资源");
        }

        // 依赖提供的功能方法
        public string GetData()
        {
            return "来自依赖的数据";
        }
    }
}```

### 4.3 开发插件UI

1. **添加UserControl**：
   - 右键点击项目 → 「添加」→ 「UserControl (WPF)」
   - 命名为 `MyPluginView`

2. **设计UI**：
   - 使用XAML设计插件界面
   - 示例XAML：

```xaml
<UserControl x:Class="MyPlugin.MyPluginView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             mc:Ignorable="d"
             d:DesignHeight="450" d:DesignWidth="800">
    <Grid>
        <StackPanel Margin="20">
            <TextBlock FontSize="24" FontWeight="Bold" Margin="0,0,0,20">
                我的插件
            </TextBlock>
            <TextBlock Margin="0,0,0,10">这是一个示例插件界面</TextBlock>
            <Button Content="点击我" Click="Button_Click" Width="100" Height="30"/>
        </StackPanel>
    </Grid>
</UserControl>
```

3. **实现交互逻辑**：
   - 在代码隐藏文件中实现交互逻辑：

```csharp
using System.Windows;
using System.Windows.Controls;

namespace MyPlugin
{
    public partial class MyPluginView : UserControl
    {
        public MyPluginView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("插件按钮被点击！");
        }
    }
}
```

## 5. 插件API使用

### 5.1 日志记录

插件可以通过 `IPluginAPI` 接口记录不同级别的日志：

```csharp
// 调试信息
_pluginApi.Debug("调试信息");

// 普通信息
_pluginApi.Info("普通信息");

// 警告信息
_pluginApi.Warn("警告信息");

// 错误信息
_pluginApi.Error("错误信息", exception);
```

### 5.2 文件操作

插件可以使用API进行文件操作：

```csharp
// 异步写入文件
await _pluginApi.WriteFileAsync("test.txt", "Hello from plugin!");

// 异步读取文件
string content = await _pluginApi.ReadFileAsync("test.txt");

// 检查文件是否存在
if (_pluginApi.FileExists("test.txt"))
{
    _pluginApi.Info("文件存在");
}

// 异步删除文件
await _pluginApi.DeleteFileAsync("test.txt");

// 异步创建目录
await _pluginApi.CreateDirectoryAsync("testDir");

// 检查目录是否存在
if (_pluginApi.DirectoryExists("testDir"))
{
    _pluginApi.Info("目录存在");
}

// 检索文件
var files = _pluginApi.SearchFiles("testDir", "*.txt", recursive: true);
```

### 5.3 窗口操作

插件可以创建和管理窗口：

```csharp
// 创建非模态窗口
var window = _pluginApi.CreateWindow("我的窗口", new MyWindowView());
_pluginApi.ShowWindow(window);

// 关闭窗口
_pluginApi.CloseWindow(window);

// 创建模态窗口
var dialog = _pluginApi.CreateWindow("对话框", new DialogView(), isModal: true);
dialog.ShowDialog();
```

### 5.4 依赖管理

插件可以获取和检查依赖：

```csharp
// 检查依赖是否存在
if (_pluginApi.HasDependency("MyDependency"))
{
    _pluginApi.Info("依赖存在");
}

// 获取依赖实例
var dependency = _pluginApi.GetDependency("MyDependency");
if (dependency != null)
{
    // 使用依赖功能
    _pluginApi.Info($"获取到依赖: {dependency.Name}");
}
```

### 5.5 配置操作

插件可以使用API进行配置的读取和保存：

```csharp
// 定义配置类
public class MyPluginConfig
{
    public string Setting1 { get; set; } = "默认值1";
    public int Setting2 { get; set; } = 42;
    public bool Setting3 { get; set; } = true;
}

// 读取配置
var config = _pluginApi.GetConfig(new MyPluginConfig());
_pluginApi.Info($"读取到配置: Setting1={config.Setting1}, Setting2={config.Setting2}");

// 修改配置
config.Setting1 = "新值";
config.Setting2 = 100;

// 保存配置
await _pluginApi.SaveConfigAsync(config);
_pluginApi.Info("配置保存成功");

// 检查是否存在配置文件
if (_pluginApi.HasConfig())
{
    _pluginApi.Info("配置文件存在");
}

// 获取配置目录
string configDir = _pluginApi.ConfigDirectory;
_pluginApi.Info($"配置目录: {configDir}");
```

### 5.6 数据共享

插件可以使用API进行数据共享，与其他插件交换数据：

```csharp
// 存储共享数据
_pluginApi.ShareData("userSettings", new { Name = "John", Age = 30 });

// 获取共享数据
var userSettings = _pluginApi.GetSharedData<dynamic>("userSettings");
if (userSettings != null)
{
    _pluginApi.Info($"共享数据: Name={userSettings.Name}, Age={userSettings.Age}");
}

// 检查是否存在共享数据
if (_pluginApi.HasSharedData("userSettings"))
{
    _pluginApi.Info("共享数据存在");
}

// 删除共享数据
_pluginApi.RemoveSharedData("userSettings");
```

### 5.7 事件总线

插件可以使用事件总线发布和订阅事件：

```csharp
// 定义事件类
public class UserLoggedInEvent
{
    public string UserName { get; set; }
    public DateTime LoginTime { get; set; }
}

// 订阅事件
_pluginApi.SubscribeEvent<UserLoggedInEvent>(OnUserLoggedIn);

// 发布事件
_pluginApi.PublishEvent(new UserLoggedInEvent
{
    UserName = "admin",
    LoginTime = DateTime.Now
});

// 事件处理方法
private void OnUserLoggedIn(UserLoggedInEvent @event)
{
    _pluginApi.Info($"用户登录: {@event.UserName} 在 {@event.LoginTime}");
}

// 取消订阅事件
_pluginApi.UnsubscribeEvent<UserLoggedInEvent>(OnUserLoggedIn);
```

### 5.8 性能监控

插件可以使用API进行性能监控：

```csharp
// 开始计时操作
_pluginApi.StartOperationTimer("数据加载");

// 执行耗时操作
LoadLargeData();

// 停止计时并记录操作
_pluginApi.StopOperationTimer("数据加载");
```

### 5.9 主题相关

插件可以使用API获取和响应主题相关的信息，实现与工具箱主题的实时同步：

#### 5.9.1 主题基本操作

```csharp
// 获取当前主题
var currentTheme = _pluginApi.CurrentTheme;
_pluginApi.Info($"当前主题: {currentTheme}");

// 获取已保存的主题（从设置文件中获取）
var savedTheme = _pluginApi.SavedTheme;
_pluginApi.Info($"已保存主题: {savedTheme}");

// 获取当前主题的背景色
var backgroundBrush = _pluginApi.CurrentBackgroundBrush;

// 获取当前主题的前景色
var foregroundBrush = _pluginApi.CurrentForegroundBrush;

// 获取当前主题的插件面板背景色
var pluginPanelBrush = _pluginApi.PluginPanelBackgroundBrush;

// 获取当前主题的插件工作区背景色
var workspaceBrush = _pluginApi.PluginWorkspaceBackgroundBrush;

// 获取当前主题的边框颜色
var borderBrush = _pluginApi.BorderBrush;

// 获取当前主题的控件背景色
var controlBgBrush = _pluginApi.ControlBackgroundColor;

// 获取当前主题的控件前景色
var controlFgBrush = _pluginApi.ControlForegroundColor;

// 获取当前主题的强调色
var accentBrush = _pluginApi.AccentColor;

// 设置是否同步工具箱主题
_pluginApi.SyncToolboxTheme = true;
```

#### 5.9.2 主题同步实现

```csharp
// 订阅主题变更事件
_pluginApi.ThemeChanged += OnThemeChanged;

// 主题变更事件处理
private void OnThemeChanged(object sender, ToolboxTheme theme)
{
    _pluginApi.Info($"收到主题变更事件: {theme}");
    UpdatePluginTheme();
}

// 更新插件主题的方法
private void UpdatePluginTheme()
{
    try
    {
        // 获取主题颜色
        var mainBgBrush = _pluginApi.CurrentBackgroundBrush;
        var mainFgBrush = _pluginApi.CurrentForegroundBrush;
        var borderBrush = _pluginApi.BorderBrush;
        
        // 更新插件UI元素
        UpdateControlTheme(mainBgBrush, mainFgBrush, borderBrush);
    }
    catch (Exception ex)
    {
        _pluginApi.Error($"更新主题失败: {ex.Message}", ex);
    }
}

// 更新控件主题
private void UpdateControlTheme(Brush backgroundBrush, Brush foregroundBrush, Brush borderBrush)
{
    // 更新根元素
    this.Background = backgroundBrush;
    this.Foreground = foregroundBrush;
    
    // 更新具体控件
    if (TitleTextBlock != null)
        TitleTextBlock.Foreground = foregroundBrush;
        
    if (ContentBorder != null)
    {
        ContentBorder.Background = backgroundBrush;
        ContentBorder.BorderBrush = borderBrush;
    }
    
    // 更新其他控件...
}
```

#### 5.9.3 应用主题到元素

插件可以使用API将主题应用到指定元素：

```csharp
// 将当前主题应用到整个插件视图
_pluginApi.ApplyThemeToElement(this);

// 将当前主题应用到特定控件
_pluginApi.ApplyThemeToElement(myButton);
```

#### 5.9.4 获取主题画笔

插件可以使用API获取特定类型的主题画笔：

```csharp
// 获取背景画笔
var bgBrush = _pluginApi.GetThemeBrush("background");

// 获取前景画笔
var fgBrush = _pluginApi.GetThemeBrush("foreground");

// 获取边框画笔
var borderBrush = _pluginApi.GetThemeBrush("border");

// 获取强调色画笔
var accentBrush = _pluginApi.GetThemeBrush("accent");
```

#### 5.9.5 完整主题同步示例

```csharp
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPFPluginToolbox.Core;

namespace ThemeAwarePlugin
{
    public partial class ThemeAwareView : UserControl
    {
        private readonly IPluginAPI _pluginApi;

        public ThemeAwareView(IPluginAPI pluginApi)
        {
            InitializeComponent();
            _pluginApi = pluginApi;
            
            // 订阅主题变更事件
            _pluginApi.ThemeChanged += OnThemeChanged;
            
            // 初始化主题
            UpdatePluginTheme();
        }

        private void OnThemeChanged(object sender, ToolboxTheme theme)
        {
            _pluginApi.Debug($"主题变更为: {theme}");
            UpdatePluginTheme();
        }

        private void UpdatePluginTheme()
        {
            try
            {
                // 方法1: 自动应用主题到整个控件
                _pluginApi.ApplyThemeToElement(this);
                
                // 方法2: 手动更新主题（如果需要更精细的控制）
                // 获取主题颜色
                // var mainBgBrush = _pluginApi.CurrentBackgroundBrush;
                // var mainFgBrush = _pluginApi.CurrentForegroundBrush;
                // var borderBrush = _pluginApi.BorderBrush;
                
                // 更新具体控件
                // TitleTextBlock.Foreground = mainFgBrush;
                // ContentBorder.Background = mainBgBrush;
                // ContentBorder.BorderBrush = borderBrush;
                
                _pluginApi.Debug("主题更新完成");
            }
            catch (Exception ex)
            {
                _pluginApi.Error($"更新主题失败: {ex.Message}", ex);
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = $"按钮点击时间: {DateTime.Now}";
        }
    }
}
```

#### 5.9.6 主题同步最佳实践

1. **及时订阅事件**：在插件初始化时立即订阅ThemeChanged事件
2. **全面更新**：确保更新所有UI元素，包括背景、前景、边框等
3. **错误处理**：添加异常捕获，确保主题更新失败不会影响插件功能
4. **调试日志**：添加详细的调试日志，便于排查主题同步问题
5. **性能优化**：避免在主题更新时进行耗时操作
6. **视觉一致性**：确保插件UI与工具箱整体视觉风格保持一致
7. **使用ApplyThemeToElement**：优先使用API提供的ApplyThemeToElement方法，简化主题应用过程

## 6. 构建与部署

### 6.1 构建插件

1. **构建项目**：
   - 右键点击项目 → 「生成」
   - 生成的 `.dll` 文件位于 `bin\Debug\net9.0-windows\` 目录下

### 6.2 部署插件

1. **部署插件**：
   - 将生成的 `.dll` 文件复制到 WPF插件工具箱应用程序的 `Plugins` 目录下
   - 如果有依赖，也需要将依赖的 `.dll` 文件复制到 `Plugins` 目录下

### 6.3 测试插件

1. **启动应用程序**：
   - 启动 WPF插件工具箱应用程序
   - 插件会自动加载

2. **测试功能**：
   - 在插件列表中选择你的插件
   - 查看插件UI是否正常显示
   - 测试插件功能
   - 查看调试信息

## 7. 最佳实践

### 7.1 插件设计

- **单一职责**：每个插件只负责一个功能领域
- **松耦合**：插件之间通过API交互，避免直接依赖
- **可扩展**：设计时考虑未来扩展需求
- **可测试**：便于单元测试和集成测试
- **文档完善**：提供清晰的文档和示例

### 7.2 性能优化

- **异步处理**：使用异步API处理耗时操作
- **延迟初始化**：只在需要时初始化资源
- **资源管理**：及时释放资源，实现 `IDisposable` 接口
- **UI优化**：使用虚拟化列表、数据模板等优化UI性能

### 7.3 安全性

- **最小权限**：只请求必要的API访问权限
- **输入验证**：验证所有用户输入
- **异常处理**：处理所有异常，避免崩溃
- **资源保护**：不修改工具箱核心文件

### 7.4 依赖管理

- **明确依赖关系**：清晰定义插件的依赖关系
- **版本兼容**：确保依赖版本兼容性
- **依赖隔离**：避免依赖冲突
- **备选方案**：为可选依赖提供备选功能

## 8. 故障排除

### 8.1 常见问题

| 问题 | 可能原因 | 解决方案 |
|------|----------|----------|
| 插件无法加载 | - 依赖缺失<br>- 代码错误<br>- 接口实现不正确 | - 检查依赖是否存在<br>- 查看调试窗口的错误信息<br>- 确保正确实现了IPlugin或IDependency接口 |
| 插件UI无法显示 | - `GetMainView()`返回null<br>- UI控件有错误 | - 确保`GetMainView()`返回有效的`UserControl`<br>- 检查UI控件的代码和XAML |
| 插件崩溃 | - 未处理异常<br>- 资源泄漏 | - 添加异常处理<br>- 正确实现`Dispose()`方法 |
| 依赖无法获取 | - 依赖ID错误<br>- 依赖未正确实现 | - 检查依赖ID是否正确<br>- 确保依赖正确实现了IDependency接口 |

### 8.2 调试技巧

1. **查看调试信息**：在主界面或调试窗口中查看日志信息
2. **添加详细日志**：在关键位置添加日志记录
3. **使用Visual Studio调试**：附加到工具箱进程调试插件
4. **检查事件查看器**：查看应用程序事件日志
5. **验证插件目录**：确保插件文件在正确的目录中

## 9. 示例插件

### 9.1 普通插件示例

见第4.2.1节

### 9.2 依赖插件示例

见第4.2.2节

## 10. 版本历史

| 版本 | 日期 | 主要变更 |
|------|------|----------|
| 1.0.0 | 2025-12-12 | 初始版本，包含核心插件系统 |
| 1.1.0 | 2025-12-13 | 新增插件依赖管理功能 |
| 1.2.0 | 2025-12-14 | 新增目录监听功能，自动重新加载插件 |
| 1.3.0 | 2025-12-15 | 完善插件API，新增文件操作和窗口操作功能 |
| 1.4.0 | 2025-12-16 | 新增调试窗口，优化日志记录功能 |
| 1.5.0 | 2025-12-17 | 完善插件生命周期管理，优化界面交互 |
| 1.6.0 | 2025-12-18 | 移除MEF框架依赖，使用AssemblyLoadContext实现插件加载 |
| 1.7.0 | 2025-12-26 | 新增主题相关功能，更新插件API文档 |
| 1.8.0 | 2026-01-07 | 完善主题同步机制，修复插件主题实时更新问题，新增数据共享和事件总线功能 |

## 11. 许可证

本项目采用 [MIT License](https://opensource.org/licenses/MIT) 许可证。

---

**WPF插件工具箱开发团队**
**最后更新日期：2026-01-07**