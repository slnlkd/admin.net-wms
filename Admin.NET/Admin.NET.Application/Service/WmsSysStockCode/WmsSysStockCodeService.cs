// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Core.Service;
using Microsoft.AspNetCore.Http;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Admin.NET.Application.Entity;
using Yitter.IdGenerator;
using Furion.DistributedIDGenerator;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
namespace Admin.NET.Application;

/// <summary>
/// 托盘管理服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsSysStockCodeService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsSysStockCode> _wmsSysStockCodeRep;
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWaareHouseRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SysDictTypeService _sysDictTypeService;
    private readonly ILogger _logger;

    public WmsSysStockCodeService(ILoggerFactory loggerFactory, SqlSugarRepository<WmsSysStockCode> wmsSysStockCodeRep, SqlSugarRepository<WmsBaseWareHouse> wmsBaseWaareHouseRep, ISqlSugarClient sqlSugarClient, SysDictTypeService sysDictTypeService)
    {
        _wmsSysStockCodeRep = wmsSysStockCodeRep;
        _wmsBaseWaareHouseRep = wmsBaseWaareHouseRep;
        _sqlSugarClient = sqlSugarClient;
        _sysDictTypeService = sysDictTypeService;
        _logger = loggerFactory.CreateLogger("条码管理");
    }

    /// <summary>
    /// 分页查询托盘管理 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询托盘管理")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsSysStockCodeOutput>> Page(PageWmsSysStockCodeInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsSysStockCodeRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.StockCode.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), u => u.StockCode.Contains(input.StockCode.Trim()))
            .WhereIF(input.WarehouseId != null, u => u.WarehouseId == input.WarehouseId)
            .WhereIF(input.Status != null, u => u.Status == input.Status)
            .WhereIF(input.StockType != null, u => u.StockType == input.StockType)
            .LeftJoin<WmsBaseWareHouse>((u, warehouse) => u.WarehouseId == warehouse.Id)
            .Select((u, warehouse) => new WmsSysStockCodeOutput
            {
                Id = u.Id,
                StockCode = u.StockCode,
                WarehouseId = u.WarehouseId,
                WarehouseFkDisplayName = $"{warehouse.WarehouseName}",
                Status = u.Status,
                PrintCount = u.PrintCount,
                StockType = u.StockType,
                IsDelete = u.IsDelete,
                CreateTime = u.CreateTime,
                UpdateTime = u.UpdateTime,
                CreateUserId = u.CreateUserId,
                CreateUserName = u.CreateUserName,
                UpdateUserId = u.UpdateUserId,
                UpdateUserName = u.UpdateUserName,
            });
		return await query.OrderBuilder(input,"u.").ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取托盘管理详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取托盘管理详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsSysStockCode> Detail([FromQuery] QueryByIdWmsSysStockCodeInput input)
    {
        return await _wmsSysStockCodeRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加托盘管理 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加托盘管理")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsSysStockCodeInput input)
    {
        var entity = input.Adapt<WmsSysStockCode>();
        return await _wmsSysStockCodeRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新托盘管理 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新托盘管理")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsSysStockCodeInput input)
    {
        var entity = input.Adapt<WmsSysStockCode>();
        await _wmsSysStockCodeRep.AsUpdateable(entity)
        .IgnoreColumns(u => new {
            u.Status,
            u.PrintCount,
        })
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除托盘管理 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除托盘管理")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsSysStockCodeInput input)
    {
        var entity = await _wmsSysStockCodeRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsSysStockCodeRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsSysStockCodeRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除托盘管理 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除托盘管理")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsSysStockCodeInput> input)
    {
        var exp = Expressionable.Create<WmsSysStockCode>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsSysStockCodeRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsSysStockCodeRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsSysStockCodeRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataWmsSysStockCodeInput input)
    {
        var warehouseIdData = await _wmsSysStockCodeRep.Context.Queryable<WmsBaseWareHouse>()
            .InnerJoinIF<WmsSysStockCode>(input.FromPage, (u, r) => u.Id == r.WarehouseId)
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.WarehouseName}"
            }).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "warehouseId", warehouseIdData },
        };
    }

    /// <summary>
    /// 获取最新条码信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取最新条码信息")]
    [ApiDescriptionSettings(Name = "GetSysStockCode"), HttpPost]
    public async Task<dynamic> GetSysStockCode(WmsSysStockCodeBaseInput input)
    {
        try
        {
            // 参数验证
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (input.WarehouseId == null)
                throw new ArgumentException("仓库ID不能为空");

            if (input.StockType == null)
                throw new ArgumentException("库存类型不能为空");

            // 查询最大条码
            var maxCode = await _wmsSysStockCodeRep.AsQueryable()
                .Where(u => u.WarehouseId == input.WarehouseId && u.StockType == input.StockType)
                .MaxAsync(u => u.StockCode);

            string newStockCode;
            WmsBaseWareHouse wmsBaseWareHouse = await _wmsBaseWaareHouseRep.GetByIdAsync(input.WarehouseId);

            if (wmsBaseWareHouse == null)
                throw new Exception($"未找到ID为{input.WarehouseId}的仓库信息");

            if (string.IsNullOrEmpty(maxCode))
            {
                // 生成初始条码：仓库编码 + 0 + 载具类型ID + 5位流水码
                newStockCode = $"{wmsBaseWareHouse.WarehouseCode}0{input.StockType}00001";
            }
            else
            {
                // 解析现有最大条码，流水号部分+1
                // 假设条码格式：固定前缀 + 3位流水号(正常是5 提高兼容性，可能客户的编码就是短的 但是后续执行新增会扩到5位数字）
                string prefix = maxCode.Substring(0, maxCode.Length - 3);
                string serialNumberStr = maxCode.Substring(maxCode.Length - 3);

                if (!int.TryParse(serialNumberStr, out int serialNumber))
                    throw new Exception($"现有条码格式错误，无法解析流水号: {maxCode}z");

                // 流水号+1，并格式化为5位数字
                serialNumber++;
                if (serialNumber > 99999)
                    throw new Exception("流水号已超过最大值99999，无法生成新条码");

                newStockCode = $"{prefix}{serialNumber:D5}";
            }
            _logger.LogInformation($"获取最新条码成功:{newStockCode}");
            // 返回结果
            return new
            {
                success = true,
                data = newStockCode,
                message = "条码生成成功"
            };
        }
        catch (Exception ex)
        {
            //记录日志
             _logger.LogError(ex, "生成条码时发生错误");

            return new
            {
                success = false,
                data = (string)null,
                message = $"生成条码失败: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// 导出托盘管理记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出托盘管理记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsSysStockCodeInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsSysStockCodeOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        var statusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "StockIsUse" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var stockTypeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "StockType" }).Result.ToDictionary(x => x.Value, x => x.Label);
        list.ForEach(e => {
            e.StatusDictLabel = statusDictMap.GetValueOrDefault(e.Status.ToString() ?? "", e.Status.ToString());
            e.StockTypeDictLabel = stockTypeDictMap.GetValueOrDefault(e.StockType.ToString() ?? "", e.StockType.ToString());
        });
        return ExcelHelper.ExportTemplate(list, "托盘管理导出记录");
    }
    
    /// <summary>
    /// 下载托盘管理数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载托盘管理数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsSysStockCodeOutput>(), "托盘管理导入模板", (_, info) =>
        {
            if (nameof(ExportWmsSysStockCodeOutput.WarehouseFkDisplayName) == info.Name) return _wmsSysStockCodeRep.Context.Queryable<WmsBaseWareHouse>().Select(u => $"{u.WarehouseName}").Distinct().ToList();
            return null;
        });
    }
    
    private static readonly object _wmsSysStockCodeImportLock = new object();
    /// <summary>
    /// 导入托盘管理记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入托盘管理记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsSysStockCodeImportLock)
        {
            var statusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "StockIsUse" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var stockTypeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "StockType" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var stream = ExcelHelper.ImportData<ImportWmsSysStockCodeInput, WmsSysStockCode>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    // 链接 所属仓库
                    var warehouseIdLabelList = pageItems.Where(x => x.WarehouseFkDisplayName != null).Select(x => x.WarehouseFkDisplayName).Distinct().ToList();
                    if (warehouseIdLabelList.Any()) {
                        var warehouseIdLinkMap = _wmsSysStockCodeRep.Context.Queryable<WmsBaseWareHouse>().Where(u => warehouseIdLabelList.Contains($"{u.WarehouseName}")).ToList().ToDictionary(u => $"{u.WarehouseName}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.WarehouseId = warehouseIdLinkMap.GetValueOrDefault(e.WarehouseFkDisplayName ?? "");
                            if (e.WarehouseId == null) e.Error = "所属仓库链接失败";
                        });
                    }
                    
                    // 映射字典值
                    foreach(var item in pageItems) {
                        System.Text.StringBuilder sbError = new System.Text.StringBuilder();
                        if (!string.IsNullOrWhiteSpace(item.StatusDictLabel)) {
                            item.Status = Convert.ToInt32(statusDictMap.GetValueOrDefault(item.StatusDictLabel));
                            if (item.Status == null) sbError.AppendLine("条码状态字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.StockTypeDictLabel)) {
                            item.StockType = Convert.ToInt32(stockTypeDictMap.GetValueOrDefault(item.StockTypeDictLabel));
                            if (item.StockType == null) sbError.AppendLine("托盘类型字典映射失败");
                        }
                        item.Error = sbError.ToString();
                    }
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        if (x.WarehouseId == null){
                            x.Error = "所属仓库不能为空";
                            return false;
                        }
                        if (x.StockType == null){
                            x.Error = "托盘类型不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<WmsSysStockCode>>();
                    
                    var storageable = _wmsSysStockCodeRep.Context.Storageable(rows)
                        .SplitError(it => string.IsNullOrWhiteSpace(it.Item.StockCode), "载具号不能为空")
                        .SplitError(it => it.Item.StockCode?.Length > 50, "载具号长度不能超过50个字符")
                        .SplitError(it => it.Item.WarehouseId == null, "所属仓库不能为空")
                        .SplitError(it => it.Item.StockType == null, "托盘类型不能为空")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.StockCode,
                        it.WarehouseId,
                        it.Status,
                        it.StockType,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
