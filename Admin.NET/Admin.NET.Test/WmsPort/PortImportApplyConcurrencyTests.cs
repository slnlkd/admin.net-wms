// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。

using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Application.Service.WmsPort.Process;
using Admin.NET.Core;
using Moq;
using SqlSugar;
using System.Collections.Concurrent;
using Xunit;

namespace Admin.NET.Test.WmsPort;

/// <summary>
/// PortImportApply 并发测试类
/// </summary>
public class PortImportApplyConcurrencyTests
{
    private readonly Mock<PortImportApply> _mockService;

    public PortImportApplyConcurrencyTests()
    {
        // Since ProcessCreateImportOrder uses a static semaphore, testing concurrency 
        // requires instantiating the real class or checking logic correctness.
        // Here we test the logic structure.
    }

    [Fact]
    public async Task Semaphore_ShouldBeStaticAndSingleton()
    {
        // 验证信号量是静态的，确保所有实例共享锁
        var field = typeof(PortImportApply).GetField("_semaphore", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(field);
        var semaphore = field.GetValue(null) as SemaphoreSlim;
        Assert.NotNull(semaphore);
        Assert.Equal(1, semaphore.CurrentCount);
    }

    // 注意：由于无法在此环境中轻松模拟真实的多线程DB操作，
    // 我们主要验证信号量配置和代码逻辑结构。
    // 实际的并发测试建议在集成测试环境中针对真实数据库运行。
}
