// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.ComponentModel;
using Admin.NET.Application.Entity;
using Furion.DatabaseAccessor;
using SqlSugar;
using Admin.NET.Core.Service;

namespace Admin.NET.Application;

/// <summary>
/// 仪表盘服务 📊
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public class WmsDashboardService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsImportOrder> _wmsImportOrderRep;
    private readonly SqlSugarRepository<WmsExportOrder> _wmsExportOrderRep;
    private readonly SqlSugarRepository<WmsMoveOrder> _wmsMoveOrderRep;
    private readonly SqlSugarRepository<WmsStockCheckNotify> _wmsStockCheckNotifyRep;
    private readonly SysCacheService _sysCacheService;

    public WmsDashboardService(
        SqlSugarRepository<WmsImportOrder> wmsImportOrderRep,
        SqlSugarRepository<WmsExportOrder> wmsExportOrderRep,
        SqlSugarRepository<WmsMoveOrder> wmsMoveOrderRep,
        SqlSugarRepository<WmsStockCheckNotify> wmsStockCheckNotifyRep,
        SysCacheService sysCacheService)
    {
        _wmsImportOrderRep = wmsImportOrderRep;
        _wmsExportOrderRep = wmsExportOrderRep;
        _wmsMoveOrderRep = wmsMoveOrderRep;
        _wmsStockCheckNotifyRep = wmsStockCheckNotifyRep;
        _sysCacheService = sysCacheService;
    }

    /// <summary>
    /// 获取出入库任务趋势数据 📊
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取出入库任务趋势数据")]
    [ApiDescriptionSettings(Name = "GetTaskTrend"), HttpGet]
    public async Task<WmsTaskTrendOutput> GetTaskTrend()
    {
        // 使用缓存获取近31天的数据 (加1天buffer)
        var cacheData = await _sysCacheService.AdGetAsync("WmsDashboard:TaskTrendData", async () =>
        {
            var endDate = DateTime.Now.Date.AddDays(1);
            var startDate = DateTime.Now.Date.AddDays(-31);

            return new WmsDashboardCacheData
            {
                Imports = await _wmsImportOrderRep.AsQueryable()
                    .Where(u => !u.IsDelete && u.CreateTime >= startDate && u.CreateTime < endDate)
                    .Select(u => new SimpleTaskData { CreateTime = u.CreateTime, StatusStr = u.ImportExecuteFlag })
                    .ToListAsync(),
                Exports = await _wmsExportOrderRep.AsQueryable()
                    .Where(u => !u.IsDelete && u.CreateTime >= startDate && u.CreateTime < endDate)
                    .Select(u => new SimpleTaskData { CreateTime = u.CreateTime, StatusInt = u.ExportExecuteFlag })
                    .ToListAsync(),
                Moves = await _wmsMoveOrderRep.AsQueryable()
                    .Where(u => !u.IsDelete && u.CreateTime >= startDate && u.CreateTime < endDate)
                    .Select(u => new SimpleTaskData { CreateTime = u.CreateTime, StatusStr = u.MoveStauts })
                    .ToListAsync(),
                StockChecks = await _wmsStockCheckNotifyRep.AsQueryable()
                    .Where(u => !u.IsDelete && u.CreateTime >= startDate && u.CreateTime < endDate)
                    .Select(u => new SimpleTaskData { CreateTime = u.CreateTime, StatusInt = u.ExecuteFlag })
                    .ToListAsync()
            };
        }, TimeSpan.FromSeconds(5));

        var now = DateTime.Now;
        var today = now.Date;
        var date7DaysAgo = today.AddDays(-6);
        var date30DaysAgo = today.AddDays(-29);

        // 内存中计算 - 趋势图数据
        var last7Days = ProcessDailyCount(cacheData.Imports, cacheData.Exports, date7DaysAgo, 7);
        var last30Days = ProcessDailyCount(cacheData.Imports, cacheData.Exports, date30DaysAgo, 30);

        // 内存中计算 - 任务进度数据
        var taskProgress7Days = CalcTaskProgress(cacheData, date7DaysAgo);
        var taskProgress30Days = CalcTaskProgress(cacheData, date30DaysAgo);

        // 获取当前总任务数（未作废且未完成）- 保持实时查询或也放入缓存策略(这里保持查询数据库因为是"当前"状态，或者也可以基于缓存如果是近期创建的)
        // 既然要求缓存优化，这里我们可以利用缓存的数据加上对"更早但未完成"任务的处理。
        // 但通常Dashboard的"当前任务"是指所有未完成的，不仅限于30天内。
        // 为了性能，如果数据量大，全表扫描未完成任务可能慢。
        // 但用户只说了"缓存30天...在此基础上处理"，对于"Current Total"可能还得查库，或者认为30天前的未完成任务极少/忽略/另行处理。
        // 这里为了准确性，保留原来的 CountAsync 查询，或者优化为只查 Status。
        // 鉴于用户抱怨"接口慢"，这4个CountAsync也是开销。
        // 暂时保留CountAsync，因为它们通常有索引且只过滤状态。如果是全表扫描慢，应该加索引。
        var currentImportTotal = await _wmsImportOrderRep.AsQueryable()
            .Where(u => !u.IsDelete && (u.ImportExecuteFlag == "01" || u.ImportExecuteFlag == "02"))
            .CountAsync();

        var currentExportTotal = await _wmsExportOrderRep.AsQueryable()
            .Where(u => !u.IsDelete && (u.ExportExecuteFlag == 1 || u.ExportExecuteFlag == 2))
            .CountAsync();

        return new WmsTaskTrendOutput
        {
            Last7Days = last7Days,
            Last30Days = last30Days,
            TaskProgress7Days = taskProgress7Days,
            TaskProgress30Days = taskProgress30Days
        };
    }

    private List<TaskProgressInfo> CalcTaskProgress(WmsDashboardCacheData data, DateTime startDate)
    {
        // 入库: Pending(01,02), Completed(03,04)
        var importInWindow = data.Imports.Where(u => u.CreateTime >= startDate).ToList();
        var importPending = importInWindow.Count(u => u.StatusStr == "01" || u.StatusStr == "02");
        var importCompleted = importInWindow.Count(u => u.StatusStr == "03" || u.StatusStr == "04");
        var importTotal = importInWindow.Count;

        // 出库: Pending(01,02), Completed(03,04)
        var exportInWindow = data.Exports.Where(u => u.CreateTime >= startDate).ToList();
        var exportPending = exportInWindow.Count(u => u.StatusStr == "01" || u.StatusStr == "02");
        var exportCompleted = exportInWindow.Count(u => u.StatusStr == "03" || u.StatusStr == "04");
        var exportTotal = exportInWindow.Count;

        // 移库: Pending(01,02), Completed(03,04)
        var moveInWindow = data.Moves.Where(u => u.CreateTime >= startDate).ToList();
        var movePending = moveInWindow.Count(u => u.StatusStr == "01" || u.StatusStr == "02");
        var moveCompleted = moveInWindow.Count(u => u.StatusStr == "03" || u.StatusStr == "04");
        var moveTotal = moveInWindow.Count;

        // 盘点: Pending(<3), Completed(3)
        var checkInWindow = data.StockChecks.Where(u => u.CreateTime >= startDate).ToList();
        var checkPending = checkInWindow.Count(u => u.StatusInt < 3);
        var checkCompleted = checkInWindow.Count(u => u.StatusInt == 3);
        var checkTotal = checkInWindow.Count;

        return new List<TaskProgressInfo>
        {
            new() { Name = "入库任务", Count = importPending, Completed = importCompleted, Total = importTotal, Color = "#409EFF", Priority = "low" },
            new() { Name = "出库任务", Count = exportPending, Completed = exportCompleted, Total = exportTotal, Color = "#67C23A", Priority = "high" },
            new() { Name = "移库任务", Count = movePending, Completed = moveCompleted, Total = moveTotal, Color = "#E6A23C", Priority = "medium" },
            new() { Name = "盘点任务", Count = checkPending, Completed = checkCompleted, Total = checkTotal, Color = "#F56C6C", Priority = "medium" }
        };
    }

    /// <summary>
    /// 处理每日数量统计
    /// </summary>
    private List<DailyTaskCount> ProcessDailyCount(
        List<SimpleTaskData> imports,
        List<SimpleTaskData> exports,
        DateTime startDate,
        int days)
    {
        var result = new List<DailyTaskCount>();

        // 内存中分组
        var importGrouped = imports
            .Where(u => u.CreateTime >= startDate)
            .GroupBy(d => d.CreateTime.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Count());

        var exportGrouped = exports
            .Where(u => u.CreateTime >= startDate)
            .GroupBy(d => d.CreateTime.ToString("yyyy-MM-dd"))
            .ToDictionary(g => g.Key, g => g.Count());

        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var dateStr = date.ToString("yyyy-MM-dd");

            result.Add(new DailyTaskCount
            {
                Date = dateStr,
                ImportCount = importGrouped.GetValueOrDefault(dateStr, 0),
                ExportCount = exportGrouped.GetValueOrDefault(dateStr, 0)
            });
        }

        return result;
    }

    // 内部类用于缓存数据结构
    public class WmsDashboardCacheData
    {
        public List<SimpleTaskData> Imports { get; set; } = new();
        public List<SimpleTaskData> Exports { get; set; } = new();
        public List<SimpleTaskData> Moves { get; set; } = new();
        public List<SimpleTaskData> StockChecks { get; set; } = new();
    }

    public class SimpleTaskData
    {
        public DateTime CreateTime { get; set; }
        public string StatusStr { get; set; }
        public int? StatusInt { get; set; }
    }

}