# WPF插件开发手册

## 1. 概述

本手册详细介绍了如何使用WPF插件工具箱开发插件，包括插件类型、开发流程、API使用和最佳实践。

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

### 3.2 引用依赖

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
        public DependencyType Type { get; } = DependencyType.Required;

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
}
```

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
| 1.0.0 | 2025-12-18 | 初始版本 |

## 11. 许可证

本项目采用 [MIT License](https://opensource.org/licenses/MIT) 许可证。

---

**WPF插件工具箱开发团队**
**最后更新日期：2025-12-18**