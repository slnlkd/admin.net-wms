// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Application.Service.BaseService;
using Admin.NET.Core.Service;
using AngleSharp.Dom;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using Microsoft.AspNetCore.Http;
using NewLife.Caching.Clusters;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
namespace Admin.NET.Application;

/// <summary>
/// 移库单据表服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsMoveOrderService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarClient sqlClient;
    private readonly SqlSugarRepository<WmsMoveOrder> _wmsMoveOrderRep;
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWareHouseRep;
    private readonly SqlSugarRepository<WmsBaseLaneway> _wmsBaseLanewayRep;
    private readonly SqlSugarRepository<WmsBaseSlot> _wmsBaseSlotRep;
    private readonly SqlSugarRepository<WmsMoveNotify> _wmsMoveNotifyRep;
    private readonly SqlSugarRepository<WmsStockTray> _wmsStockTrayRep;
    private readonly SqlSugarRepository<WmsMoveTask> _wmsMoveTaskRep;

    public WmsMoveOrderService(ISqlSugarClient SqlSugarClient, SqlSugarRepository<WmsMoveOrder> wmsMoveOrderRep,
        SqlSugarRepository<WmsBaseWareHouse> wmsBaseWareHouseRep,
        SqlSugarRepository<WmsBaseLaneway> wmsBaseLanewayRep,
        SqlSugarRepository<WmsBaseSlot> wmsBaseSlotRep,
        SqlSugarRepository<WmsMoveNotify> wmsMoveNotifyRep,
        SqlSugarRepository<WmsStockTray> wmsStockTrayRep,
        SqlSugarRepository<WmsMoveTask> wmsMoveTaskRep)
    {
        sqlClient = SqlSugarClient;
        _wmsMoveOrderRep = wmsMoveOrderRep;
        _wmsBaseWareHouseRep = wmsBaseWareHouseRep;
        _wmsBaseLanewayRep = wmsBaseLanewayRep;
        _wmsBaseSlotRep = wmsBaseSlotRep;
        _wmsMoveNotifyRep = wmsMoveNotifyRep;
        _wmsStockTrayRep = wmsStockTrayRep;
        _wmsMoveTaskRep = wmsMoveTaskRep;
    }

    /// <summary>
    /// 分页查询移库单据表 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询移库单据表")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsMoveOrderOutput>> Page(PageWmsMoveOrderInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsMoveOrderRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.MoveNo.Contains(input.Keyword) || u.MoveStauts.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MoveNo), u => u.MoveNo.Contains(input.MoveNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MoveStauts), u => u.MoveStauts.Contains(input.MoveStauts.Trim()))
            .Select<WmsMoveOrderOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取移库单据表详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取移库单据表详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsMoveOrder> Detail([FromQuery] QueryByIdWmsMoveOrderInput input)
    {
        return await _wmsMoveOrderRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加移库单据表 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加移库单据表")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsMoveOrderInput input)
    {
        var entity = input.Adapt<WmsMoveOrder>();
        return await _wmsMoveOrderRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新移库单据表 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新移库单据表")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsMoveOrderInput input)
    {
        var entity = input.Adapt<WmsMoveOrder>();
        await _wmsMoveOrderRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除移库单据表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除移库单据表")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsMoveOrderInput input)
    {
        var entity = await _wmsMoveOrderRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsMoveOrderRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsMoveOrderRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除移库单据表 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除移库单据表")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsMoveOrderInput> input)
    {
        var exp = Expressionable.Create<WmsMoveOrder>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsMoveOrderRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsMoveOrderRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsMoveOrderRep.DeleteAsync(list);   //真删除
    }


    /// <summary>
    /// 查询移库单据表
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("查询移库单据表")]
    [ApiDescriptionSettings(Name = "ShowMoveOrder"), HttpPost]
    public async Task<SqlSugarPagedList<ShowMoveOrderOfPage>> ShowMoveOrder(ShowMoveOrderWhere dto)
    {
        try
        {
            var query = await _wmsMoveOrderRep.AsQueryable()
                .Where(x => x.IsDelete == false)
                .WhereIF(string.IsNullOrEmpty(dto.moveNo) == false, x => x.MoveNo.Contains(dto.moveNo))
                .WhereIF(string.IsNullOrEmpty(dto.moveStauts) == false, x => x.MoveStauts == dto.moveStauts)
                .WhereIF(dto.startTime != null && dto.startTime is not null, x => x.CreateTime >= dto.startTime)
                .WhereIF(dto.endTime != null && dto.endTime is not null, x => x.CreateTime <= dto.endTime)
                .OrderBy(x => x.CreateTime)
                .Select(x => new ShowMoveOrderOfPage
                {
                    id = x.Id,
                    moveNo = x.MoveNo,
                    moveStauts = x.MoveStauts,
                    createUserName = x.CreateUserName,
                    createTime = x.CreateTime

                }).ToPagedListAsync(dto.Page, dto.PageSize);

            var listMoveOrder = query.Items;
            foreach (var item in listMoveOrder)
            {
                switch (item.moveStauts)
                {
                    case "0":
                        item.moveStautsStr = "未开始";
                        break;
                    case "1":
                        item.moveStautsStr = "执行中";
                        break;
                    case "2":
                        item.moveStautsStr = "执行完成";
                        break;
                    case "3":
                        item.moveStautsStr = "手动完成";
                        break;
                    case "4":
                        item.moveStautsStr = "手动取消";
                        break;
                    default:

                        break;
                }
            }

            query.Items = listMoveOrder;
            return query;

        }
        catch (Exception ex)
        {
            return new SqlSugarPagedList<ShowMoveOrderOfPage>();
        }
    }


    /// <summary>
    /// 移库仓库下拉框
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("移库仓库下拉框")]
    [ApiDescriptionSettings(Name = "GetWmsBaseWareHouse"), HttpGet]
    public async Task<List<GetWmsBaseWareHouseOfPage>> GetWmsBaseWareHouse()
    {
        try
        {
            return await _wmsBaseWareHouseRep.AsQueryable()
                .Where(x => x.IsDelete == false)
                 .Select(x => new GetWmsBaseWareHouseOfPage
                 {
                     Id = x.Id,
                     WarehouseName = x.WarehouseName


                 }).ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<GetWmsBaseWareHouseOfPage>();
        }
    }

    /// <summary>
    /// 巷道信息下拉框
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("巷道信息下拉框")]
    [ApiDescriptionSettings(Name = "GetWmsBaseLaneway"), HttpPost]
    public async Task<List<GetWmsBaseLanewayOfPage>> GetWmsBaseLaneway(GetWmsBaseLanewayWhere dto)
    {
        try
        {
            return await _wmsBaseLanewayRep.AsQueryable()
                .Where(x => x.IsDelete == false)
                //.WhereIF(dto.OtherId != null && dto.OtherId != -1, x => x.Id != dto.OtherId)
                .WhereIF(dto.WareHouseId != null && dto.WareHouseId != -1, x => x.WarehouseId == dto.WareHouseId)
                .Select(x => new GetWmsBaseLanewayOfPage
                {
                    Id = x.Id,
                    LanewayCode = x.LanewayCode,
                    LanewayName = x.LanewayName
                }).ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<GetWmsBaseLanewayOfPage>();
        }
    }

    /// <summary>
    /// 根据巷道号获取可移出货位
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("根据巷道号获取可移出货位")]
    [ApiDescriptionSettings(Name = "GetSlotIdHaveStock"), HttpPost]
    public async Task<List<GetSlotIdHaveStockOfPage>> GetSlotIdHaveStock(GetSlotIdHaveStockWhere dto)
    {
        try
        {
            var laneInfo = await _wmsBaseLanewayRep.GetFirstAsync(w => w.Id == dto.LaneId);
            return await _wmsStockTrayRep.AsQueryable()
                .LeftJoin<WmsBaseSlot>((a, b) => a.StockSlotCode == b.SlotCode)
                .Where((a, b) => a.StockStatusFlag == 0)
                .Where((a, b) => b.SlotCloseFlag == 0)
                .Where((a, b) => b.SlotLanewayId == dto.LaneId)
                .Where((a, b) => b.SlotStatus == 1)
                .Where((a, b) => b.WarehouseId == laneInfo.WarehouseId)
                .Select((a, b) => new GetSlotIdHaveStockOfPage
                {
                    SlotId = b.Id,
                    SlotCode = b.SlotCode
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<GetSlotIdHaveStockOfPage>();
        }
    }



    /// <summary>
    /// 根据巷道号获取可移入货位
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("根据巷道号获取可移入货位")]
    [ApiDescriptionSettings(Name = "GetEmptySlotId"), HttpPost]
    public async Task<List<GetEmptySlotIdOfPage>> GetEmptySlotId(GetEmptySlotIdWhere dto)
    {
        try
        {
            return await _wmsBaseSlotRep.AsQueryable()
                //.LeftJoin<WmsBaseLaneway>((x, a) => a.WarehouseId == x.WarehouseId)
                .Where((x) => x.IsDelete == false)
                .Where((x) => x.SlotImlockFlag == 0)
                .Where((x) => x.SlotCloseFlag == 0)
                .WhereIF(dto.LaneId != null && dto.LaneId != -1, (x) => x.SlotLanewayId == dto.LaneId)
                .Where((x) => x.SlotStatus == 0)
                //.Where((x,a)=>x.Make=="02")
                .OrderBy((x) => x.Id)
                .Select((x) => new GetEmptySlotIdOfPage
                {
                    SlotId = x.Id,
                    SlotCode = x.SlotCode
                }).ToListAsync();
        }
        catch (Exception ex)
        {
            return new List<GetEmptySlotIdOfPage>();
        }
    }

    /// <summary>
    /// 查询移库物品信息
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("查询移库物品信息")]
    [ApiDescriptionSettings(Name = "GetStockSlotListBySlotId"), HttpPost]
    public async Task<SqlSugarPagedList<ShowMoveOrderDetail>> GetStockSlotListBySlotId(ShowMoveOrderWhere dto)
    {
        try
        {
            var list = await _wmsStockTrayRep.AsQueryable()
                .LeftJoin<WmsBaseMaterial>((a, b) => a.MaterialId == b.Id.ToString())
                .Where((a, b) => a.StockSlotCode == dto.moveOutSlotCode)
                .Select((a, b) => new ShowMoveOrderDetail()
                {
                    StockCode = a.StockCode,
                    StockSlotCode = a.StockSlotCode,
                    LotNo = a.LotNo,
                    StockDate = Convert.ToDateTime(a.StockDate).ToString("yyyy-MM-dd HH:mm:ss"),
                    MaterialId = b.Id,
                    MaterialCode = b.MaterialCode,
                    MaterialName = b.MaterialName,

                }).ToPagedListAsync(dto.Page, dto.PageSize);
            return list;
        }
        catch (Exception ex)
        {
            return new SqlSugarPagedList<ShowMoveOrderDetail>();
        }
    }
    /// <summary>
    /// 添加移库明细
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [DisplayName("添加移库明细")]
    [ApiDescriptionSettings(Name = "AddWmsMoveNotify"), HttpPost, UnitOfWork]
    public async Task<int> AddWmsMoveNotify(WmsMoveNotify dto)
    {
        try
        {

            var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
            #region#生成移库单号
            string moveNo = string.Empty;
            string time = DateTime.Now.ToString("yyyyMMdd");
            var code = _wmsMoveOrderRep.AsQueryable().OrderByDescending(a => a.CreateTime).First();
            if (code == null)
            {
                moveNo = "0000";
                int liushui = Convert.ToInt16(moveNo) + 1;
                moveNo = "YK" + time + Convert.ToString(liushui).PadLeft(4, '0');
            }
            else
            {
                string riqi = code.MoveNo.Substring(2, 8);
                if (riqi == time)
                {
                    moveNo = code.MoveNo.Substring(10, 4);
                    int liushui = Convert.ToInt16(moveNo) + 1;
                    moveNo = "YK" + time + Convert.ToString(liushui).PadLeft(4, '0');
                }
                else
                {
                    moveNo = "0000";
                    int liushui = Convert.ToInt16(moveNo + 1);
                    moveNo = "YK" + time + Convert.ToString(liushui).PadLeft(4, '0');
                }
            }
            #endregion
            //移库总单
            var moveOrder = new WmsMoveOrder();
            moveOrder.MoveNo = moveNo;
            moveOrder.MoveStauts = "02";//正在执行
            moveOrder.CreateUserId = userId;
            moveOrder.UpdateUserId = userId;
            moveOrder.UpdateTime = DateTime.Now;
            await _wmsMoveOrderRep.InsertAsync(moveOrder);

            //移库明细
            dto.MoveBillCode = moveNo;
            dto.MoveExecuteFlag = "02";//正在执行
            dto.Id = SnowFlakeSingle.Instance.NextId();
            await _wmsMoveNotifyRep.InsertAsync(dto);

            //移库任务
            var task = new WmsMoveTask();
            //任务号
            string taskNo = new CommonMethod(sqlClient).GetImExNo("YKR");
            task.TaskNo = taskNo;
            task.Sender = "WMS";
            task.Receiver = "WCS";
            task.SendDate = DateTime.Now;
            task.StockCodeId = dto.StockStockCodeId;
            task.MessageDate = "移库任务";
            task.Msg = dto.MoveOutSlotCode + "货位转移到" + dto.MoveInSlotCode;
            task.IsSend = 0;
            task.IsCancel = 0;
            task.IsFinish = 0;
            task.IsShow = 0;
            task.IsSuccess = 0;
            task.Status = 0;
            await _wmsMoveTaskRep.InsertAsync(task);

            //移出库存信息
            var baseslotOut = _wmsBaseSlotRep.AsQueryable().First(w => w.SlotCode == dto.MoveOutSlotCode);
            baseslotOut.SlotStatus = 5;//正在移出
            //移入库存信息
            var baseslotyiIn= _wmsBaseSlotRep.AsQueryable().First(w => w.SlotCode == dto.MoveInSlotCode);
            baseslotyiIn.SlotStatus = 4;//正在移入

            #region#下发给WCS任务

            #endregion


            return 1;
        }
        catch (Exception ex)
        {
            return 0;
        }
    }
}
