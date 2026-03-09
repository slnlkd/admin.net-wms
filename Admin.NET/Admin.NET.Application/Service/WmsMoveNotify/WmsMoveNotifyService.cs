// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Core.Service;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Humanizer;
using Mapster;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Admin.NET.Application;

/// <summary>
/// 移库单据明细表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsMoveNotifyService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsMoveNotify> _wmsMoveNotifyRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsMoveNotifyService(SqlSugarRepository<WmsMoveNotify> wmsMoveNotifyRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsMoveNotifyRep = wmsMoveNotifyRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询移库单据明细表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询移库单据明细表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsMoveNotifyOutput>> Page(PageWmsMoveNotifyInput input)
    {
        var list = await _wmsMoveNotifyRep.AsQueryable()
            .LeftJoin<WmsBaseMaterial>((a, b) => a.MaterialId == b.Id.ToString())
            .Where((a, b) => a.MoveBillCode == input.MoveBillCode.Trim())
            .Select((a, b) => new WmsMoveNotifyOutput()
            {
                StockStockCodeId = a.StockStockCodeId,
                MoveOutSlotCode = a.MoveOutSlotCode,
                MoveInSlotCode = a.MoveInSlotCode,
                MaterialCode = b.MaterialCode,
                MaterialName = b.MaterialName,
                CreateTime = a.CreateTime
            }).ToPagedListAsync(input.Page, input.PageSize); ;
		return list;
    }

    /// <summary>
    /// 获取移库单据明细表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取移库单据明细表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsMoveNotify> Detail([FromQuery] QueryByIdWmsMoveNotifyInput input)
    {
        return await _wmsMoveNotifyRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加移库单据明细表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加移库单据明细表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsMoveNotifyInput input)
    {
        var entity = input.Adapt<WmsMoveNotify>();
        return await _wmsMoveNotifyRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新移库单据明细表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新移库单据明细表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsMoveNotifyInput input)
    {
        var entity = input.Adapt<WmsMoveNotify>();
        await _wmsMoveNotifyRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除移库单据明细表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除移库单据明细表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsMoveNotifyInput input)
    {
        var entity = await _wmsMoveNotifyRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsMoveNotifyRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsMoveNotifyRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除移库单据明细表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除移库单据明细表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsMoveNotifyInput> input)
    {
        var exp = Expressionable.Create<WmsMoveNotify>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsMoveNotifyRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsMoveNotifyRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsMoveNotifyRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 导出移库单据明细表记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出移库单据明细表记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsMoveNotifyInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsMoveNotifyOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "移库单据明细表导出记录");
    }
    
    /// <summary>
    /// 下载移库单据明细表数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载移库单据明细表数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsMoveNotifyOutput>(), "移库单据明细表导入模板");
    }
    
    private static readonly object _wmsMoveNotifyImportLock = new object();
    /// <summary>
    /// 导入移库单据明细表记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入移库单据明细表记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsMoveNotifyImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsMoveNotifyInput, WmsMoveNotify>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsMoveNotify>>();
                    
                    var storageable = _wmsMoveNotifyRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.MoveBillCode?.Length > 50, "移库单据类型长度不能超过50个字符")
                        .SplitError(it => it.Item.MoveLotNo?.Length > 50, "移库批次长度不能超过50个字符")
                        .SplitError(it => it.Item.MaterialId?.Length > 50, "物料id长度不能超过50个字符")
                        .SplitError(it => it.Item.MoveMaterialModel?.Length > 100, "移库物料型号长度不能超过100个字符")
                        .SplitError(it => it.Item.MoveMaterialTemp?.Length > 50, "物料温度长度不能超过50个字符")
                        .SplitError(it => it.Item.MoveMaterialType?.Length > 100, "物料类型（字典MaterialType）长度不能超过100个字符")
                        .SplitError(it => it.Item.MoveMaterialStatus?.Length > 100, "物料状态长度不能超过100个字符")
                        .SplitError(it => it.Item.MoveMaterialUnit?.Length > 50, "物料单位长度不能超过50个字符")
                        .SplitError(it => it.Item.MoveMaterialBrand?.Length > 100, "物料品牌长度不能超过100个字符")
                        .SplitError(it => it.Item.MoveLanewayInCode?.Length > 50, "移入巷道id长度不能超过50个字符")
                        .SplitError(it => it.Item.MoveOutSlotCode?.Length > 50, "移出储位编码长度不能超过50个字符")
                        .SplitError(it => it.Item.MoveInSlotCode?.Length > 50, "移入储位编码长度不能超过50个字符")
                        .SplitError(it => it.Item.MoveExecuteFlag?.Length > 50, "执行标志（01待执行、02正在执行、03已完成、04已上传）长度不能超过50个字符")
                        .SplitError(it => it.Item.MoveRemark?.Length > 100, "备注长度不能超过100个字符")
                        .SplitError(it => it.Item.MoveTaskNo?.Length > 100, "移库任务号长度不能超过100个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.MoveBillCode,
                        it.MoveListNo,
                        it.MoveLotNo,
                        it.MaterialId,
                        it.MoveMaterialModel,
                        it.MoveMaterialTemp,
                        it.MoveMaterialType,
                        it.MoveMaterialStatus,
                        it.MoveMaterialUnit,
                        it.MoveMaterialBrand,
                        it.MoveProductionDate,
                        it.MoveLostDate,
                        it.MoveQuantity,
                        it.MoveDate,
                        it.MoveWarehouseId,
                        it.MoveLanewayOutCode,
                        it.MoveLanewayInCode,
                        it.MoveOutSlotCode,
                        it.MoveInSlotCode,
                        it.MoveExecuteFlag,
                        it.MoveRemark,
                        it.MoveTaskNo,
                        it.StockStockCodeId,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
