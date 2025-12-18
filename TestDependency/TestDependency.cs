using System.ComponentModel.Composition;
using WPFPluginToolbox.Core;

namespace TestDependency
{
    [Export(typeof(IDependency))]
    public class TestDependency : IDependency
    {
        public string Id => "TestDependency";
        
        public string Name => "测试依赖";
        
        public string Description => "为其他插件提供测试功能的依赖";
        
        public string Version => "1.0.0";
        
        public void Initialize()
        {
            System.Console.WriteLine("测试依赖已初始化");
        }
        
        public void Dispose()
        {
            System.Console.WriteLine("测试依赖已释放");
        }
        
        // 提供给其他插件使用的方法
        public string GetTestData()
        {
            return "来自依赖插件的数据";
        }
        
        public int Calculate(int a, int b)
        {
            return a + b;
        }
    }
}