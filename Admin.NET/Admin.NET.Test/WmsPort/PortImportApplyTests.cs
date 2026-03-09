// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.WmsPda.Dto;
using Admin.NET.Application.Service.WmsPort.Dto;
using Admin.NET.Application.Service.WmsPort.Process;
using Admin.NET.Application.Service.WmsSqlView;
using Admin.NET.Core;
using Furion.FriendlyException;
using Microsoft.Extensions.Logging;
using Moq;
using SqlSugar;
using System.Linq.Expressions;
using Xunit;

namespace Admin.NET.Test.WmsPort;

/// <summary>
/// PortImportApply 单元测试类
/// </summary>
public class PortImportApplyTests
{
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<ILogger> _mockLogger;
    private readonly Mock<ISqlSugarClient> _mockSqlSugarClient;
    private readonly Mock<PortBaseRepos> _mockRepos;
    private readonly Mock<PortSlotAlloc> _mockSlotAlloc;
    private readonly Mock<PortImportBind> _mockImportBind;
    private readonly Mock<WmsSqlViewService> _mockSqlViewService;
    private readonly PortImportApply _service;

    // Repos Mocks
    private readonly Mock<SqlSugarRepository<WmsImportTask>> _mockImportTaskRep;
    private readonly Mock<SqlSugarRepository<WmsBaseWareHouse>> _mockWarehouseRep;
    private readonly Mock<SqlSugarRepository<WmsStockTray>> _mockStockTrayRep;
    private readonly Mock<SqlSugarRepository<WmsStockSlotBoxInfo>> _mockBoxInfoRep;
    private readonly Mock<SqlSugarRepository<WmsExportOrder>> _mockExportOrderRep;
    private readonly Mock<SqlSugarRepository<WmsSysStockCode>> _mockStockCodeRep;
    private readonly Mock<SqlSugarRepository<WmsImportNotify>> _mockImportNotifyRep;
    private readonly Mock<SqlSugarRepository<WmsBaseSlot>> _mockSlotRep;
    private readonly Mock<SqlSugarRepository<WmsBaseMaterial>> _mockMaterialRep;
    private readonly Mock<SqlSugarRepository<WmsBaseLaneway>> _mockLanewayRep;
    private readonly Mock<SqlSugarRepository<WmsImportOrder>> _mockImportOrderRep;

    public PortImportApplyTests()
    {
        // 1. Init Mocks
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLogger = new Mock<ILogger>();
        _mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(_mockLogger.Object);

        _mockSqlSugarClient = new Mock<ISqlSugarClient>();
        // Mock Transaction
        var mockAdo = new Mock<IAdo>();
        _mockSqlSugarClient.Setup(x => x.Ado).Returns(mockAdo.Object);
        // UseTranAsync just executes the action
        mockAdo.Setup(x => x.UseTranAsync(It.IsAny<Func<Task>>(), It.IsAny<Action<Exception>>()))
               .Returns((Func<Task> action, Action<Exception> error) => action());

        // 2. Init Repo Mocks
        _mockImportTaskRep = new Mock<SqlSugarRepository<WmsImportTask>>();
        _mockWarehouseRep = new Mock<SqlSugarRepository<WmsBaseWareHouse>>();
        _mockStockTrayRep = new Mock<SqlSugarRepository<WmsStockTray>>();
        _mockBoxInfoRep = new Mock<SqlSugarRepository<WmsStockSlotBoxInfo>>();
        _mockExportOrderRep = new Mock<SqlSugarRepository<WmsExportOrder>>();
        _mockStockCodeRep = new Mock<SqlSugarRepository<WmsSysStockCode>>();
        _mockImportNotifyRep = new Mock<SqlSugarRepository<WmsImportNotify>>();
        _mockSlotRep = new Mock<SqlSugarRepository<WmsBaseSlot>>();
        _mockMaterialRep = new Mock<SqlSugarRepository<WmsBaseMaterial>>();
        _mockLanewayRep = new Mock<SqlSugarRepository<WmsBaseLaneway>>();
        _mockImportOrderRep = new Mock<SqlSugarRepository<WmsImportOrder>>();

        // Setup PortBaseRepos with Mocks
        // Note: passing null for unused repos to save setup time, add as needed
        _mockRepos = new Mock<PortBaseRepos>(
            _mockImportTaskRep.Object,
            _mockImportNotifyRep.Object,
            new Mock<SqlSugarRepository<WmsImportNotifyDetail>>().Object,
            _mockImportOrderRep.Object,
            new Mock<SqlSugarRepository<WmsImportLabelPrint>>().Object,
            new Mock<SqlSugarRepository<WmsImportSecondCursor>>().Object,
            new Mock<SqlSugarRepository<WMSTaskNoPub>>().Object,
            _mockStockTrayRep.Object,
            _mockBoxInfoRep.Object,
            new Mock<SqlSugarRepository<WmsStockInfo>>().Object,
            new Mock<SqlSugarRepository<WmsStock>>().Object,
            _mockStockCodeRep.Object,
            new Mock<SqlSugarRepository<WmsStockCheckTask2>>().Object,
            new Mock<SqlSugarRepository<WmsBaseArea>>().Object,
            _mockSlotRep.Object,
            _mockMaterialRep.Object,
            _mockLanewayRep.Object,
            _mockWarehouseRep.Object,
            _mockExportOrderRep.Object,
            new Mock<SqlSugarRepository<WmsExportTask>>().Object,
            new Mock<SqlSugarRepository<WmsBackToWareHouse>>().Object,
            new Mock<SqlSugarRepository<WmsMoveTask>>().Object,
            new Mock<SqlSugarRepository<WmsMoveNotify>>().Object,
            new Mock<SqlSugarRepository<WmsMoveOrder>>().Object
        );

        // 3. Init Service Mocks
        _mockSlotAlloc = new Mock<PortSlotAlloc>(null, null, null, null, null, null, null);
        _mockImportBind = new Mock<PortImportBind>(null, null, null, null, null);
        _mockSqlViewService = new Mock<WmsSqlViewService>(null);

        // 4. Create Service
        _service = new PortImportApply(
            _mockLoggerFactory.Object,
            _mockSqlSugarClient.Object,
            _mockRepos.Object,
            _mockSlotAlloc.Object,
            _mockImportBind.Object,
            _mockSqlViewService.Object
        );
    }

    [Fact]
    public async Task ProcessCreateImportOrder_NullInput_ShouldThrowOops()
    {
        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.ProcessCreateImportOrder(null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task ProcessCreateImportOrder_InvalidStockCode_ShouldThrowOops(string stockCode)
    {
        var input = new ImportApplyInput { StockCode = stockCode, HouseCode = "WH001" };
        await Assert.ThrowsAsync<Exception>(() => _service.ProcessCreateImportOrder(input));
    }

    [Fact]
    public async Task ProcessCreateImportOrder_WarehouseNotFound_ShouldThrowOops()
    {
        // Arrange
        var input = new ImportApplyInput { StockCode = "T01", HouseCode = "WH_NOT_EXIST" };
        
        // Mock Task Generation
        _mockSqlViewService.Setup(x => x.GenerateTaskNo()).ReturnsAsync("TASK001");
        
        // Mock Warehouse not found
        _mockWarehouseRep.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<WmsBaseWareHouse, bool>>>()))
            .ReturnsAsync((WmsBaseWareHouse)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.ProcessCreateImportOrder(input));
        Assert.Contains("仓库信息不存在", ex.Message);
    }

    [Fact]
    public async Task ProcessCreateImportOrder_StereoWarehouse_NoType_ShouldThrowOops()
    {
        // Arrange
        var input = new ImportApplyInput { StockCode = "T01", HouseCode = "WH001", Type = "" };

        _mockSqlViewService.Setup(x => x.GenerateTaskNo()).ReturnsAsync("TASK001");
        
        // Mock Warehouse
        var warehouse = new WmsBaseWareHouse { WarehouseCode = "WH001", WarehouseType = ImportApplyConstants.WarehouseType.StereoWarehouse };
        _mockWarehouseRep.Setup(x => x.GetFirstAsync(It.IsAny<Expression<Func<WmsBaseWareHouse, bool>>>()))
            .ReturnsAsync(warehouse);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _service.ProcessCreateImportOrder(input));
        Assert.Contains("货位类型不能为空", ex.Message);
    }
}
