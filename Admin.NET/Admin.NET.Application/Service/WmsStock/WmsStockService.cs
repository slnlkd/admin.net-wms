// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Core.Service;
using DocumentFormat.OpenXml.EMMA;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
namespace Admin.NET.Application;

/// <summary>
/// 库存查询服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsStockService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsStock> _wmsStockRep;
    private readonly SqlSugarRepository<WmsBaseMaterial> _wmsBaseMaterialRep;
    private readonly SqlSugarRepository<WmsBaseUnit> _wmsBaseUnitRep;
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWareHouseRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsStockService(SqlSugarRepository<WmsStock> wmsStockRep,
        SqlSugarRepository<WmsBaseMaterial> wmsBaseMaterialRep,
        SqlSugarRepository<WmsBaseUnit> wmsBaseUnitRep,
        SqlSugarRepository<WmsBaseWareHouse> wmsBaseWareHouseRep,
        ISqlSugarClient sqlSugarClient)
    {
        _wmsStockRep = wmsStockRep;
        _wmsBaseMaterialRep = wmsBaseMaterialRep;
        _wmsBaseUnitRep = wmsBaseUnitRep;
        _wmsBaseWareHouseRep = wmsBaseWareHouseRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 物料总量查询    分页查询库存查询 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询库存查询")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockOutput>> Page(PageWmsStockInput input)
    {
        var query = await _wmsStockRep.AsQueryable()
            .LeftJoin<WmsBaseMaterial>((s, m) => s.MaterialId == m.Id && m.IsDelete == false)
            .LeftJoin<WmsBaseUnit>((s, m, u) => m.MaterialUnit == u.Id)
            .InnerJoin<WmsBaseWareHouse>((s, m, u, w) => s.WarehouseId == w.Id)
            .Where(s => s.IsDelete == false)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), (s, m, u, w) =>w.WarehouseName.Contains(input.Keyword) || m.MaterialName.Contains(input.Keyword) || m.MaterialCode.Contains(input.Keyword))
            .WhereIF(input.WarehouseId != null,s => s.WarehouseId == input.WarehouseId)
            //.WhereIF(input.MaterialId != null, u => u.MaterialId == input.MaterialId)
            .WhereIF(input.MaterialCode != null, (s, m) => m.MaterialCode == input.MaterialCode)
            .WhereIF(input.MaterialName != null, (s, m) => m.MaterialName.Contains(input.MaterialName))
            .Select((s, m, u, w) => new
            {
                s.WarehouseId,
                m.MaterialName,
                s.MaterialId,
                s.LockQuantity,
                w.WarehouseName,
                s.StockQuantity
            }).ToListAsync();
        //// 根据用户ID获取当前用户的权限并过滤数据
        //if (query.Count() > 0)
        //{
        //    var ware = dal.GetRoleWarehouse(model.userId).Select(a => a.Id).Distinct().ToList();
        //    if (ware.Any())
        //    {
        //        query = query.Where(a => ware.Contains(a.WarehouseId));
        //    }
        //}
        // 根据仓库ID进行过滤
        //if (!string.IsNullOrWhiteSpace(input.WarehouseId))
        //{
        //    var wareCode = DataContext.WmsBaseWareHouse
        //        .Where(a => a.WarehouseCode == input.WarehouseId)
        //        .Select(a => a.Id)
        //        .FirstOrDefault();

        //    if (wareCode == null)
        //    {
        //        throw new Exception("获取信息失败");
        //    }

        //    query = query.Where(a => a.WarehouseId == wareCode.Trim());
        //}
        //// 根据物料编码进行过滤
        //if (!string.IsNullOrEmpty(model.MaterialCode))
        //{
        //    query = query.Where(a => a.MaterialId.Contains(model.MaterialCode.Trim()));
        //}
        //// 根据物料名称进行过滤
        //if (!string.IsNullOrEmpty(model.MaterialName))
        //{
        //    query = query.Where(a => a.MaterialName.Contains(model.MaterialName.Trim()));
        //}
        //分组并聚合数据
        var cy = query
            .GroupBy(g => new { g.MaterialName, g.WarehouseName })
            .Select(g => new WmsStockOutput
            {
               MaterialName = g.Key.MaterialName,
                StockQuantity = g.Sum(i => i.StockQuantity),
                LockQuantity = g.Sum(i => i.LockQuantity),
                WarehouseName = g.Key.WarehouseName
            })
            .ToPagedList(input.Page, input.PageSize);
        return cy;



    }

    /// <summary>
    /// 物料总量明细查询
    /// </summary>
    [DisplayName("分页查询库存查询")]
    [ApiDescriptionSettings(Name = "GetStockDetailsList"), HttpPost]
    public async Task<SqlSugarPagedList<WmsStockDetailOutput>> GetStockDetailsList(PageWmsStockDetailInput model)
    {
        try
        {
            // 返回数据或为空
            if (string.IsNullOrEmpty(model.MaterialName))
            {
                return null;
            }
            var datalist = await _wmsStockRep.AsQueryable()// 库存表别名s
                                                         // 内连接物料表（复刻视图的INNER JOIN + IsDel=0条件）
                    .InnerJoin<WmsBaseMaterial>( (s, m) => s.MaterialId == m.Id && m.IsDelete == false)
                    // 内连接仓库表（复刻视图的INNER JOIN）
                    .InnerJoin<WmsBaseWareHouse>((s, m, w) => s.WarehouseId == w.Id)
                    // 左连接单位表（复刻视图的LEFT OUTER JOIN）
                    .LeftJoin<WmsBaseUnit>((s, m, w, u) => m.MaterialUnit == u.Id)
                    // 过滤条件：库存表IsDel=0（复刻LINQ的where条件）
                    .Where((s, m, w, u) => s.IsDelete == false)
                    // 动态过滤：物料编码
                    .WhereIF(!string.IsNullOrEmpty(model.MaterialCode),
                        (s, m, w, u) => m.MaterialCode == model.MaterialCode.Trim())
                    // 动态过滤：物料名称
                    .WhereIF(!string.IsNullOrEmpty(model.MaterialName),
                        (s, m, w, u) => m.MaterialName == model.MaterialName.Trim())
                    // 动态过滤：仓库名称（包含查询）
                    .WhereIF(!string.IsNullOrEmpty(model.WarehouseName), (s, m, w, u) => w.WarehouseName.Contains(model.WarehouseName.Trim()))
                    // 选择需要的字段（完全复刻LINQ的select字段）
                    .Select((s, m, w, u) => new
                    {
                        s.Id,
                        s.WarehouseId,
                        w.WarehouseName,
                        m.MaterialName,
                        s.MaterialId,
                        m.MaterialCode,
                        BaseMaterialCode = m.MaterialCode, // 视图中重命名的字段
                        s.LotNo,
                        s.InspectionStatus,
                        m.MaterialStandard,
                        u.UnitName,
                        s.StockQuantity,
                        s.LockQuantity,
                        m.MaterialValidityDay1,
                        m.CreateTime
                    })
                    // 去重（复刻LINQ的Distinct）
                    .Distinct()
                    // 转为列表（异步版本，同步用ToList()）
                    .ToListAsync();
                //// 根据用户权限过滤数据
                //if (datalist.Count() > 0)
                //{
                //    var ware = dal.GetRoleWarehouse(model.userId).Select(a => a.Id).Distinct().ToList();
                //    if (ware.Any())  // 有权限仓库
                //    {
                //        datalist = datalist.Where(a => ware.Contains(a.WarehouseId));
                //    }
                //}

                // 分组并计算合计
                var cy = datalist
                    .GroupBy(g => new
                    {
                        g.MaterialCode,
                        g.MaterialName,
                        g.MaterialStandard,
                        g.UnitName,
                        g.WarehouseName,
                        g.WarehouseId,
                        g.LotNo,
                        g.InspectionStatus,
                        g.BaseMaterialCode,
                        g.MaterialValidityDay1,
                        g.CreateTime
                    })
                    .Select(g => new WmsStockDetailOutput
                    {
                        MaterialCode = g.Key.MaterialCode,
                        MaterialName=g.Key.MaterialName,
                        MaterialStandard = g.Key.MaterialStandard,
                        WarehouseName = g.Key.WarehouseName,
                        WarehouseId=g.Key.WarehouseId.ToString(),
                        UnitName = g.Key.UnitName,
                        LotNo= g.Key.LotNo,
                        InspectionStatus = g.Key.InspectionStatus,
                        BaseMaterialCode = g.Key.BaseMaterialCode,
                        MaterialValidityDay1 = g.Key.MaterialValidityDay1,
                        CreateTime = g.Key.CreateTime,
                        StockQuantity = g.Sum(i => i.StockQuantity),
                        LockQuantity = g.Sum(j => j.LockQuantity)
                    }).OrderBy(g => g.LotNo)
                     .ToPagedList(model.Page, model.PageSize);
                
                return cy;
        }
        catch (Exception ex)
        {
            throw new Exception("物料总量明细查询" + ex.Message);
        }
    }

    /// <summary>
    /// 获取用户对应的仓库
    /// </summary>
    /// <returns></returns>
    public List<WmsBaseWareHouse> GetRoleWarehouse(string userId, string type = "")
    {
        //try
        //{
            //    // 直接关联基础表，替代视图，一步完成所有筛选
            //    var wareHouses = sqlSugarClient.Queryable<WmsSysUser>("a") // 别名a：用户表
            //                                                               // 关联角色表
            //        .LeftJoin<WmsSysRole>("b", (a, b) => a.UserRoleId == b.Id)
            //        // 关联权限表
            //        .LeftJoin<WmsSysRight>("c", (a, b, c) => c.RoleId == b.Id)
            //        // 关联仓库表（ModuleId对应仓库Id）
            //        .LeftJoin<WmsBaseWareHouse>("w", (a, b, c, w) => c.ModuleId == w.Id)
            //        // 核心筛选条件
            //        .Where((a, b, c, w) =>
            //            a.Id == userId          // 匹配指定用户
            //            && c.Type == 1          // 权限类型为1
            //            && w.IsDel == 0)        // 仓库未删除
            //                                    // 处理type的特殊过滤逻辑
            //        .Where((a, b, c, w) =>
            //            (type == "1" && w.Id != "1")  // type=1时排除ID=1的仓库
            //            || (type == "2" && w.Id == "1")) // type=2时只保留ID=1的仓库
            //                                             // 只选择仓库表的字段
            //        .Select<WmsBaseWareHouse>((a, b, c, w) => w)
            //        // 去重（避免关联导致重复数据）
            //        .Distinct()
            //        // 转换为列表
            //        .ToList();

            //    return wareHouses;


            //    var menut = DataContext.View_WmsSysRoleModule.Where(a => a.userId == userId && a.Type == 1).ToList();
            //        List<WmsBaseWareHouse> wareHouses = new List<WmsBaseWareHouse>();
            //        foreach (var item in menut)
            //        {
            //            var ware = DataContext.WmsBaseWareHouse.Where(a => a.Id == item.ModuleId && a.IsDel == 0).FirstOrDefault();
            //            if (ware.Id == "1" && type == "1")
            //            {
            //                continue;
            //            }
            //            if (ware.Id != "1" && type == "2")
            //            {
            //                continue;
            //            }
            //            wareHouses.Add(ware);
            //        }
            //        return wareHouses;
            //}
            //catch (Exception ex)
            //{

            //    throw new Exception(ex.Message);
            //}
            return null;
    }
    /// <summary>
    /// 获取库存查询详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取库存查询详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsStock> Detail([FromQuery] QueryByIdWmsStockInput input)
    {
        return await _wmsStockRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加库存查询 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加库存查询")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsStockInput input)
    {
        var entity = input.Adapt<WmsStock>();
        return await _wmsStockRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新库存查询 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新库存查询")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsStockInput input)
    {
        var entity = input.Adapt<WmsStock>();
        await _wmsStockRep.AsUpdateable(entity)
        .IgnoreColumns(u => new {
            u.MaterialId,
            u.StockQuantity,
            u.LockQuantity,
            u.LotNo,
            u.WarehouseId,
            u.ProductionDate,
            u.ValidateDay,
            u.InspectionStatus,
            u.CustomerId,
            u.ProduceId,
            u.SupplierId,
            u.ManufacturerId,
        })
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除库存查询 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除库存查询")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsStockInput input)
    {
        var entity = await _wmsStockRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsStockRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsStockRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除库存查询 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除库存查询")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsStockInput> input)
    {
        var exp = Expressionable.Create<WmsStock>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsStockRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsStockRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsStockRep.DeleteAsync(list);   //真删除
    }
}
