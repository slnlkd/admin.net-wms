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
namespace Admin.NET.Application;

/// <summary>
/// 库存明细查询服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsStockTrayService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsStockTray> _wmsStockTrayRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsStockTrayService(SqlSugarRepository<WmsStockTray> wmsStockTrayRep, ISqlSugarClient sqlSugarClient)
    {
        _wmsStockTrayRep = wmsStockTrayRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询库存明细查询 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询库存明细查询")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockTrayOutput>> Page(PageWmsStockTrayInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsStockTrayRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.StockSlotCode.Contains(input.Keyword) || u.StockCode.Contains(input.Keyword) || u.MaterialId.Contains(input.Keyword) || u.LotNo.Contains(input.Keyword) || u.LanewayId.Contains(input.Keyword) || u.WarehouseId.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockSlotCode), u => u.StockSlotCode.Contains(input.StockSlotCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockCode), u => u.StockCode.Contains(input.StockCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialId), u => u.MaterialId.Contains(input.MaterialId.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), u => u.LotNo.Contains(input.LotNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LanewayId), u => u.LanewayId.Contains(input.LanewayId.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.WarehouseId), u => u.WarehouseId.Contains(input.WarehouseId.Trim()))
            .WhereIF(input.StockDateRange?.Length == 2, u => u.StockDate >= input.StockDateRange[0] && u.StockDate <= input.StockDateRange[1])
            .WhereIF(input.StockStatusFlag != null, u => u.StockStatusFlag == input.StockStatusFlag)
            .WhereIF(input.InspectionStatus != null, u => u.InspectionStatus == input.InspectionStatus)
            .WhereIF(input.AbnormalStatu != null, u => u.AbnormalStatu == input.AbnormalStatu)
            .LeftJoin<WmsBaseMaterial>((u, material) => u.MaterialId == material.Id.ToString())
            .Select((u, material) => new WmsStockTrayOutput
            {
                Id = u.Id,
                StockSlotCode = u.StockSlotCode,
                StockCode = u.StockCode,
                StockQuantity = u.StockQuantity,
                StockDate = u.StockDate,
                MaterialId = u.MaterialId,
                MaterialFkDisplayName = $"{material.MaterialCode}-{material.MaterialName}-{material.Id}",
                StockStatusFlag = u.StockStatusFlag,
                LotNo = u.LotNo,
                InspectionStatus = u.InspectionStatus,
                LanewayId = u.LanewayId,
                WarehouseId = u.WarehouseId,
                Remark = u.Remark,
                IsSamolingTray = u.IsSamolingTray,
                LockQuantity = u.LockQuantity,
                OwnerCode = u.OwnerCode,
                AbnormalStatu = u.AbnormalStatu,
                ProductionDate = u.ProductionDate,
                ValidateDay = u.ValidateDay,
                ReleaseStatus = u.ReleaseStatus,
                RetestDate = u.RetestDate,
                InspectionNumber = u.InspectionNumber,
                outQty = u.outQty,
                BoxQuantity = u.BoxQuantity,
                OddMarking = u.OddMarking,
                CustomerId = u.CustomerId,
                RevisionDate = u.RevisionDate,
                VehicleSubId = u.VehicleSubId,
                InspectFlag = u.InspectFlag,
                ReleaseDate = u.ReleaseDate,
                CreateUserName = u.CreateUserName,
                UpdateUserName = u.UpdateUserName,
                IsDelete = u.IsDelete,
                CreateTime = u.CreateTime,
                UpdateTime = u.UpdateTime,
                CreateUserId = u.CreateUserId,
                UpdateUserId = u.UpdateUserId,
                SupplierId = u.SupplierId,
                ManufacturerId = u.ManufacturerId,
            }).OrderBy(u => u.LotNo);

        return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取库存明细查询详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取库存明细查询详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsStockTray> Detail([FromQuery] QueryByIdWmsStockTrayInput input)
    {
        return await _wmsStockTrayRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加库存明细查询 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加库存明细查询")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsStockTrayInput input)
    {
        var entity = input.Adapt<WmsStockTray>();
        return await _wmsStockTrayRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新库存明细查询 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新库存明细查询")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsStockTrayInput input)
    {
        var entity = input.Adapt<WmsStockTray>();
        await _wmsStockTrayRep.AsUpdateable(entity)
        .IgnoreColumns(u => new {
            u.StockSlotCode,
            u.StockCode,
            u.StockQuantity,
            u.StockDate,
            u.MaterialId,
            u.StockStatusFlag,
            u.LotNo,
            u.InspectionStatus,
            u.LanewayId,
            u.WarehouseId,
            u.Remark,
            u.IsSamolingTray,
            u.LockQuantity,
            u.OwnerCode,
            u.AbnormalStatu,
            u.ProductionDate,
            u.ValidateDay,
            u.ReleaseStatus,
            u.RetestDate,
            u.InspectionNumber,
            u.outQty,
            u.BoxQuantity,
            u.OddMarking,
            u.CustomerId,
            u.RevisionDate,
            u.VehicleSubId,
            u.InspectFlag,
            u.ReleaseDate,
            u.SupplierId,
            u.ManufacturerId,
        })
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除库存明细查询 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除库存明细查询")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsStockTrayInput input)
    {
        var entity = await _wmsStockTrayRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsStockTrayRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsStockTrayRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除库存明细查询 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除库存明细查询")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsStockTrayInput> input)
    {
        var exp = Expressionable.Create<WmsStockTray>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsStockTrayRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsStockTrayRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsStockTrayRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataWmsStockTrayInput input)
    {
        var materialIdData = await _wmsStockTrayRep.Context.Queryable<WmsBaseMaterial>()
            .InnerJoinIF<WmsStockTray>(input.FromPage, (u, r) => u.Id.ToString() == r.MaterialId)
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.MaterialCode}-{u.MaterialName}-{u.Id}"
            }).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "materialId", materialIdData },
        };
    }
}
