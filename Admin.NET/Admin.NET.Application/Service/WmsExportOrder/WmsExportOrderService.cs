// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Core.Service;
using Dm.util;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using Microsoft.AspNetCore.Http;
using NewLife;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;
using SqlSugar;
using StackExchange.Redis;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace Admin.NET.Application;

/// <summary>
/// 出库流水服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsExportOrderService : IDynamicApiController, ITransient
{
    
    private readonly SqlSugarRepository<WmsExportOrder> _wmsExportOrderRep;
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWareHouseRep;
    private readonly SqlSugarRepository<WmsBaseSlot> _wmsBaseSlotRep;
    private readonly SqlSugarRepository<WmsStockTray> _wmsStockTrayRep;
    private readonly SqlSugarRepository<WmsExportNotify> _wmsExportNotifyRep;
    private readonly SqlSugarRepository<WmsStockInfo> _wmsStockInfoRep;
    private readonly SqlSugarRepository<WmsBaseMaterial> _wmsBaseMaterialRep;
    private readonly SqlSugarRepository<WmsExportNotifyDetail> _wmsExportNotifyDetailRep;
    private readonly SqlSugarRepository<WmsExportTask> _wmsExportTaskRep;
    private readonly SqlSugarRepository<WmsSysStockCode> _wmsSysStockCodeRep;
    private readonly SqlSugarRepository<SysUser> _sysUserRep;
    private readonly SqlSugarRepository<WmsButtonRecord> _wmsButtonRecordRep;
    private readonly SqlSugarRepository<WmsBaseBillType> _wmsBaseBillTypeRep;
    private readonly SqlSugarRepository<WmsExportBoxInfo> _wmsExportBoxInfoRep;
    private readonly SqlSugarRepository<WmsStock> _wmsStockRep;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsExportOrderService(SqlSugarRepository<WmsExportOrder> wmsExportOrderRep,
        SqlSugarRepository<WmsBaseWareHouse> wmsBaseWareHouseRep,
        SqlSugarRepository<WmsBaseSlot> wmsBaseSlotRep,
        SqlSugarRepository<WmsStockTray> wmsStockTrayRep,
        SqlSugarRepository<WmsExportNotify> wmsExportNotifyRep,
        SqlSugarRepository<WmsStockInfo> wmsStockInfoRep,
        SqlSugarRepository<WmsBaseMaterial> wmsBaseMaterialRep,
        SqlSugarRepository<WmsExportNotifyDetail> wmsExportNotifyDetailRep,
        SqlSugarRepository<WmsExportTask> wmsExportTaskRep,
        SqlSugarRepository<WmsSysStockCode> wmsSysStockCodeRep,
        SqlSugarRepository<SysUser> sysUserRep,
        SqlSugarRepository<WmsButtonRecord> wmsButtonRecordRep,
        SqlSugarRepository<WmsBaseBillType> wmsBaseBillTypeRep,
        SqlSugarRepository<WmsExportBoxInfo> wmsExportBoxInfoRep,
        SqlSugarRepository<WmsStock> wmsStockRep,
        ISqlSugarClient sqlSugarClient)
    {
        _wmsExportOrderRep = wmsExportOrderRep;
        wmsBaseWareHouseRep = _wmsBaseWareHouseRep;
        wmsBaseSlotRep = _wmsBaseSlotRep;
        _wmsStockTrayRep = wmsStockTrayRep;
        _wmsExportNotifyRep = wmsExportNotifyRep;
        _wmsStockInfoRep = wmsStockInfoRep;
        _wmsBaseMaterialRep = wmsBaseMaterialRep;
        _wmsExportNotifyDetailRep = wmsExportNotifyDetailRep;
        _wmsExportTaskRep = wmsExportTaskRep;
        _wmsSysStockCodeRep = wmsSysStockCodeRep;
        _sysUserRep = sysUserRep;
        _wmsButtonRecordRep = wmsButtonRecordRep;
        _wmsBaseBillTypeRep = wmsBaseBillTypeRep;
        _wmsExportBoxInfoRep = wmsExportBoxInfoRep;
        _wmsStockRep = wmsStockRep;
        _sqlSugarClient = sqlSugarClient;
    }

    /// <summary>
    /// 分页查询出库流水 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询出库流水")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsExportOrderOutput>> Page(PageWmsExportOrderInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsExportOrderRep.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.ExportOrderNo.Contains(input.Keyword) || u.ExportMaterialCode.Contains(input.Keyword) || u.ExportMaterialName.Contains(input.Keyword) || u.ExportMaterialStandard.Contains(input.Keyword) || u.ExportMaterialModel.Contains(input.Keyword) || u.ExportMaterialType.Contains(input.Keyword) || u.ExportMaterialUnit.Contains(input.Keyword) || u.ExportSlotCode.Contains(input.Keyword) || u.ExportStockCode.Contains(input.Keyword) || u.ExportWeight.Contains(input.Keyword) || u.ExportTaskNo.Contains(input.Keyword) || u.ExportRemark.Contains(input.Keyword) || u.ExportLotNo.Contains(input.Keyword) || u.ExportBillCode.Contains(input.Keyword) || u.ExportListNo.Contains(input.Keyword) || u.ScanUserNames.Contains(input.Keyword) || u.WholeBoxNum.Contains(input.Keyword) || u.StockWholeBoxNum.Contains(input.Keyword) || u.StockQuantity.Contains(input.Keyword) || u.WholeOutWare.Contains(input.Keyword) || u.ExportStockSlotCode.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportOrderNo), u => u.ExportOrderNo.Contains(input.ExportOrderNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportMaterialCode), u => u.ExportMaterialCode.Contains(input.ExportMaterialCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportMaterialName), u => u.ExportMaterialName.Contains(input.ExportMaterialName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportMaterialStandard), u => u.ExportMaterialStandard.Contains(input.ExportMaterialStandard.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportMaterialModel), u => u.ExportMaterialModel.Contains(input.ExportMaterialModel.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportMaterialType), u => u.ExportMaterialType.Contains(input.ExportMaterialType.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportMaterialUnit), u => u.ExportMaterialUnit.Contains(input.ExportMaterialUnit.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportSlotCode), u => u.ExportSlotCode.Contains(input.ExportSlotCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportStockCode), u => u.ExportStockCode.Contains(input.ExportStockCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportWeight), u => u.ExportWeight.Contains(input.ExportWeight.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportTaskNo), u => u.ExportTaskNo.Contains(input.ExportTaskNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportRemark), u => u.ExportRemark.Contains(input.ExportRemark.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportLotNo), u => u.ExportLotNo.Contains(input.ExportLotNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportBillCode), u => u.ExportBillCode.Contains(input.ExportBillCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportListNo), u => u.ExportListNo.Contains(input.ExportListNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ScanUserNames), u => u.ScanUserNames.Contains(input.ScanUserNames.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.WholeBoxNum), u => u.WholeBoxNum.Contains(input.WholeBoxNum.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockWholeBoxNum), u => u.StockWholeBoxNum.Contains(input.StockWholeBoxNum.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.StockQuantity), u => u.StockQuantity.Contains(input.StockQuantity.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.WholeOutWare), u => u.WholeOutWare.Contains(input.WholeOutWare.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.ExportStockSlotCode), u => u.ExportStockSlotCode.Contains(input.ExportStockSlotCode.Trim()))
            .WhereIF(input.ExportBillType != null, u => u.ExportBillType == input.ExportBillType)
            .WhereIF(input.ExportMaterialId != null, u => u.ExportMaterialId == input.ExportMaterialId)
            .WhereIF(input.ExportWarehouseId != null, u => u.ExportWarehouseId == input.ExportWarehouseId)
            .WhereIF(input.ExportAreaId != null, u => u.ExportAreaId == input.ExportAreaId)
            .WhereIF(input.ExportProductionDateRange?.Length == 2, u => u.ExportProductionDate >= input.ExportProductionDateRange[0] && u.ExportProductionDate <= input.ExportProductionDateRange[1])
            .WhereIF(input.ExportLoseDateRange?.Length == 2, u => u.ExportLoseDate >= input.ExportLoseDateRange[0] && u.ExportLoseDate <= input.ExportLoseDateRange[1])
            .WhereIF(input.ExportDepartmentCode != null, u => u.ExportDepartmentCode == input.ExportDepartmentCode)
            .WhereIF(input.ExpotSupplierCode != null, u => u.ExpotSupplierCode == input.ExpotSupplierCode)
            .WhereIF(input.ExportCustomerCode != null, u => u.ExportCustomerCode == input.ExportCustomerCode)
            .WhereIF(input.ExportExecuteFlag != null, u => u.ExportExecuteFlag == input.ExportExecuteFlag)
            .WhereIF(input.ExporOrederDateRange?.Length == 2, u => u.ExporOrederDate >= input.ExporOrederDateRange[0] && u.ExporOrederDate <= input.ExporOrederDateRange[1])
            .WhereIF(input.CompleteDateRange?.Length == 2, u => u.CompleteDate >= input.CompleteDateRange[0] && u.CompleteDate <= input.CompleteDateRange[1])
            .WhereIF(input.ExportDetailId != null, u => u.ExportDetailId == input.ExportDetailId)
            .WhereIF(input.OutType != null, u => u.OutType == input.OutType)
            .WhereIF(input.ExportType != null, u => u.ExportType == input.ExportType)
            .WhereIF(input.InspectionStatus != null, u => u.InspectionStatus == input.InspectionStatus)
            .WhereIF(input.OrderByStatus != null, u => u.OrderByStatus == input.OrderByStatus)
            .Select<WmsExportOrderOutput>();
		return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取出库流水详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取出库流水详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsExportOrder> Detail([FromQuery] QueryByIdWmsExportOrderInput input)
    {
        return await _wmsExportOrderRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加出库流水 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加出库流水")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsExportOrderInput input)
    {
        var entity = input.Adapt<WmsExportOrder>();
        return await _wmsExportOrderRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新出库流水 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新出库流水")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsExportOrderInput input)
    {
        var entity = input.Adapt<WmsExportOrder>();
        await _wmsExportOrderRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除出库流水 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除出库流水")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsExportOrderInput input)
    {
        var entity = await _wmsExportOrderRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsExportOrderRep.FakeDeleteAsync(entity);   //假删除
        //await _wmsExportOrderRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除出库流水 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除出库流水")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsExportOrderInput> input)
    {
        var exp = Expressionable.Create<WmsExportOrder>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsExportOrderRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();
   
        return await _wmsExportOrderRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsExportOrderRep.DeleteAsync(list);   //真删除
    }
    #region 出库流水
    /// <summary>
    /// 获取列表信息。
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取列表信息。")]
    [ApiDescriptionSettings(Name = "GetExportOrderList"), HttpPost, UnitOfWork]
    public async Task<SqlSugarPagedList<View_WmsExportOrder>> GetExportOrderList(ExportOrderSearchModel model)
    {
        //try
        //{
        //    var view_WmsExportOrder = await View_WmsExportOrder();
        //    var list = view_WmsExportOrder.Where(a => a.IsDelete == false).ToList();
        //    if (!string.IsNullOrWhiteSpace(model.ExportExecuteFlag))//执行状态
        //    {
        //        list = list.Where(a => a.IsDelete == false && a.ExportExecuteFlag.ToString() == model.ExportExecuteFlag).ToList();
        //    }
        //    else
        //    {
        //        list = list.Where(a => a.ExportExecuteFlag == 0 || a.ExportExecuteFlag == 1).ToList();
        //    }
        //    if (!string.IsNullOrEmpty(model.ExportOrderNo))//流水单据
        //        list = list.Where(a => a.ExportOrderNo == model.ExportOrderNo).ToList();
        //    if (!string.IsNullOrEmpty(model.ExportBill))//出库单据
        //        list = list.Where(a => a.ExportBillCode == model.ExportBill).ToList();
        //    if (!string.IsNullOrEmpty(model.MaterialName))//物料名称
        //        list = list.Where(a => a.MaterialName.Contains(model.MaterialName.Trim())).ToList();
        //    if (!string.IsNullOrEmpty(model.StockLotNo))//批次
        //        list = list.Where(a => a.ExportLotNo.Contains(model.StockLotNo.Trim())).ToList();
        //    if (!string.IsNullOrEmpty(model.StockSlotCode))//储位
        //        list = list.Where(a => a.ExportSlotCode.Contains(model.StockSlotCode.Trim())).ToList();
        //    if (!string.IsNullOrEmpty(model.StockCode))//托盘号
        //        list = list.Where(a => a.ExportStockCode.Contains(model.StockCode.Trim())).ToList();
        //    if (!string.IsNullOrEmpty(model.StartTime))//开始时间
        //        list = list.Where(a => a.CreateTime >= Convert.ToDateTime(model.StartTime)).ToList();
        //    if (!string.IsNullOrEmpty(model.EndTime))//结束时间
        //    {
        //        var time = Convert.ToDateTime(model.EndTime);
        //        var times = time.AddDays(1);
        //        list = list.Where(a => a.CreateTime <= times).ToList();
        //    }
        //    return list.OrderBy(a => a.ExportOrderNo).ToPagedList(model.Page, model.PageSize);

        //}
        //catch (Exception ex)
        //{

        //    throw new Exception("查询出库流水信息" + ex.Message);
        //}
        return null;
    }


    /// <summary>
    /// 获取出库流水明细列表信息。
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取出库流水明细列表信息。")]
    [ApiDescriptionSettings(Name = "GetExportBoxInfoList"), HttpPost, UnitOfWork]
    public async Task<SqlSugarPagedList<View_WmsExportBoxInfo>> GetExportBoxInfoList(GetExportBoxInfoListWhere dto)
    {
        //try
        //{
        //    var view_WmsExportBoxInfo = await View_WmsExportBoxInfo();
        //    var list = view_WmsExportBoxInfo.Where(a => a.IsDelete == false && a.ExportOrderNo == dto.ExportOrderId && a.MaterialId == dto.ExportMaterialId).OrderBy(p => p.BoxCode).ToList();
        //    return list.ToPagedList(dto.Page, dto.PageSize);
        //}
        //catch (Exception ex)
        //{
        //    throw;
        //}
        return null;
    }

    /// <summary>
    ///下发出库（调用cs接口给他库位地址）
    /// </summary>
    /// <returns></returns>
    [DisplayName("下发出库（调用cs接口给他库位地址）")]
    [ApiDescriptionSettings(Name = "IssueOutBound"), HttpPost, UnitOfWork]
    public async Task<string> IssueOutBound(List<DeleteWmsExportOrderInput> input)
    {
        try
        {
            DateTime strdate = DateTime.Now;
            //var sr = new System.IO.StreamReader(Request.InputStream);
            //var stream = sr.ReadToEnd();
            //JavaScriptSerializer js = new JavaScriptSerializer();
            //var ret = js.Deserialize<ExprotOrderModel>(stream);

            var exp = Expressionable.Create<WmsExportOrder>();
            foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
            var DataList = await _wmsExportOrderRep.AsQueryable().Where(exp.ToExpression())
            .LeftJoin<WmsBaseWareHouse>((u, detail) => u.ExportWarehouseId == detail.Id)
            .Select((u, detail) => new WmsExportOrderOut
            {
                Id = u.Id,
                WarehouseName = detail.WarehouseType,
            }).ToListAsync();

            var list = new List<ExportLibraryDTO>();
            if (DataList.Count() > 0)
            {
                var Ids = new List<string>();
                var wareTypeLTK = DataList.Where(a =>a.WarehouseName== "01").ToList();//判断是否有立体库流水
                if (wareTypeLTK.Count() > 0)
                {
                    wareTypeLTK.ForEach(a => { Ids.Add(a.Id.toString()); });
                    var list1 = await IssueOutBoundNew3(Ids, "");
                    list.AddRange(list1);
                }
                var wareTypeMJK = DataList.Where(a => a.WarehouseName == "02" || a.WarehouseName == "05").ToList();//判断是否有平库流水
                if (wareTypeMJK.Count() > 0)
                {
                    wareTypeMJK.ForEach(a => { Ids.Add(a.Id.toString()); });
                    var list1 = await IssueOutBoundNew4(Ids, "");
                    list.AddRange(list1);

                }
                if (list.Count() != 0)
                {
                    string LogAddress = @"D:\log\WCS\下发出库信息" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    //var jsonData = js.Serialize(list);

                    var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented);

                    if (list[0].HouseCode == "A")
                    {
                        LogAddress = @"D:\log\WCS\血浆库\下发出库信息" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    }
                    else if (list[0].HouseCode == "B")
                    {
                        LogAddress = @"D:\log\WCS\原料库\下发出库信息" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    }
                    else if (list[0].HouseCode == "C")
                    {
                        LogAddress = @"D:\log\WCS\成品库\下发出库信息" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    }
                    //Log.SaveLogToFile("数据：" + jsonData, LogAddress);
                    string response = "";
                    /*   到时候问一下发送接口再做
                    try
                    {
                                                //Log.SaveLogToFile("移库给WCS发送数据：" + jsonData, LogAddress);
                        StringBuilder sbJsonData = new StringBuilder();
                        sbJsonData.Append("移库给WCS发送数据：" + jsonData);
                        await Admin.NET.Core.FileHelper.WriteToFileAsync(LogAddress, sbJsonData);

                        response = HttpHelper.DoPost(ApiUrl.GetWcsHost() + ApiUrl.TaskApiUrl, jsonData);
                        //解析返回数据
                        JObject obj = JObject.Parse(response);
                        var wcsModel = JsonConvert.DeserializeObject<WcsReturnModel>(response);
                        if (wcsModel.stateCode != "0")
                        {
                            Log.SaveLogToFile("反馈数据：" + wcsModel, LogAddress);
                            foreach (var item in list)
                            {
                                //更改任务状态
                                dal.EditExTaskStatus(item.TaskNo, 1);
                            }
                            result.Data = new { code = 1, msg = "下发出库成功", data = list };
                        }
                        else
                        {
                            Log.SaveLogToFile("反馈失败数据：" + response, LogAddress);
                            result.Data = new { code = 0, msg = "下发出库失败，" + wcsModel.errMsg, data = list };
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.SaveLogToFile("反馈异常数据：" + ex.Message, LogAddress);
                        result.Data = new { code = 0, msg = ex.Message, data = list };
                    }
                    */
                    //result.Data = new { code = 1, msg = "下发出库成功", data = list };
                }
                else
                {
                    return "暂无可下发出库任务";
                }
            }
            else
            {
                return "获取数据失败";

            }
            return "发送成功";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }




    /// <summary>
    ///撤销出库流水
    /// </summary>
    /// <returns></returns>
    [DisplayName("撤销出库流水")]
    [ApiDescriptionSettings(Name = "ExportUndo"), HttpPost, UnitOfWork]
    public async Task<string> ExportUndo(ExOrderUndo ret)
    {
        try
        {
            string res = string.Empty;
            if (!string.IsNullOrEmpty(ret.ExportTypeName) && ret.ExportTypeName == "托盘调整")
            {
                res = await CancelPalletAllocate2(ret.OrderNo);//撤销托盘调整
            }
            else
            {
                res = await CancelPalletAllocate(ret.OrderNo);
            }
            if (res == "ok")
            {
                return "操作完成";
            }
            else
            {
                return res;
            }

        }
        catch (Exception ex)
        {
            throw;
        }
    }
    #endregion

    #region 下发立体库出库



    /// <summary>
    /// 获取移库目标库位
    /// </summary>
    /// <param name="oldAddre">需要移动的库位地址</param>
    /// <returns>目标库位地址 为"" 直接下发两次出库指令</returns>
    public async Task<string> MoveAddre(string oldAddre)//A01020261
    {
        string nowAddre = "";
        // 获取移库目标储位
        var sqlString = string.Empty;
        var roadway = oldAddre.Substring(0, 3);
        var row = int.Parse(oldAddre.Substring(3, 2));
        var lie = int.Parse(oldAddre.Substring(5, 2));
        var ceng = int.Parse(oldAddre.Substring(7, 2));
        WmsExportTask moveTask = new WmsExportTask();

        //using (LinqModelDataContext DataContext = new LinqModelDataContext(DataConnection.GetDataConnection))
        //{
        var slotList = await _wmsBaseSlotRep.AsQueryable().Where(m => m.SlotCode == oldAddre).ToListAsync();
        var slot = slotList.FirstOrDefault();
        var wareList = await _wmsBaseWareHouseRep.AsQueryable().ToListAsync();
        var ware = wareList.FirstOrDefault(a => a.Id == slot.WarehouseId);
        sqlString = "select slotCode,SlotRow,SlotColumn,SlotLayer,";
        sqlString += $"(ABS(SlotRow-{slot.SlotRow}) + ABS(SlotColumn-{slot.SlotColumn}) + ABS(SlotLayer-{slot.SlotLayer}))  as distNum ";
        sqlString += "from WmsBaseSlot ";
        //sqlString += $"where /*Property = '03' and SlotStatus in (0,3,5) and SlotInout = 2 and SlotCode like '{roadway}%' and SlotHigh='"+
        sqlString += $"where /*Property = '03' and*/ SlotStatus in (0) and make='01' and SlotStatus='0'  and SlotInout = 2 and SlotCode like '{roadway/*ware.WarehouseCode*/}%' and SlotHigh='" +
        slot.SlotHigh + "' and SlotAreaId='" + slot.SlotAreaId + "' ";
        sqlString += "order by distNum";

        //var addreModels = DataContext.ExecuteQuery<addreClass>(sqlString).ToList();


        var addreModels = _sqlSugarClient.Ado.SqlQuery<addreClass>(sqlString).ToList();



        if (addreModels.Count > 0)   // 判断同巷道内排空库位
        {
            // 目标内库位对应的外库位
            string addrewai = addreModels[0].slotCode.Substring(0, addreModels[0].slotCode.Length - 1) + "1";

            // 判断目标库位的外库位是否存在物料   正在移入情况不存在
            //SlotStatus 0: 空储位 1：有货  2：正在入库  3：正在出库   4：正在移入  5：正在移出
            sqlString = "select count(*) from WmsBaseSlot where slotCode = " + addrewai + " and  SlotStatus in ('1','2') and make='01' /*and Property = '03'*/ ";


            //var rowNum = DataContext.ExecuteQuery<int>(sqlString, addrewai).ToInt();
            var rowNum = _sqlSugarClient.Ado.SqlQuery<int>(sqlString).ToInt();


            if (rowNum > 0)
            {
                // 获取外库位对应的托盘号
                sqlString = " select StockStockCode from WmsStockTray where StockSlotCode = " + addrewai + "";
                //var stockTrays = DataContext.ExecuteQuery<WmsStockTray>(sqlString, addrewai).ToList();
                var stockTrays = _sqlSugarClient.Ado.SqlQuery<WmsStockTray>(sqlString).ToList();


                moveTask.ExportTaskNo = await GetImExNo("CKR");
                // 下发目标库位的外库位移到目标库位  addrewai => addreModels[0].slotCode
                ExportLibraryDTO moveCmd = new ExportLibraryDTO
                {
                    TaskBegin = addrewai,                                        // 库位地址
                    StockCode = stockTrays[0].StockCode,                   // 托盘号            
                    TaskNo = moveTask.ExportTaskNo,                                   // 任务号
                    TaskType = "2",                                             // 任务类型
                    TaskEnd = addreModels[0].slotCode                    // 目标库位
                };

                // 调用WCS接口下发指令
                try
                {
                    moveTask.Sender = "WMS";                                                            //发出方 
                    moveTask.Receiver = "WCS";                                                          //接受方
                    moveTask.IsSuccess = 0;                                                             //是否成功 -1: 失败  0:发出  1：成功
                                                                                                        //moveTask.MessageDate = "下达出库指令";                                              //报文描述
                    moveTask.StockCode = stockTrays[0].StockCode;                                //托盘码
                    moveTask.Msg = "空托出库产生的移库指令：由:" + addrewai + "移至:" + addreModels[0].slotCode;                                     //关键信息

                    //string json = JsonConvert.SerializeObject(moveCmd);
                    //var response = Utility.Extra.HttpHelper.DoPost(WcsApiUrl.GetHost() + WcsApiUrl.TaskApiUrl, json);     // 下发移库指令
                    moveTask.BackDate = DateTime.Now;                                                   //返回时间 
                                                                                                        //var wcsModel = JsonConvert.DeserializeObject<WcsReturnModel>(response);

                    moveTask.IsSuccess = 1;                                                             // 是否成功
                                                                                                        //moveTask.Information = wcsModel.errMsg;
                    var slotChangeList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                    var slotChange = slotChangeList.FirstOrDefault(m => m.SlotCode == addrewai);
                    var slotChange2List = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                    var slotChange2 = slotChange2List.FirstOrDefault(m => m.SlotCode == addreModels[0].slotCode);
                    slotChange.SlotStatus = 5;
                    slotChange2.SlotStatus = 4;
                    //DataContext.SubmitChanges();

                }
                catch (Exception ex)
                {
                    moveTask.BackDate = DateTime.Now;
                    moveTask.IsSuccess = -1;                                                            // 是否成功
                    moveTask.Information = ex.Message.ToString();
                }
                //this.AddTasks(moveTask);

                nowAddre = addrewai;
            }
            else
            {
                nowAddre = addreModels[0].slotCode;
            }
        }
        else
        {
            // 判断同巷道外排空库位
            sqlString = "select slotCode,SlotRow,SlotColumn,SlotLayer,";
            sqlString += $"(ABS(SlotRow-{slot.SlotRow}) + ABS(SlotColumn-{slot.SlotColumn}) + ABS(SlotLayer-{slot.SlotLayer})) as distNum ";
            sqlString += "from WmsBaseSlot ";
            sqlString += "where SlotStatus in (0) and SlotInout = '1' and make='01' ";
            sqlString += $"and SlotCode like '{roadway}%'";
            sqlString += $"and SlotHigh='" + slot.SlotHigh + "' and SlotAreaId='" + slot.SlotAreaId + "'";
            sqlString += "order by distNum;";
            //var addreClasses = DataContext.ExecuteQuery<addreClass>(sqlString).ToList();

            var addreClasses = _sqlSugarClient.Ado.SqlQuery<addreClass>(sqlString).ToList();

            if (addreClasses.Count > 0)
            {
                nowAddre = addreClasses[0].slotCode;
            }
            else
            {
                // 库内不存在空储位  
                nowAddre = "";
            }
        }

        return nowAddre;
    }



    /// <summary>
    /// 下发立体库出库
    /// </summary>
    /// <param name="exportOrderIds">流水Id</param>
    /// <param name="getJsonData"></param>
    /// <returns></returns>
    public async Task<List<ExportLibraryDTO>> IssueOutBoundNew3(List<string> exportOrderIds, string pickArea)
    {
        try
        {

            //开始事务
            //DataContext.Connection?.Open();
            //var tran = DataContext.Connection.BeginTransaction();
            //DataContext.Transaction = tran;
            await  _sqlSugarClient.Ado.BeginTranAsync();

            #region 集合
            var outBeforeDto = new List<ExportLibraryDTO>(); //先出库数据的集合（深度为1的储位）
            var outAfterDto = new List<ExportLibraryDTO>(); //后出库数据的集合（深度为2的储位）
            var moveDto = new List<ExportLibraryDTO>(); //要移库数据的集合
            var isErr = string.Empty;
            #endregion
            try
            {
                //所有要出库的出库流水单

                var list = await _wmsExportOrderRep.AsQueryable()
                    .LeftJoin<WmsBaseSlot>((a, g) => a.ExportSlotCode == g.SlotCode)
                    .Where((a, g) => a.IsDelete == false && exportOrderIds.Contains(a.Id.ToString()) && a.ExportExecuteFlag == 0)
                    .OrderBy((a, g) => a.ExportSlotCode)
                    .OrderBy((a, g) => g.SlotInout)
                    .ToListAsync();

                var listData = list.GroupBy(m => new { m.ExportStockCode, m.ExportSlotCode, m.ExportWarehouseId });
                List<WmsExportTask> taskList = new List<WmsExportTask>();


                //listData = from a in listData//新增
                //           join g in DataContext.WmsBaseSlot on a.Key.ExportSlotCode equals g.SlotCode
                //           orderby g.SlotInout
                //           select a;






                foreach (var demo in listData)
                {
                    //判断当前出库流水中的托盘是否在库内
                    var tuoPanList = await _wmsStockTrayRep.AsQueryable().ToListAsync();
                    var tuoPan = tuoPanList.FirstOrDefault(m => m.StockCode == demo.Key.ExportStockCode);
                    var baseSlotList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                    var baseSlotAsync = baseSlotList.FirstOrDefault(m => m.SlotCode == tuoPan.StockSlotCode);
                    if (tuoPan == null || string.IsNullOrEmpty(tuoPan.StockSlotCode) || baseSlotAsync == null)//不在仓库内,直接跳出
                    {
                        continue;
                    }
                    foreach (var item in demo)
                    {
                        var exportDetail1List = await _wmsExportNotifyRep.AsQueryable().ToListAsync();
                        var exportDetail1 = exportDetail1List.FirstOrDefault(m => m.ExportBillCode == item.ExportBillCode.ToString());

                        //添加流水单
                        string exportTaskCode = string.Empty;
                        if (taskList.Count < 1)
                        {
                            exportTaskCode = await GetImExNo("CKR");
                        }
                        else
                        {
                            var lastExportTask = taskList.LastOrDefault();
                            var num = Convert.ToInt64(lastExportTask.ExportTaskNo.Substring(3));
                            num++;
                            exportTaskCode = $"CKR{num}";
                        }
                        var outSlotList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                        var outSlot = outSlotList.FirstOrDefault(m => m.SlotCode == item.ExportSlotCode);
                        if (outSlot.SlotStatus == 1)//验证当前的储位是否正是有货状态
                        {
                            #region 出库区域                                        
                            var HouseCodeList = await _wmsBaseWareHouseRep.AsQueryable().Where(p => p.Id == outSlot.WarehouseId && p.IsDelete == false).ToListAsync();
                            var HouseCode = HouseCodeList.Where(p => p.Id == outSlot.WarehouseId && p.IsDelete == false).FirstOrDefault();//所属仓库
                            if (HouseCode == null)
                            {
                                throw new Exception("获取当前仓库信息失败");
                            }
                            #endregion

                            if (outSlot.SlotInout == 2)
                            {
                                var slotBeforeList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                                var slotBefore = slotBeforeList.FirstOrDefault(m => m.SlotInout == 1 && m.SlotRow == outSlot.SlotRow && m.SlotColumn == outSlot.SlotColumn && m.SlotLayer == outSlot.SlotLayer && m.WarehouseId == outSlot.WarehouseId && m.SlotLanewayId == outSlot.SlotLanewayId);
                                // 获取要出库深度为2储位前面的储位信息
                                //var slotBefore = OutsideOfItself(outSlot);
                                if (slotBefore.SlotStatus == 1) //前面的储位有物料、进行移库操作
                                {
                                    //去库存表中找到储位对应的托盘码操作
                                    var stocknewList = await _wmsStockTrayRep.AsQueryable().ToListAsync();
                                    var stocknew = stocknewList.FirstOrDefault(m => m.StockSlotCode == slotBefore.SlotCode);
                                    if (stocknew == null)
                                    {
                                        var slotChangeList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                                        var slotChange = slotChangeList.FirstOrDefault(m => m.SlotCode == slotBefore.SlotCode);
                                        slotChange.SlotStatus = 0;
                                        // DataContext.SubmitChanges();
                                    }
                                    else
                                    {
                                        var newSlot = await MoveAddre(slotBefore.SlotCode);//寻找移库库位
                                        if (!string.IsNullOrEmpty(newSlot))
                                        {
                                            #region 添加出库时发生的移库任务
                                            var VehicleSub = await _wmsStockTrayRep.AsQueryable().Where(a => a.StockSlotCode == slotBefore.SlotCode).ToListAsync();
                                            foreach (var itemSub in VehicleSub)
                                            {
                                                //主副托盘0:主托盘，1:副托盘
                                                string zftp = "1";
                                                //判断主次托盘
                                                if (itemSub.StockCode == itemSub.VehicleSubId)
                                                {
                                                    zftp = "0";
                                                }
                                                var exTaskList = taskList.FirstOrDefault(a => a.ExportTaskNo == exportTaskCode);
                                                if (exTaskList != null && taskList.Count() > 0)
                                                {
                                                    var lastExportTask = taskList.LastOrDefault();
                                                    var num = Convert.ToInt64(lastExportTask.ExportTaskNo.Substring(3));
                                                    num++;
                                                    exportTaskCode = $"CKR{num}";
                                                }
                                                var exYkTask1 = new WmsExportTask
                                                {
                                                    Id = SnowFlakeSingle.Instance.NextId(),
                                                    ExportTaskNo = exportTaskCode,
                                                    ExportTaskFlag = 0,
                                                    Sender = "WMS",//发出方 
                                                    Receiver = "WCS",//接受方
                                                    IsSuccess = 0,//是否成功 ----成功后更改
                                                    SendDate = DateTime.Now,//发送时间
                                                    BackDate = null,//返回时间 ----下面更改
                                                    StockCode = itemSub.StockCode,//托盘码
                                                    Msg = $"由{slotBefore.SlotCode}储位移动到{newSlot}储位",//关键信息
                                                    IsDelete = false,//是否删除                                                      
                                                    WarehouseId = HouseCode.Id,
                                                    StartLocation = slotBefore.SlotCode,
                                                    EndLocation = newSlot,
                                                    CreateTime = DateTime.Now,
                                                    TaskType = 2,
                                                    ExportOrderNo = item.ExportOrderNo,
                                                };

                                                //dataContext.WmsExportTask.InsertOnSubmit(exYkTask1);
                                                //dataContext.SubmitChanges();
                                                taskList.Add(exYkTask1);
                                                #endregion
                                                moveDto.Add(new ExportLibraryDTO()
                                                {
                                                    BillCode = item.ExportBillCode.toString(),
                                                    TaskBegin = slotBefore.SlotCode, // 储位号
                                                    StockCode = VehicleSub[0].StockCode, // 托盘号
                                                    TaskNo = exportTaskCode, // 移库任务号
                                                    TaskType = "3", // 任务类型 (移库)
                                                    HouseCode = HouseCode.WarehouseCode,
                                                    TaskEnd = newSlot, //移库的目标储位（位置）
                                                    IsExportStockTary = zftp//0:主托盘，1:副托盘
                                                });

                                            }

                                            var SlotChangeList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                                            var SlotChange = SlotChangeList.FirstOrDefault(m => m.Id == slotBefore.Id);

                                            var slotChangeList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                                            var slotChange2 = slotChangeList.FirstOrDefault(m =>
                                                     m.SlotCode == newSlot);
                                            SlotChange.SlotStatus = 5;//改变状态（正在移库）
                                            slotChange2.SlotStatus = 4; // 改变状态（正在移入）
                                            //DataContext.SubmitChanges();
                                        }
                                    }
                                }
                            }
                            int IsExportStockTary = 0;////0:主托盘；1:副托盘
                            var stockTrays = await _wmsStockTrayRep.AsQueryable().Where(m => m.VehicleSubId == item.ExportStockCode).ToListAsync();//先查询主载具为当前流水的载具
                            if (stockTrays == null || stockTrays.Count < 1)//为空查找载具为当前流水的载具
                            {
                                stockTrays = await _wmsStockTrayRep.AsQueryable().Where(m => m.StockCode == item.ExportStockCode).ToListAsync();
                                if (stockTrays != null && stockTrays.Count > 0)//查询当前载具的主载具号
                                {
                                    var stockTray1 = new List<WmsStockTray>();
                                    stockTray1 = await _wmsStockTrayRep.AsQueryable().Where(m => m.VehicleSubId == stockTrays[0].VehicleSubId).ToListAsync();
                                    if (stockTray1 != null && stockTray1.Count > 0) //不为空的话直接根据当前主载具创建两条任务
                                    {
                                        stockTrays = stockTray1;
                                    }
                                }
                            }
                            if (stockTrays.Count() > 0)
                            {
                                foreach (var itemTary in stockTrays)
                                {
                                    IsExportStockTary = 0;
                                    //判断主次托盘
                                    if (itemTary.StockCode != itemTary.VehicleSubId)
                                        IsExportStockTary = 1;
                                    var exTaskList = taskList.FirstOrDefault(a => a.ExportTaskNo == exportTaskCode);
                                    if (exTaskList != null && taskList.Count() > 0)
                                    {
                                        var lastExportTask = taskList.LastOrDefault();
                                        var num = Convert.ToInt64(lastExportTask.ExportTaskNo.Substring(3));
                                        num++;
                                        exportTaskCode = $"CKR{num}";
                                    }
                                    #region 添加出库任务
                                    //出库任务
                                    var exTask1 = new WmsExportTask();
                                    exTask1.Id = SnowFlakeSingle.Instance.NextId();
                                    exTask1.ExportTaskNo = exportTaskCode;
                                    exTask1.Sender = "WMS";//发出方 
                                    exTask1.Receiver = "WCS";//接受方
                                    exTask1.ExportTaskFlag = 0;
                                    exTask1.ExportOrderNo = item.ExportOrderNo;
                                    exTask1.IsSuccess = 0;
                                    exTask1.SendDate = DateTime.Now; //发送时间
                                    exTask1.BackDate = null;//返回时间 ----下面更改
                                    exTask1.StockCode = itemTary.StockCode;//托盘码
                                    exTask1.Msg = itemTary.StockCode + $" 目标出库口为" + item.WholeOutWare + "出库口";
                                    exTask1.IsDelete = false;//是否删除
                                    exTask1.WarehouseId = HouseCode.Id;
                                    exTask1.StartLocation = outSlot.SlotCode;
                                    exTask1.EndLocation = item.WholeOutWare;
                                    exTask1.TaskType = 1;
                                    exTask1.CreateTime = DateTime.Now;
                                    //dataContext.WmsExportTask.InsertOnSubmit(exTask1);
                                    //dataContext.SubmitChanges();                                              
                                    taskList.Add(exTask1);
                                    List<StockBoxInfo> boxInfos1 = new List<StockBoxInfo>();
                                    var StockInfos = await _wmsStockInfoRep.AsQueryable().Where(a => a.TrayId == itemTary.Id.ToString()).ToListAsync();
                                    var MaterialCodeList = await _wmsBaseMaterialRep.AsQueryable().ToListAsync();
                                    foreach (var stockInfo in StockInfos)
                                    {
                                        var stockBoxInfo = new StockBoxInfo();
                                        stockBoxInfo.BoxCode = stockInfo.BoxCode;
                                        stockBoxInfo.LotNo = stockInfo.LotNo;
                                        stockBoxInfo.Qty = (decimal)stockInfo.Qty;
                                        stockBoxInfo.MaterialCode = MaterialCodeList.FirstOrDefault(a => a.Id.ToString() == stockInfo.MaterialId).MaterialCode;
                                        stockBoxInfo.ProductionDate = stockInfo.ProductionDate.ToString();
                                        stockBoxInfo.ValidateDay = stockInfo.ValidateDay.ToString();
                                        stockBoxInfo.BulkTank = 0;
                                        stockBoxInfo.SamplingDate = stockInfo.SamplingDate.ToString();
                                        stockBoxInfo.StaffCode = stockInfo.StaffCode;
                                        stockBoxInfo.StaffName = stockInfo.StaffName;
                                        //stockBoxInfo.Weight = stockInfo.Weight.ToString();
                                        if (string.IsNullOrWhiteSpace(stockInfo.PickingSlurry))
                                        {
                                            stockBoxInfo.PickingSlurry = "0";
                                        }
                                        else
                                        {
                                            stockBoxInfo.PickingSlurry = stockInfo.PickingSlurry.ToString();
                                        }
                                        if (string.IsNullOrWhiteSpace(stockInfo.ExtractStatus.ToString()))
                                        {
                                            stockBoxInfo.ExtractStatus = null;
                                        }
                                        stockBoxInfo.InspectionStatus = stockInfo.InspectionStatus.ToString();
                                        boxInfos1.Add(stockBoxInfo);
                                    }
                                    //outBeforeDto.Add(new ExportLibraryDTO()
                                    //出库中移库任务顺序应在出库任务之前
                                    outAfterDto.Add(new ExportLibraryDTO()
                                    {
                                        TaskBegin = outSlot.SlotCode, // 储位号
                                        StockCode = itemTary.StockCode, // 托盘号
                                        TaskNo = exTask1.ExportTaskNo, // 任务号
                                        TaskType = "1",// 任务类型 (出库)
                                        HouseCode = HouseCode.WarehouseCode,
                                        Qty = Convert.ToDecimal(itemTary.LockQuantity),
                                        WholeBoxNum = itemTary.LockQuantity.ToString(),
                                        BillCode = item.ExportBillCode.toString(),
                                        TaskEnd = item.WholeOutWare,  //目标地址
                                        boxInfos = boxInfos1,
                                        IsExportStockTary = IsExportStockTary.ToString()//0:出库拣货托盘；1:出库跟随托盘
                                    });
                                    item.ExportTaskNo = exTask1.ExportTaskNo;
                                    #endregion
                                }
                            }
                            //要出库的储位改变状态
                            var sList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                            var s = sList.FirstOrDefault(m => m.SlotCode == outSlot.SlotCode);
                            s.SlotStatus = 3; //正在出库

                            var orderList = await _wmsExportOrderRep.AsQueryable().ToListAsync();
                            var order = orderList.FirstOrDefault(m => m.Id == item.Id);
                            order.ExportExecuteFlag = 1; // 出库流水状态改为正在执行 
                            if (order.ExportType != 5 && order.ExportType != 6)//5为换托盘箱，没有出库单据,6为暂存出库
                            {
                                var exportDetailList = await _wmsExportNotifyDetailRep.AsQueryable().ToListAsync();
                                var exportDetail = exportDetailList.FirstOrDefault(m => m.Id == item.ExportDetailId);
                                exportDetail.FactQuantity += Convert.ToDecimal(item.PickedNum);
                                exportDetail.ExportDetailFlag = 2;
                                //DataContext.SubmitChanges();

                                var exNotifyList = await _wmsExportNotifyRep.AsQueryable().ToListAsync();
                                var exNotify = exNotifyList.FirstOrDefault(m => m.IsDelete == false && m.ExportBillCode == item.ExportBillCode.ToString());
                                if (exNotify != null && exNotify.ExportExecuteFlag == 0 || exNotify.ExportExecuteFlag == 1)
                                {
                                    var detailList = await _wmsExportNotifyDetailRep.AsQueryable().Where(m => m.IsDelete == false && m.ExportBillCode == item.ExportBillCode).ToListAsync();
                                    if (detailList.Count() > 0)
                                    {
                                        exNotify.ExportExecuteFlag = 2;
                                        //DataContext.SubmitChanges();
                                    }
                                }
                            }
                        }
                        else if (outSlot.SlotStatus == 3) //正在出库的
                        {
                            var orderList = await _wmsExportOrderRep.AsQueryable().ToListAsync();
                            var order = orderList.FirstOrDefault(m => m.Id == item.Id);
                            if (order.ExportType != 5 && order.ExportType != 6)
                            {
                                var exNotifyList = await _wmsExportNotifyRep.AsQueryable().ToListAsync();
                                var exNotify = exNotifyList.FirstOrDefault(m => m.IsDelete == false && m.ExportBillCode == item.ExportBillCode.ToString());
                                if (exNotify != null && exNotify.ExportExecuteFlag == 0 || exNotify.ExportExecuteFlag == 1)
                                {
                                    var detailList = await _wmsExportNotifyDetailRep.AsQueryable().Where(m => m.IsDelete == false && m.ExportBillCode.ToString() == exNotify.ExportBillCode && m.ExportDetailFlag == 1).ToListAsync();
                                    if (detailList.Count() > 0)
                                    {
                                        //血浆出库后直接把单据状态改为完成
                                        var exportDetail = detailList.FirstOrDefault(m => m.Id == item.ExportDetailId);
                                        exportDetail.CompleteQuantity += Convert.ToDecimal(item.PickedNum);
                                        exportDetail.ExportDetailFlag = 2;
                                    }
                                }
                            }
                            order.ExportExecuteFlag = 1; // 出库流水状态改为正在执行                                  
                        }
                    }
                }
                foreach (var itemTask in taskList)
                {
                    // DataContext.WmsExportTask.InsertOnSubmit(itemTask);
                    await _wmsExportTaskRep.InsertAsync(itemTask);
                }
                //DataContext.SubmitChanges();
                //tran.Commit();

            }
            catch (Exception e)
            {
                _sqlSugarClient.Ado.RollbackTranAsync();
                isErr = e.Message;
            }
            finally
            {
                //DataContext.Connection.Close();
                _sqlSugarClient.Ado.CommitTranAsync();
            }

            #region MyRegion
            //先移库后出库
            outBeforeDto.AddRange(moveDto);
            outBeforeDto.AddRange(outAfterDto);
            return outBeforeDto;
            #endregion

        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    #region 下发自动化平库出库
    /// <summary>
    /// 下发自动化平库出库
    /// </summary>
    /// <param name="exportOrderIds"></param>
    /// <param name="pickArea"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<ExportLibraryDTO>> IssueOutBoundNew4(List<string> exportOrderIds, string pickArea)
    {
        #region 集合
        var slotDataNo = new List<string>(); //因状态不能出库的集合
        var outBeforeDto = new List<ExportLibraryDTO>(); //先出库数据的集合（深度为1的储位）
        var backUpDtos = new List<BackInfoDTO>(); //移回集合

        #endregion

        //开始事务
        //DataContext.Connection?.Open();
        //var tran = DataContext.Connection.BeginTransaction();
        //DataContext.Transaction = tran;

       await  _sqlSugarClient.Ado.BeginTranAsync();

        try
        {

            //var list = DataContext.WmsExportOrder.Where(a => a.IsDel == 0 && exportOrderIds.Contains(a.Id) && a.ExportExecuteFlag == 0/*0待执行*/).ToList().OrderBy(a => a.ExportSlotCode/*储位编码*/);
            ////根据托盘条码，储位编码，流水状态分组
            //var listData = list.GroupBy(m => new { m.ExportStockCode/*托盘条码*/, m.ExportSlotCode, m.ExportWarehouseId });

            //所有要出库的出库流水单  
            //var listData1 = await _wmsExportOrderRep.AsQueryable()
            //    .LeftJoin<WmsBaseSlot>((a, g) => a.ExportSlotCode == g.SlotCode)
            //    .Where((a, g) => a.IsDelete == false && exportOrderIds.Contains(a.Id.ToString()) && a.ExportExecuteFlag == 0/*0待执行*/)
            //    .GroupBy((a, g) => a.ExportStockCode)
            //    .GroupBy((a, g) => a.ExportSlotCode)
            //    .GroupBy((a, g) => a.ExportWarehouseId)
            //    .OrderBy((a, g) => a.ExportSlotCode/*储位编码*/)
            //    .Select((a, g) => new listData1
            //    {
            //        ExportStockCode = a.ExportStockCode,
            //        ExportSlotCode = a.ExportSlotCode,
            //        ExportWarehouseId = a.ExportWarehouseId,
            //    })
            //    .ToListAsync();
            //var listData1 = await _wmsExportOrderRep.AsQueryable()
            //                        .Where(a =>
            //                            a.IsDelete == false
            //                            && exportOrderIds.Contains(a.Id.ToString())
            //                            && a.ExportExecuteFlag == 0 // 0待执行
            //                        )
            //                        // 复合分组：按三个字段一起分组（核心修正：替换连续GroupBy）
            //                        .GroupBy(a => new
            //                        {
            //                            a.ExportStockCode,  // 托盘条码
            //                            a.ExportSlotCode,   // 储位编码
            //                            a.ExportWarehouseId // 仓库ID
            //                        })
            //                        // 按储位编码排序
            //                        .OrderBy(a => a.ExportSlotCode)
            //                        // 投影分组后的核心字段
            //                        .Select(a => new
            //                        {
            //                            ExportStockCode = a.ExportStockCode,
            //                            ExportSlotCode = a.ExportSlotCode,
            //                            ExportWarehouseId = a.ExportWarehouseId
            //                        })
            //                        // 左连接WmsBaseSlot（直接在数据库层面关联）
            //                        .LeftJoin<WmsBaseSlot>((a, g) => a.ExportSlotCode == g.SlotCode)
            //                        .Select((a, g) => new
            //                        {
            //                            a.ExportStockCode,
            //                            a.ExportSlotCode,
            //                            g.SlotLanewayId,
            //                            g.SlotInout,
            //                            a.ExportWarehouseId,
            //                            g.SlotLayer
            //                        })
            //                        .ToListAsync();
            // 第一步：查询所有待出库的流水单（修正：ToList()后再排序，且排序返回List）
            var list = await _wmsExportOrderRep.AsQueryable()
                .Where(a => a.IsDelete == false && exportOrderIds.Contains(a.Id.ToString()) && a.ExportExecuteFlag == 0)
                .ToListAsync(); // 先异步查询数据库
            var sortedList = list.OrderBy(a => a.ExportSlotCode).ToList(); // 内存中按储位编码排序
            // 第二步：按托盘条码、储位编码、仓库ID分组
            var listData = sortedList.GroupBy(m => new
            {
                m.ExportStockCode,
                m.ExportSlotCode,
                m.ExportWarehouseId
            }); 
            // 第三步：获取货位表数据（内存中关联需先加载货位表）
            var wmsBaseSlotList = await _wmsBaseSlotRep.AsQueryable().ToListAsync(); // 假设你有货位表仓储
             // 第四步：内存中左连接WmsBaseSlot（替代SqlSugar的LeftJoin）
            var listData1 = listData
                // 内存左连接核心：GroupJoin + SelectMany + DefaultIfEmpty
                .GroupJoin(
                    wmsBaseSlotList, // 被关联的货位表数据
                    a => a.Key.ExportSlotCode, // 分组集合的关联键（储位编码）
                    g => g.SlotCode, // 货位表的关联键
                    (a, gGroup) => new { GroupData = a, SlotList = gGroup } // 关联结果：分组数据 + 匹配的货位列表
                )
                .SelectMany(
                    x => x.SlotList.DefaultIfEmpty(), // 左连接：无匹配货位时返回null
                    (x, g) => new // 投影最终字段
                    {
                        x.GroupData.Key.ExportStockCode,
                        x.GroupData.Key.ExportSlotCode,
                        SlotLanewayId = g?.SlotLanewayId, // 空值保护：无匹配货位时为null
                        SlotInout = g?.SlotInout,
                        x.GroupData.Key.ExportWarehouseId,
                        SlotLayer = g?.SlotLayer
                    }
                );
            // 第五步：按巷道ID、仓库ID二次分组
            var listdata2 = listData1.GroupBy(a => new
            {
                a.SlotLanewayId,
                a.ExportWarehouseId
            }).ToList(); // 转为List方便后续操作
            //var listdata2 = listData1.GroupBy(a => new { a.SlotLanewayId, a.ExportWarehouseId });

            List<WmsExportTask> taskList = new List<WmsExportTask>();
            var backTaskList = new List<WmsExportTask>();

            foreach (var item1 in listdata2)
            {
                var newListData = from a in listData//新增
                                  join g in wmsBaseSlotList on a.Key.ExportSlotCode equals g.SlotCode
                                  orderby g.SlotInout
                                  where g.SlotLanewayId == item1.Key.SlotLanewayId && g.WarehouseId == item1.Key.ExportWarehouseId
                                  select a;


                //var newListData = await _wmsExportOrderRep.AsQueryable()
                //    .LeftJoin<WmsBaseSlot>((a, g) => a.ExportSlotCode == g.SlotCode)
                //    .Where((a, g) => a.IsDelete == false && exportOrderIds.Contains(a.Id.ToString()) && a.ExportExecuteFlag == 0/*0待执行*/)
                //    .Where((a, g) => g.SlotLanewayId == item1.Key.SlotLanewayId && g.WarehouseId == item1.Key.ExportWarehouseId)
                //    .GroupBy((a, g) => a.ExportStockCode)
                //    .GroupBy((a, g) => a.ExportSlotCode)
                //    .GroupBy((a, g) => a.ExportWarehouseId)
                //    .OrderBy((a, g) => a.ExportSlotCode/*储位编码*/)
                //    .OrderBy((a, g) => g.SlotInout)
                //    .ToListAsync();

                foreach (var demo in newListData)
                {
                    //判断当前出库流水中的托盘是否在库内
                    var tuoPanList = await _wmsStockTrayRep.AsQueryable().ToListAsync();
                    var tuoPan = tuoPanList.FirstOrDefault(m => m.StockCode == demo.Key.ExportStockCode);
                    var baseSlotList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                    if (tuoPan == null || string.IsNullOrEmpty(tuoPan.StockSlotCode) || baseSlotList.FirstOrDefault(m => m.SlotCode == tuoPan.StockSlotCode) == null)//不在仓库内,直接跳出
                    {
                        continue;
                    }
                    foreach (var item in demo)
                    {
                        //根据单号查询单据信息
                        //var exportDetail1 = DataContext.WmsExportNotify.FirstOrDefault(m => m.ExportBillCode == item.ExportBillCode);
                        #region 判断流水中的储位和库存中的储位是否一致（看是否被移库）
                        //库位类型（01存储库位 02 中转库位）字典表类型Make
                        var outSlotSum = await _wmsBaseSlotRep.AsQueryable().Where(a => (a.SlotStatus != 8/*8储位不存在*/ || a.SlotStatus != 9/*9异常储位*/) && a.Make != "02" /*&& a.SlotImlockFlag == 0*/&& a.WarehouseId == item.ExportWarehouseId).ToListAsync();
                        var outSlot = outSlotSum.FirstOrDefault(m => m.SlotCode/*储位编码*/ == item.ExportSlotCode);
                        if (outSlot == null)
                        {
                            throw new Exception(item.ExportSlotCode + " 储位获取失败");
                        }
                        //switch (outSlot.SlotAreaId)
                        //{
                        //    case 1300000000101://常温库
                        //        pickArea = "B01";
                        //        break;
                        //    case 1300000000102://阴凉库
                        //        pickArea = "C01";
                        //        break;
                        //}

                        //从库存中获取并验证储位是否正确（看是否被移库）
                        var stockTrayList = await _wmsStockTrayRep.AsQueryable().ToListAsync();
                        var stockTray = stockTrayList.FirstOrDefault(m => m.StockCode == item.ExportStockCode);
                        var stockCodeList = await _wmsSysStockCodeRep.AsQueryable().ToListAsync();
                        var stockCode = stockCodeList.FirstOrDefault(n => n.StockCode == stockTray.StockCode);
                        if (stockTray.StockSlotCode != item.ExportSlotCode)
                        {
                            outSlot = outSlotSum.FirstOrDefault(m => m.SlotCode == stockTray.StockSlotCode);
                        }
                        #endregion
                        if (outSlot.SlotStatus == 1)//验证当前的储位是否正是有货状态
                        {
                            //SlotStatus 储位状态(0空储位、1 有物品、2 正在入库、3正在出库、4 正在移入、5正在移出、6空载具、7 屏蔽、8储位不存在、9异常储位）
                            //SlotInout 深度(1靠近堆垛机一侧储位，2远离堆垛机一侧储位)                                               
                            var dangLuSlot1 = outSlotSum.Where(a => /*a.WarehouseId == outSlot.WarehouseId &&*/ a.SlotAreaId == outSlot.SlotAreaId/*区域ID*/ && (a.SlotStatus == 2/*正在入库*/ || a.SlotStatus == 4/*正在移入*/ || a.SlotStatus == 6/*空载具*/) && a.SlotInout < outSlot.SlotInout && a.SlotLanewayId == outSlot.SlotLanewayId/*巷道ID*/ && a.SlotLayer == outSlot.SlotLayer/*层*/).OrderBy(a => a.SlotInout).ToList();
                            if (dangLuSlot1.Count > 0)//判断是否有正在移入，正在入库等数据，有的话直接跳出，避免下面找移库任务时找不到
                            {
                                break;
                            }
                            //判断任务列表当前巷道是否有移回任务，有的话跳出
                            var dangLuSlot2 = outSlotSum.Where(a => a.WarehouseId == outSlot.WarehouseId && a.SlotAreaId == outSlot.SlotAreaId && a.SlotLanewayId == outSlot.SlotLanewayId && a.SlotInout < outSlot.SlotInout && a.SlotLayer == outSlot.SlotLayer).ToList();
                            if (dangLuSlot2.Count > 0)
                            {
                                bool isBreak = false;
                                foreach (var danglu in dangLuSlot2)
                                {
                                    var exportTask = await _wmsExportTaskRep.AsQueryable().Where(a => a.EndLocation == danglu.SlotCode && a.ExportTaskFlag < 2).ToListAsync();
                                    if (exportTask != null && exportTask.Count > 0)
                                    {
                                        isBreak = true;
                                        break;
                                    }
                                }
                                if (isBreak)
                                {
                                    break;
                                }
                            }
                            var HouseCodeList = await _wmsBaseWareHouseRep.AsQueryable().ToListAsync();
                            var HouseCode = HouseCodeList.Where(p => p.Id == outSlot.WarehouseId).FirstOrDefault();//所属仓库
                            string ExportTaskNo;
                            if (taskList.Count < 1)
                            {
                                ExportTaskNo = await GetImExNo("CKR");
                            }
                            else
                            {
                                var lastOrder = taskList.LastOrDefault();
                                var num = Convert.ToInt64(lastOrder.ExportTaskNo.Substring(3));
                                num++;
                                ExportTaskNo = $"CKR{num}";
                            }

                            var exckTask1 = new WmsExportTask
                            {
                                Id = SnowFlakeSingle.Instance.NextId(),
                                ExportTaskNo = ExportTaskNo,
                                Sender = "WMS",//发出方 
                                Receiver = "WCS",//接受方
                                IsSuccess = 0,//是否成功 ----成功后更改
                                Information = "",//异常信息  ----有异常更改
                                SendDate = DateTime.Now,//发送时间
                                BackDate = null,//返回时间 ----下面更改                                              
                                StockCode = tuoPan.StockCode,//托盘码                                                                             
                                Msg = "由" + outSlot.SlotCode + "储位出库",
                                IsDelete = false,//是否删除
                                CreateTime = DateTime.Now,//创建日期
                                CreateUserId = 0,//创建人
                                                 //UpdateTime = null,//更改日期
                                                 //UpdateUserId = null,//更新人
                                ExportOrderNo = item.ExportOrderNo,//出库单号
                                WarehouseId = HouseCode.Id,
                                StartLocation = outSlot.SlotCode,
                                EndLocation = item.WholeOutWare,//pickArea,
                                TaskType = 1
                            };
                            //DataContext.WmsExportTask.InsertOnSubmit(exckTask1);
                            taskList.Add(exckTask1);

                            //查询出所有需要移库的储位信息                                                                                                   
                            //SlotInout 深度(1靠近堆垛机一侧储位，2远离堆垛机一侧储位)                               //SlotStatus 1 有物品
                            var dangLuSlot = outSlotSum.Where(a => a.SlotAreaId == outSlot.SlotAreaId/*区域ID*/ && a.SlotStatus == 1 && a.SlotInout < outSlot.SlotInout && a.SlotLanewayId == outSlot.SlotLanewayId/*巷道ID*/ && a.SlotLayer == outSlot.SlotLayer/*层*/).OrderBy(a => a.SlotInout).ToList();

                            if (dangLuSlot.Count > 0)
                            {
                                foreach (var item2 in dangLuSlot)
                                {
                                    //获取当前

                                    var outSlotSumGroup = outSlotSum.Where(a => a.SlotImlockFlag != 1 && a.SlotExlockFlag != 1 && a.SlotCloseFlag != 1).OrderBy(x => x.SlotLayer).GroupBy(a => a.SlotLayer);
                                    int max = outSlotSum.Where(a => a.SlotAreaId == outSlot.SlotAreaId).Max(a => Convert.ToInt32(a.SlotLanewayId));//最大的巷道
                                    int min = outSlotSum.Where(a => a.SlotAreaId == outSlot.SlotAreaId).Min(a => Convert.ToInt32(a.SlotLanewayId));//最小的巷道
                                    int SlotLayer = outSlotSum.Max(a => (int)a.SlotLayer);//获取最大的层数
                                    YiKuModel slot = null;
                                    int layer = 1;//查询出库层数
                                    int count = 1;//循环第一次
                                    foreach (var itemGroup in outSlotSumGroup)
                                    {
                                        var num = 1;
                                        #region 判断层数
                                        //判断当前出库层数
                                        if (outSlot.SlotLayer == 1)
                                        {
                                            //当层数等于1，循环第一次一进去，循环第二次时加1
                                            if (count > 1)
                                            {
                                                layer++;
                                            }
                                        }
                                        else if (outSlot.SlotLayer == 2)
                                        {
                                            //层数等于2，第一次循环不进去，第二次循环先找3层，第三次循环找1层，第四次找4层
                                            layer = (int)outSlot.SlotLayer;
                                            //先查等于2层，然后在查3层，再查一层，最后4层
                                            if (count == 2)
                                            {
                                                layer = 3;
                                            }
                                            else if (count == 3)
                                            {
                                                layer = 1;
                                            }
                                            else if (count == 4)
                                            {
                                                layer = 4;
                                            }
                                        }
                                        else if (outSlot.SlotLayer == 3)
                                        {
                                            //层数等于3，第一次循环不进去，第二次循环先找4层，第三次循环找2层，第四次找1层
                                            layer = (int)outSlot.SlotLayer;
                                            if (count == 2)
                                            {
                                                layer = 4;
                                            }
                                            else if (count == 3)
                                            {
                                                layer = 2;
                                            }
                                            else if (count == 4)
                                            {
                                                layer = 1;
                                            }
                                        }
                                        else if (outSlot.SlotLayer == 4)
                                        {
                                            //循环第四层，当循环次数大于1时，代表4层满了，找第三层
                                            if (count > 1)
                                            {
                                                layer--;
                                            }
                                            else
                                            {
                                                //第一次循环先找第四层
                                                layer = (int)outSlot.SlotLayer;
                                            }
                                        }
                                        count++;
                                        #endregion
                                        for (int i = 0; i < max; i++)
                                        {
                                            //if (num > max)
                                            //{
                                            //    throw new Exception("未找到合适的移库储位");
                                            //}
                                            string sqlString2 = string.Empty;
                                            var sql = Convert.ToInt32(outSlot.SlotLanewayId/*巷道*/) + num;
                                            if (max >= sql)
                                            {
                                                //根据巷道查询相邻的巷道，如果找不到直接跳出
                                                sqlString2 = @"select a.SlotCode,a.WarehouseId,a.SlotLanewayId,a.SlotStatus,a.SlotInout,a.SlotAreaId,a.EndTransitLocation,a.SlotLayer from WmsBaseSlot a left join WmsBaseLaneway b on a.SlotLanewayId=b.Id  
  where a.WarehouseId=" + outSlot.WarehouseId + " and a.SlotLanewayId =" + sql + " and a.Make='01' and a.SlotAreaId=" + item2.SlotAreaId + " and SlotStatus =0 and SlotStatus!=7 and SlotStatus!=8 and SlotStatus!=9 and SlotCloseFlag = 0 and SlotImlockFlag = 0  and SlotExlockFlag = 0 and SlotLayer=" + layer + " order by SlotInout desc";


                                                //slot = DataContext.ExecuteQuery<YiKuModel>(sqlString2, outSlot.WarehouseId, sql, item2.SlotAreaId).FirstOrDefault();

                                                slot = _sqlSugarClient.Ado.SqlQuery<YiKuModel>(sqlString2).ToList().FirstOrDefault();

                                                if (slot != null)
                                                {
                                                    var preslotList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                                                    var preslot = preslotList.FirstOrDefault(m => m.SlotStatus == 1 && m.SlotInout < slot.SlotInout && m.SlotLanewayId == slot.SlotLanewayId && m.SlotLayer == slot.SlotLayer);



                                                    if (preslot == null)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                            sql = Convert.ToInt32(outSlot.SlotLanewayId) - num;
                                            if (min <= sql)
                                            {
                                                //根据巷道查询相邻的巷道，如果找不到直接跳出
                                                sqlString2 = @"select a.SlotCode,a.WarehouseId,a.SlotLanewayId,a.SlotStatus,a.SlotInout,a.SlotAreaId,a.EndTransitLocation,a.SlotLayer from WmsBaseSlot a left join WmsBaseLaneway b on a.SlotLanewayId=b.Id  
  where a.WarehouseId=" + outSlot.WarehouseId + " and a.SlotLanewayId =" + sql + " and a.Make='01' and a.SlotAreaId=" + item2.SlotAreaId + " and SlotStatus =0 and SlotStatus!=7 and SlotStatus!=8 and SlotStatus!=9 and SlotCloseFlag = 0 and SlotImlockFlag = 0  and SlotExlockFlag = 0 and SlotLayer=" + layer + " order by SlotInout desc";

                                                //slot = DataContext.ExecuteQuery<YiKuModel>(sqlString2, outSlot.WarehouseId, sql, item2.SlotAreaId).FirstOrDefault();
                                                slot = _sqlSugarClient.Ado.SqlQuery<YiKuModel>(sqlString2).ToList().FirstOrDefault();


                                                if (slot != null)
                                                {
                                                    var preslotList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                                                    var preslot = preslotList.FirstOrDefault(m => m.SlotStatus == 1 && m.SlotInout < slot.SlotInout && m.SlotLanewayId == slot.SlotLanewayId && m.SlotLayer == slot.SlotLayer);
                                                    if (preslot == null)
                                                    {
                                                        break;
                                                    }
                                                }
                                            }
                                            num++;
                                        }
                                        if (slot != null)
                                        {
                                            break;
                                        }
                                    }

                                    //判断储位变量不为空，执行增加临时表和移库任务表，更新状态
                                    if (slot != null)
                                    {
                                        var tuopanhaoList = await _wmsStockTrayRep.AsQueryable().Where(a => a.StockSlotCode == item2.SlotCode).ToListAsync();
                                        var tuopanhao = tuopanhaoList.FirstOrDefault();

                                        #region 添加出库时发生的移库任务

                                        var lastOrder = taskList.LastOrDefault();
                                        var num1 = Convert.ToInt64(lastOrder.ExportTaskNo.Substring(3));
                                        num1++;
                                        ExportTaskNo = $"CKR{num1}";
                                        var exYkTask1 = new WmsExportTask
                                        {
                                            Id = SnowFlakeSingle.Instance.NextId(),
                                            ExportTaskNo = ExportTaskNo,
                                            Sender = "WMS",//发出方 
                                            Receiver = "WCS",//接受方
                                            IsSuccess = 0,//是否成功 ----成功后更改
                                            Information = "",//异常信息  ----有异常更改
                                            SendDate = DateTime.Now,//发送时间
                                            BackDate = null,//返回时间 ----下面更改
                                            StockCode = tuopanhao.StockCode,//托盘码
                                            Msg = $"由{item2.SlotCode}储位移动到{slot.SlotCode}储位",//关键信息  
                                            IsDelete = false,//是否删除
                                            CreateTime = DateTime.Now,//创建日期
                                            CreateUserId = 0,//创建人
                                                             //UpdateTime = null,//更改日期
                                                             //UpdateUser = "",//更新人
                                            WarehouseId = HouseCode.Id,
                                            StartLocation = item2.SlotCode,
                                            EndLocation = slot.SlotCode,
                                            TaskType = 2,
                                            ExportOrderNo = item.ExportOrderNo
                                        };

                                        //DataContext.WmsExportTask.InsertOnSubmit(exYkTask1);
                                        taskList.Add(exYkTask1);
                                        #endregion

                                        //更新储位状态
                                        var SlotChangeList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                                        var SlotChange = SlotChangeList.FirstOrDefault(m => m.SlotCode == item2.SlotCode);
                                        var slotChange2 = SlotChangeList.FirstOrDefault(m => m.SlotCode == slot.SlotCode);

                                        SlotChange.SlotStatus = 5;//改变状态（正在出库）
                                        slotChange2.SlotStatus = 4; // 改变状态（正在移入）
                                        await _wmsBaseSlotRep.UpdateAsync(SlotChange);
                                        await _wmsBaseSlotRep.UpdateAsync(slotChange2);
                                        //添加移库数据
                                        outBeforeDto.Add(new ExportLibraryDTO()
                                        {
                                            BillCode = item.ExportBillCode.toString(),
                                            TaskBegin = item2.SlotCode, // 原始储位号
                                            StockCode = tuopanhao.StockCode, // 托盘号
                                            TaskNo = exYkTask1.ExportTaskNo, // 移库任务号
                                            TaskType = "3", // 任务类型 (移库)
                                            HouseCode = HouseCode.WarehouseCode,
                                            TaskEnd = slot.SlotCode, //移库的目标储位（位置）
                                            LanewayId = item2.SlotLanewayId.toString(),
                                            beginTransitLocation = item2.EndTransitLocation,//起始中转位置
                                            EndTransitLocation = slot.EndTransitLocation//目的中转位置
                                        });
                                        //DataContext.SubmitChanges();

                                        //1.先记录移除的货位，等此巷道所有出库完成之后，操作回移计算

                                        backUpDtos.Add(new BackInfoDTO()
                                        {
                                            TaskNo = $"CKR{++num1}",
                                            TrayNo = tuopanhao.StockCode,
                                            MoveSlot = item2,
                                            YiKuSlot = slot,
                                            ExportNo = item.ExportOrderNo.ToString(),
                                            ExportBillCode = item.ExportBillCode.ToString()
                                        });
                                    }
                                    else
                                    {
                                        throw new Exception("未找到合适的移库储位,请稍候再试!");
                                    }

                                }
                            }
                            #region 添加出库任务

                            List<StockBoxInfo> boxInfos1 = new List<StockBoxInfo>();
                            var StockInfos = await _wmsStockInfoRep.AsQueryable().Where(a => a.TrayId == tuoPan.Id.ToString()).ToListAsync();
                            var materialList = await _wmsBaseMaterialRep.AsQueryable().ToListAsync();
                            foreach (var stockInfo in StockInfos)
                            {
                                var stockBoxInfo = new StockBoxInfo();
                                stockBoxInfo.BoxCode = stockInfo.BoxCode;
                                stockBoxInfo.LotNo = stockInfo.LotNo;
                                stockBoxInfo.Qty = (decimal)stockInfo.Qty;
                                stockBoxInfo.MaterialCode = materialList.FirstOrDefault(a => a.Id.ToString() == stockInfo.MaterialId).MaterialCode;
                                stockBoxInfo.ProductionDate = stockInfo.ProductionDate.ToString();
                                stockBoxInfo.ValidateDay = stockInfo.ValidateDay.ToString();
                                stockBoxInfo.BulkTank = (int)stockInfo.IsFractionBox;
                                stockBoxInfo.RFIDCode = stockInfo.RFIDCode;
                                stockBoxInfo.SamplingDate = stockInfo.SamplingDate.ToString();
                                stockBoxInfo.StaffCode = stockInfo.StaffCode;
                                stockBoxInfo.StaffName = stockInfo.StaffName;
                                //stockBoxInfo.Weight = stockInfo.Weight.ToString();
                                //stockBoxInfo.outQty = stockInfo.outQty.ToString();
                                stockBoxInfo.PickingSlurry = "0";
                                stockBoxInfo.ExtractStatus = stockInfo.ExtractStatus.ToString();
                                boxInfos1.Add(stockBoxInfo);
                            }
                            outBeforeDto.Add(new ExportLibraryDTO()
                            {
                                TaskBegin = outSlot.SlotCode, // 储位号
                                StockCode = item.ExportStockCode, // 托盘号
                                TaskNo = exckTask1.ExportTaskNo, // 任务号
                                TaskType = "1",// 任务类型 (出库)
                                HouseCode = HouseCode.WarehouseCode,
                                Qty = Convert.ToDecimal(item.WholeBoxNum),
                                WholeBoxNum = item.StockWholeBoxNum.ToString(),
                                BillCode = item.ExportBillCode.ToString(),
                                TaskEnd = item.WholeOutWare,// pickArea,  //目标地址
                                boxInfos = boxInfos1,
                                LanewayId = outSlot.SlotLanewayId.ToString(),
                                beginTransitLocation = outSlot.EndTransitLocation
                            });


                            #endregion

                            item.ExportTaskNo = exckTask1.ExportTaskNo;
                            exckTask1.ExportOrderNo = item.ExportOrderNo;

                            //DataContext.SubmitChanges();

                            #region 改变数据

                            //要出库的储位改变状态
                            var sList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                            var s = sList.FirstOrDefault(m => m.SlotCode == outSlot.SlotCode);
                            s.SlotStatus = 3; //正在出库
                            await _wmsBaseSlotRep.UpdateAsync(s);
                            var orderList = await _wmsExportOrderRep.AsQueryable().ToListAsync();
                            var order = orderList.FirstOrDefault(m => m.Id == item.Id);
                            order.ExportExecuteFlag = 1; // 出库流水状态改为正在执行
                            await _wmsExportOrderRep.UpdateAsync(order);
                            if (order.ExportType != 4 && order.ExportType != 5)//备料数据直接出库，不再占用已下发数量；5为换托盘箱，没有出库单据
                            {
                                var exportDetailList = await _wmsExportNotifyDetailRep.AsQueryable().ToListAsync();
                                var exportDetail = exportDetailList.FirstOrDefault(m => m.Id == item.ExportDetailId);
                                exportDetail.ExportDetailFlag = 2;
                                exportDetail.FactQuantity += Convert.ToDecimal(item.PickedNum);
                                await _wmsExportNotifyDetailRep.UpdateAsync(exportDetail);
                            }
                            //DataContext.SubmitChanges();
                            if (order.ExportType != 5)//5为换托盘箱，没有出库单据
                            {
                                var exNotifyList = await _wmsExportNotifyRep.AsQueryable().ToListAsync();
                                var exNotify = exNotifyList.FirstOrDefault(m => m.IsDelete == false && m.ExportBillCode == item.ExportBillCode.ToString());
                                if (exNotify != null && exNotify.ExportExecuteFlag == 0 || exNotify.ExportExecuteFlag == 1)
                                {
                                    exNotify.ExportExecuteFlag = 2;
                                    await _wmsExportNotifyRep.UpdateAsync(exNotify);                                     
                                }
                            }
                            #endregion

                        }
                        else if (outSlot.SlotStatus == 3) //正在出库的
                        {
                            var orderList = await _wmsExportOrderRep.AsQueryable().ToListAsync();
                            var order = orderList.FirstOrDefault(m => m.Id == item.Id);
                            order.ExportExecuteFlag = 1; // 出库流水状态改为正在执行
                            await _wmsExportOrderRep.UpdateAsync(order);
                            var exNotifyList = await _wmsExportNotifyRep.AsQueryable().ToListAsync();
                            var exNotify = exNotifyList.FirstOrDefault(m => m.IsDelete == false && m.ExportBillCode == item.ExportBillCode.ToString());
                            if (exNotify != null && exNotify.ExportExecuteFlag == 0 || exNotify.ExportExecuteFlag == 1)
                            {
                                var detailList = await _wmsExportNotifyDetailRep.AsQueryable().Where(m => m.IsDelete == false && m.ExportBillCode.ToString() == exNotify.ExportBillCode && m.ExportDetailFlag == 1).ToListAsync();
                                if (detailList.Count() > 0)
                                {
                                    //血浆出库后直接把单据状态改为正在执行
                                    var exportDetail = detailList.FirstOrDefault(m => m.Id == item.ExportDetailId);
                                    exportDetail.CompleteQuantity += Convert.ToDecimal(item.PickedNum);
                                    exportDetail.ExportDetailFlag = 2;
                                    await _wmsExportNotifyDetailRep.UpdateAsync(exportDetail);
                                }
                                exNotify.ExportExecuteFlag = 2;
                                await _wmsExportNotifyRep.UpdateAsync(exNotify);
                            }
                        }
                    }
                }
                //移回库位
                if (backUpDtos != null && backUpDtos.Count > 0)
                {
                    backUpDtos.Reverse();

                    for (int i = 0; i < backUpDtos.Count; i++)
                    {
                        var backDto = backUpDtos[i];

                        var backToSlotList = await _wmsBaseSlotRep.AsQueryable().ToListAsync();
                        var backToSlot = backToSlotList
                            .Where(m => m.SlotLanewayId == backDto.MoveSlot.SlotLanewayId && m.SlotLayer == backDto.MoveSlot.SlotLayer && m.SlotInout > backDto.MoveSlot.SlotInout
                            && m.WarehouseId == backDto.MoveSlot.WarehouseId && m.SlotAreaId == backDto.MoveSlot.SlotAreaId && (m.SlotStatus == 0 || m.SlotStatus == 3 || m.SlotStatus == 5) && m.Make == "01")
                            .OrderByDescending(m => m.SlotInout).OrderByDescending(m => m.SlotInout).ToList()[i];

                        taskList.Add(new WmsExportTask
                        {
                            Id = SnowFlakeSingle.Instance.NextId(),
                            ExportTaskNo = backDto.TaskNo,
                            Sender = "WMS",//发出方 
                            Receiver = "WCS",//接受方
                            IsSuccess = 0,//是否成功 ----成功后更改
                            Information = "",//异常信息  ----有异常更改
                            SendDate = DateTime.Now,//发送时间
                            BackDate = null,//返回时间 ----下面更改
                            StockCode = backDto.TrayNo,//托盘码
                            Msg = $"由{backDto.YiKuSlot.SlotCode}储位移动到{backToSlot.SlotCode}储位",//关键信息  
                            IsDelete = false,//是否删除
                            CreateTime = DateTime.Now,//创建日期
                            CreateUserId = 0,//创建人
                            //UpdateTime = null,//更改日期
                            //UpdateUser = "",//更新人
                            WarehouseId = backDto.YiKuSlot.WarehouseId,
                            StartLocation = backDto.YiKuSlot.SlotCode,
                            EndLocation = backToSlot.SlotCode,
                            TaskType = 2,
                            ExportOrderNo = backDto.ExportNo
                        });

                        outBeforeDto.Add(new ExportLibraryDTO()
                        {
                            BillCode = backDto.ExportBillCode,
                            TaskBegin = backDto.YiKuSlot.SlotCode, // 原始储位号
                            StockCode = backDto.TrayNo, // 托盘号
                            TaskNo = backDto.TaskNo, // 移库任务号
                            TaskType = "3", // 任务类型 (移库)
                            HouseCode = backDto.MoveSlot.WarehouseId.ToString(),
                            TaskEnd = backToSlot.SlotCode, //移库的目标储位（位置）
                            LanewayId = backDto.MoveSlot.SlotLanewayId.ToString(),
                            beginTransitLocation = backDto.YiKuSlot.EndTransitLocation,//目的中转位置
                            EndTransitLocation = backToSlot.EndTransitLocation//起始中转位置 

                        });

                    }
                    backUpDtos.Clear();
                }
            }
            #region MyRegion
            foreach (var itemTask in taskList)
            {
                itemTask.ExportTaskFlag = 0;
                //DataContext.WmsExportTask.InsertOnSubmit(itemTask);
                await _wmsExportTaskRep.InsertAsync(itemTask);
            }
            //DataContext.SubmitChanges();
            //tran.Commit();
            await _sqlSugarClient.Ado.CommitTranAsync();
            return outBeforeDto;
            #endregion
        }
        catch (Exception ex)
        {
            //tran.Rollback();
            await _sqlSugarClient.Ado.RollbackTranAsync();
            throw new Exception(ex.Message);
        }
        finally
        {
            //DataContext.Connection.Close();
        }

    }

    #endregion

    #endregion


    #region 撤销出库流水
    /// <summary>
    /// 撤销出库流水
    /// </summary>
    /// <param name="orderNo"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<string> CancelPalletAllocate(string orderNo)
    {
        try
        {
            await _sqlSugarClient.Ado.BeginTranAsync();
            var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
            var orderList = await _wmsExportOrderRep.AsQueryable().Where(o => o.ExportOrderNo.ToString() == orderNo && o.ExportExecuteFlag != 4).ToListAsync();
            var order = orderList.FirstOrDefault();
            if (order.ExportExecuteFlag != 0)
            {
                return "当前出库流水单据不是等待执行！";
            }
            var notifyList = await _wmsExportNotifyRep.AsQueryable().Where(n => n.ExportBillCode == order.ExportBillCode.ToString()).ToListAsync();
            var notify = notifyList.FirstOrDefault();
            //
            var pq = await _wmsStockTrayRep.AsQueryable().Where(t => t.MaterialId == order.ExportMaterialId.ToString()
                        /*&& t.StockSlotCode == order.ExportSlotCode*/ && t.StockCode == order.ExportStockCode && t.InspectionStatus == order.InspectionStatus).ToListAsync();
            if (pq == null || pq.Count() < 1)
            {
                return "库存查找失败！";
            }
            if (!string.IsNullOrWhiteSpace(order.ExportLotNo))
            {
                pq = pq.Where(t => t.LotNo == order.ExportLotNo).ToList();
            }
            else
            {
                pq = pq.Where(t => t.LotNo == null || t.LotNo == "").ToList();
            }
            #region 代储
            var billTypeList = await _wmsBaseBillTypeRep.AsQueryable().ToListAsync();
            var billType = billTypeList.FirstOrDefault(a => a.Id == notify.ExportBillType);
            if (billType != null)
            {
                if (billType.ProxyStatus == 1)//判断当前单据类型是否为代储
                {
                    if (notify.ExportCustomerId != null && notify.ExportCustomerId != -1)//判断客户是否为空
                    {
                        pq = pq.Where(s => s.OwnerCode == notify.ExportCustomerId.ToString()).ToList();
                    }
                    else
                    {
                        return "当前" + notify.ExportCustomerId + "库存信息中为空";
                    }
                }
                //else
                //{
                //    if (!string.IsNullOrWhiteSpace(notify.ExportCustomerId))
                //    {
                //        pq = pq.Where(s => s.CustomerId == notify.ExportCustomerId);
                //    }
                //    else
                //    {
                //        pq = pq.Where(s => s.CustomerId == string.Empty || s.CustomerId == "" || s.CustomerId == null);
                //    }
                //}
            }
            else
            {
                return "当前" + notify.ExportBillType + "单据类型获取失败"; ;
            }
            #endregion

            var pallet = pq.FirstOrDefault();
            pallet.StockQuantity += order.ExportQuantity.Value;
            pallet.LockQuantity -= order.ExportQuantity.Value;
            await _wmsStockTrayRep.UpdateAsync(pallet);
            var eboxs = await _wmsExportBoxInfoRep.AsQueryable().Where(b => b.ExportOrderNo == order.ExportOrderNo.ToString() && b.PickNum > 0).ToListAsync();
            foreach (var e in eboxs)
            {
                e.IsDelete = true;
                var fd = new WmsStockInfo();
                if (!string.IsNullOrEmpty(e.BoxCode))
                {
                    var fdList = await _wmsStockInfoRep.AsQueryable().Where(b => b.TrayId == pallet.Id.ToString() && b.BoxCode == e.BoxCode).ToListAsync();
                    fd = fdList.FirstOrDefault();
                }
                else
                {
                    var fdList = await _wmsStockInfoRep.AsQueryable().Where(b => b.TrayId == pallet.Id.ToString()).ToListAsync();
                    fd = fdList.FirstOrDefault();
                }
                fd.Qty += e.PickNum.Value;
                fd.LockQuantity -= e.PickNum.Value;
                await _wmsStockInfoRep.UpdateAsync(fd);
            }
            await _wmsExportBoxInfoRep.UpdateRangeAsync(eboxs);

            order.ExportExecuteFlag = 4;
            order.IsDelete = true;
            await _wmsExportOrderRep.UpdateAsync(order);
            //
            var details = await _wmsExportNotifyDetailRep.AsQueryable().Where(d => d.ExportBillCode.ToString() == notify.ExportBillCode && d.IsDelete == false).ToListAsync();
            var dtl = details.Where(d => d.Id == order.ExportDetailId).FirstOrDefault();
            //
            dtl.ExportDetailFlag = 0;
            dtl.AllocateQuantity = 0;
            await _wmsExportNotifyDetailRep.UpdateAsync(dtl);
            //还有其它
            if (details.Where(d => d.Id != dtl.Id && d.ExportDetailFlag == 1).Count() > 0)
            {
                notify.ExportExecuteFlag = 1;
            }
            else
            {
                notify.ExportExecuteFlag = 0;
            }
            notify.UpdateUserId = userId;
            notify.UpdateTime = DateTime.Now;
            await _wmsExportNotifyRep.UpdateAsync(notify);
            //DataContext.SubmitChanges();

            await _sqlSugarClient.Ado.CommitTranAsync();
            return "ok";
        }
        catch (Exception ex)
        {
            await _sqlSugarClient.Ado.RollbackTranAsync();
            return ex.ToString();
        }
    }
    /// <summary>
    /// 撤销出库流水（变更托盘专用）
    /// </summary>
    /// <param name="orderNo"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<string> CancelPalletAllocate2(string orderNo)
    {
        try
        {
            await _sqlSugarClient.Ado.BeginTranAsync();

            var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
            //出库流水
            var orderList = await _wmsExportOrderRep.AsQueryable().Where(o => o.ExportOrderNo.ToString() == orderNo && o.ExportExecuteFlag != 4).ToListAsync();
            var order = orderList.FirstOrDefault();
            if (order.ExportExecuteFlag != 0)
            {
                return "当前出库流水单据不是等待执行！";
            }
            //库存托盘
            var palletList = await _wmsStockTrayRep.AsQueryable().Where(t => t.MaterialId == order.ExportMaterialId.ToString()
                        && t.StockSlotCode == order.ExportSlotCode && t.StockCode == order.ExportStockCode && t.LotNo == order.ExportLotNo).ToListAsync();
            var pallet = palletList.FirstOrDefault();
            //更新库存托盘
            pallet.StockQuantity += order.ExportQuantity.Value;
            pallet.LockQuantity -= order.ExportQuantity.Value;
            await _wmsStockTrayRep.UpdateAsync(pallet);
            //库存信息
            var stockList = await _wmsStockRep.AsQueryable().Where(w => w.MaterialId == order.ExportMaterialId).ToListAsync();
            var stock = stockList.FirstOrDefault();
            ///更新库存信息
            stock.StockQuantity += order.ExportQuantity.Value;
            stock.LockQuantity -= order.ExportQuantity.Value;
            await _wmsStockRep.UpdateAsync(stock);
            //出库流水明细
            var eboxs = await _wmsExportBoxInfoRep.AsQueryable().Where(b => b.ExportOrderNo == order.ExportOrderNo.ToString() && b.Qty > 0).ToListAsync();
            foreach (var e in eboxs)
            {
                //更新出库流水明细
                e.IsDelete = true;
                //更新库存箱信息
                var fdList = await _wmsStockInfoRep.AsQueryable().Where(b => b.TrayId == pallet.Id.ToString() && b.MaterialId == e.MaterialId).Where(m => m.BoxCode == e.BoxCode).ToListAsync();
                var fd = fdList.FirstOrDefault();
                fd.Qty += e.Qty.Value;
                fd.LockQuantity -= e.Qty.Value;
                await _wmsStockInfoRep.UpdateAsync(fd);
            }
            await _wmsExportBoxInfoRep.UpdateRangeAsync(eboxs);
            //更新出库流水
            order.ExportExecuteFlag = 4;
            order.IsDelete = true;
            await _wmsExportOrderRep.UpdateAsync(order);

            //DataContext.SubmitChanges();

            await _sqlSugarClient.Ado.CommitTranAsync();

            return "ok";
        }
        catch (Exception ex)
        {
            await _sqlSugarClient.Ado.RollbackTranAsync();
            return ex.ToString();
        }
    }

    #endregion


    /// <summary>
    /// 导出出库流水记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出出库流水记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsExportOrderInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsExportOrderOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "出库流水导出记录");
    }
    
    /// <summary>
    /// 下载出库流水数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载出库流水数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsExportOrderOutput>(), "出库流水导入模板");
    }
    
    private static readonly object _wmsExportOrderImportLock = new object();
    /// <summary>
    /// 导入出库流水记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入出库流水记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsExportOrderImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsExportOrderInput, WmsExportOrder>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    
                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x => {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsExportOrder>>();
                    
                    var storageable = _wmsExportOrderRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.ExportOrderNo?.Length > 32, "出库流水单据长度不能超过32个字符")
                        .SplitError(it => it.Item.ExportMaterialCode?.Length > 200, "物料编码长度不能超过200个字符")
                        .SplitError(it => it.Item.ExportMaterialName?.Length > 200, "物料名称长度不能超过200个字符")
                        .SplitError(it => it.Item.ExportMaterialStandard?.Length > 200, "物料规格长度不能超过200个字符")
                        .SplitError(it => it.Item.ExportMaterialModel?.Length > 100, "物料型号长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportMaterialType?.Length > 100, "物料类型长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportMaterialUnit?.Length > 100, "物料单位长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportSlotCode?.Length > 100, "储位编码长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportStockCode?.Length > 100, "托盘条码长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportWeight?.Length > 100, "出库重量长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportTaskNo?.Length > 100, "任务号长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportRemark?.Length > 2147483647, "备注长度不能超过2147483647个字符")
                        .SplitError(it => it.Item.ExportLotNo?.Length > 100, "批次长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportBillCode?.Length > 100, "出库单据长度不能超过100个字符")
                        .SplitError(it => it.Item.ExportListNo?.Length > 100, "出库序号长度不能超过100个字符")
                        .SplitError(it => it.Item.ScanUserNames?.Length > 100, "扫码人长度不能超过100个字符")
                        .SplitError(it => it.Item.WholeBoxNum?.Length > 100, "箱数长度不能超过100个字符")
                        .SplitError(it => it.Item.StockWholeBoxNum?.Length > 100, "托盘总箱数长度不能超过100个字符")
                        .SplitError(it => it.Item.StockQuantity?.Length > 100, "托盘总数量长度不能超过100个字符")
                        .SplitError(it => it.Item.WholeOutWare?.Length > 64, "WholeOutWare长度不能超过64个字符")
                        .SplitError(it => it.Item.ExportStockSlotCode?.Length > 32, "储位编码长度不能超过32个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.ExportOrderNo,
                        it.ExportBillType,
                        it.ExportMaterialId,
                        it.ExportMaterialCode,
                        it.ExportMaterialName,
                        it.ExportMaterialStandard,
                        it.ExportMaterialModel,
                        it.ExportMaterialType,
                        it.ExportMaterialUnit,
                        it.ExportWarehouseId,
                        it.ExportAreaId,
                        it.ExportSlotCode,
                        it.ExportStockCode,
                        it.ExportQuantity,
                        it.ExportWeight,
                        it.ExportProductionDate,
                        it.ExportLoseDate,
                        it.ExportDepartmentCode,
                        it.ExpotSupplierCode,
                        it.ExportCustomerCode,
                        it.ExportTaskNo,
                        it.ExportExecuteFlag,
                        it.ExporOrederDate,
                        it.ExportRemark,
                        it.ExportLotNo,
                        it.ExportBillCode,
                        it.ExportListNo,
                        it.ScanQuantity,
                        it.ScanUserNames,
                        it.CompleteDate,
                        it.PickedNum,
                        it.ExportDetailId,
                        it.WholeBoxNum,
                        it.OutType,
                        it.StockWholeBoxNum,
                        it.StockQuantity,
                        it.ExportType,
                        it.InspectionStatus,
                        it.WholeOutWare,
                        it.OrderByStatus,
                        it.ExportStockSlotCode,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
    #region 公共方法


    /// <summary>
    /// 获取入出库自增的单据号/流水号
    /// </summary>
    /// <param name="inOutFlag">入库单:RKS;出库单:CKS;入库流水:RK;出库流水:CK;</param>
    /// <returns></returns>
    public async Task<string> GetImExNo(string inOutFlag)
    {
        string maxNo = "";
        //获取流水号
        if (inOutFlag == EnmuValue.InOutFlag.CK.ToString() || inOutFlag == EnmuValue.InOutFlag.RK.ToString() || inOutFlag == EnmuValue.InOutFlag.TJ.ToString() || inOutFlag == EnmuValue.InOutFlag.YS.ToString() || inOutFlag == EnmuValue.InOutFlag.PK.ToString())
        {

            maxNo = await SelectOrderMaxNo(inOutFlag);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(2);
            }
        }
        else if (inOutFlag == EnmuValue.InOutFlag.RKR.ToString() || inOutFlag == EnmuValue.InOutFlag.CKR.ToString() || inOutFlag == EnmuValue.InOutFlag.YKR.ToString() || inOutFlag == EnmuValue.InOutFlag.PKR.ToString() || inOutFlag == EnmuValue.InOutFlag.IF.ToString() || inOutFlag == EnmuValue.InOutFlag.YSR.ToString() || inOutFlag == EnmuValue.InOutFlag.TJR.ToString() || inOutFlag == EnmuValue.InOutFlag.PKR.ToString())
        {
            maxNo = await SelectTaskMaxNo(inOutFlag);
            if (!string.IsNullOrEmpty(maxNo))
            {
                maxNo = maxNo.Substring(3);
            }
        }
        else
        {
            return "";
        }
        //获取数据库时间八位
        string date = GetDBTime().Trim();
        string no = "";
        if (string.IsNullOrEmpty(maxNo))
        {
            no = inOutFlag + date + "000001";
        }
        else
        {
            if (maxNo.Substring(0, 8) == date)
            {
                int lastNo = Convert.ToInt32(maxNo.Substring(8, 6)) + 1;
                no = inOutFlag + date + (lastNo.ToString().PadLeft(6, '0'));
            }
            else
            {
                no = inOutFlag + date + "000001";
            }
        }
        return no;
    }
    /// <summary>
    /// 获取最大流水号
    /// </summary>
    /// <returns></returns>
    private async Task<string> SelectOrderMaxNo(string inoutFlag)
    {
        try
        {
            string orderNo = "";
            if (inoutFlag == EnmuValue.InOutFlag.CK.ToString())
            {
                var list = await _wmsExportOrderRep.AsQueryable().Where(a => a.ExportOrderNo.ToString().StartsWith("CK")).ToListAsync();
                orderNo = list.Select(a => a.ExportOrderNo).Max().ToString();
            }
            return orderNo;

        }
        catch (Exception ex)
        {

            throw new Exception("获取最大流水号" + ex.Message);
        }
    }
    /// <summary>
    /// 获取最大单据号(入库任务、出库任务、移库任务、盘库任务)
    /// </summary>
    /// <returns></returns>
    private async Task<string> SelectTaskMaxNo(string inoutFlag)
    {
        try
        {

            string taskNo = "";
            if (inoutFlag == EnmuValue.InOutFlag.CKR.ToString())
            {
                var list = await _wmsExportTaskRep.AsQueryable().Where(a => a.ExportTaskNo.StartsWith("CKR")).ToListAsync();
                taskNo = list.Select(a => a.ExportTaskNo).Max();
            }
            return taskNo;

        }
        catch (Exception ex)
        {

            return "";
        }
    }
    /// <summary>
    /// 获取服务器时间八位yyyyMMdd
    /// </summary>
    /// <returns></returns>
    public string GetDBTime()
    {
        try
        {
            var dt = _sqlSugarClient.Ado.GetDataTable("select * from View_GetServerTime");
            //var serverTime = DataContext.View_GetServerTime.Select(a => a.ServerTime).First();

            var serverTime = dt.Rows[0][0] as String;
            return serverTime;

        }
        catch (Exception ex)
        {

            throw new Exception("获取服务器时间八位yyyyMMdd" + ex.Message);
        }
    }
    /// <summary>
    /// 添加按钮点击记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="ButtonInformation">记录信息</param>
    public async Task<string> InsertButtonRecord(string userId, string pageName, string ButtonInformation)
    {
        try
        {
            _sqlSugarClient.Ado.BeginTranAsync();
            //判断用户是否为空
            if (!string.IsNullOrWhiteSpace(userId))
            {
                //查询对应的用户
                var listList = await _sysUserRep.AsQueryable().ToListAsync();
                var list = listList.FirstOrDefault(a => a.Id.ToString() == userId);
                if (list != null && list.Id != null && list.Id > 0)
                {
                    WmsButtonRecord wmsButton = new WmsButtonRecord();
                    wmsButton.Id = SnowFlakeSingle.Instance.NextId();
                    wmsButton.UserId = list.Id;
                    wmsButton.ClickButtonDate = DateTime.Now;
                    wmsButton.PageName = pageName;

                    //用户名+按钮点击时间+记录按钮信息
                    wmsButton.ButtonInformation = list.RealName + " 在 " + DateTime.Now + " " + ButtonInformation;
                    //DataContext.WmsButtonRecord.InsertOnSubmit(wmsButton);
                    //DataContext.SubmitChanges();

                    await _wmsButtonRecordRep.InsertAsync(wmsButton);

                }
            }
            _sqlSugarClient.Ado.CommitTranAsync();
            return "成功";
        }
        catch (Exception ex)
        {
            _sqlSugarClient.Ado.RollbackTranAsync();
            throw new Exception("添加按钮点击记录功能失败：" + ex.Message);
        }
    }

    #region 批量生成空托入库流水
    //public string GetOrderNo(string inOutFlag)
    //{
    //    string maxNo = "";
    //    //获取流水号
    //    if (inOutFlag == InOutFlag.CK.ToString() || inOutFlag == InOutFlag.RK.ToString() || inOutFlag == InOutFlag.TJ.ToString() || inOutFlag == InOutFlag.YS.ToString() || inOutFlag == InOutFlag.PK.ToString())
    //    {
    //        maxNo = GetOrderMaxNo(inOutFlag);
    //        if (!string.IsNullOrEmpty(maxNo))
    //        {
    //            maxNo = maxNo.Substring(2);
    //        }
    //    }
    //    else
    //    {
    //        return "";
    //    }
    //    //获取数据库时间八位
    //    string date = GetDBTime().Trim();
    //    string no = "";
    //    if (string.IsNullOrEmpty(maxNo))
    //    {
    //        no = inOutFlag + date + "000001";
    //    }
    //    else
    //    {
    //        if (maxNo.Substring(0, 8) == date)
    //        {
    //            int lastNo = Convert.ToInt32(maxNo.Substring(8, 6)) + 1;
    //            no = inOutFlag + date + (lastNo.ToString().PadLeft(6, '0'));
    //        }
    //        else
    //        {
    //            no = inOutFlag + date + "000001";
    //        }
    //    }
    //    return no;
    //}
    //private static string GetOrderMaxNo(string inoutFlag)
    //{
    //    try
    //    {
    //        using (var conn = new SqlConnection(DataConnection.GetDataConnection))
    //        {
    //            conn.Open();

    //            string sql = "";
    //            if (inoutFlag == InOutFlag.RK.ToString())
    //            {
    //                sql = @"SELECT MAX(ImportOrderNo) 
    //                FROM WmsImportOrder WITH (NOLOCK) 
    //                WHERE ImportOrderNo LIKE @Prefix + '%'";
    //            }

    //            using (var cmd = new SqlCommand(sql, conn))
    //            {
    //                cmd.Parameters.AddWithValue("@Prefix", inoutFlag);
    //                cmd.CommandTimeout = 30; // 可调大

    //                var result = cmd.ExecuteScalar();
    //                return result == DBNull.Value ? "" : (string)result;
    //            }
    //        }
    //    }
    //    catch (Exception ex)
    //    {

    //        throw new Exception("获取最大流水号" + ex.Message);
    //    }
    //}

    #endregion

    #endregion

}
