// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Admin.NET.Application.Entity;
using Admin.NET.Core.Service;

using Furion.DatabaseAccessor;
using Furion.FriendlyException;

using Mapster;

using Microsoft.AspNetCore.Http;

using NewLife;

using SqlSugar;
namespace Admin.NET.Application;

/// <summary>
/// 入库标签打印服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsImportLabelPrintService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsImportLabelPrint> _wmsImportLabelPrintRep;
    private readonly SqlSugarRepository<WmsImportNotifyDetail> _wmsImportNotifyDetailRep;
    private readonly SqlSugarRepository<WmsBaseMaterial> _wmsBaseMaterialRep;
    private readonly WmsImportNotifyDetailService _wmsImportNotifyDetailService;
    private readonly ISqlSugarClient _sqlSugarClient;

    public WmsImportLabelPrintService(SqlSugarRepository<WmsImportLabelPrint> wmsImportLabelPrintRep, ISqlSugarClient sqlSugarClient, SqlSugarRepository<WmsImportNotifyDetail> wmsImportNotifyDetailRep, SqlSugarRepository<WmsBaseMaterial> wmsBaseMaterialRep, WmsImportNotifyDetailService WmsImportNotifyDetailService)
    {
        _wmsImportLabelPrintRep = wmsImportLabelPrintRep;
        _sqlSugarClient = sqlSugarClient;
        _wmsImportNotifyDetailRep = wmsImportNotifyDetailRep;
        _wmsBaseMaterialRep = wmsBaseMaterialRep;
        _wmsImportNotifyDetailService = WmsImportNotifyDetailService;
    }

    /// <summary>
    /// 分页查询入库标签打印 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询入库标签打印")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsImportLabelPrintOutput>> Page(PageWmsImportLabelPrintInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsImportLabelPrintRep.AsQueryable().Where(u=>u.ImportDetailId==input.ImportDetailId)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), u => u.LabelStream.Contains(input.Keyword) || u.MaterialCode.Contains(input.Keyword) || u.MaterialName.Contains(input.Keyword) || u.MaterialStandard.Contains(input.Keyword) || u.LotNo.Contains(input.Keyword) || u.BoxCode.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LabelStream), u => u.LabelStream.Contains(input.LabelStream.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialCode), u => u.MaterialCode.Contains(input.MaterialCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialName), u => u.MaterialName.Contains(input.MaterialName.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.MaterialStandard), u => u.MaterialStandard.Contains(input.MaterialStandard.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.LotNo), u => u.LotNo.Contains(input.LotNo.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.BoxCode), u => u.BoxCode.Contains(input.BoxCode.Trim()))
            .WhereIF(input.ImportId != null, u => u.ImportId == input.ImportId)
            .WhereIF(input.IsUse != null, u => u.IsUse == input.IsUse)
            .WhereIF(input.MxFlag != null, u => u.MxFlag == input.MxFlag)
            .Select<WmsImportLabelPrintOutput>();
        return await query.OrderBuilder(input).ToPagedListAsync(input.Page, input.PageSize);
    }
    /// <summary>
    /// 入库标签打印 🖨️
    /// 重构自《ImportController》《GetImportId》接口
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("入库标签打印")]
    [ApiDescriptionSettings(Name = "Print"), HttpPost]
    public async Task<List<WmsImportLabelPrintOutput>> Print(WmsImportLabelPrintInput input)
    {
        try
        {
            // 打印前操作
            await PrintByBefore(input);
            if (!string.IsNullOrEmpty(input.LabelID))
            {
                // 根据标签ID打印 
                return await PrintByLabelId(input);
            }
            else if (input.ImportDetailId.HasValue)
            {
                // 根据入库明细ID打印
                return await PrintByImportDetailId(input);
            }
            else
            {
                throw new Exception("参数错误：必须提供标签ID或入库明细ID");
            }
        }
        catch (Exception ex)
        {
            // 记录日志
            Console.WriteLine($"标签打印错误: {ex.Message}");
            throw;
        }
    }
    /// <summary>
    /// 打印前操作
    /// </summary>
    /// <param name="input"></param>
    private async Task PrintByBefore(WmsImportLabelPrintInput input)
    {
        var detail = await _wmsImportNotifyDetailService.DetailJoinMaterial(new QueryByIdWmsImportNotifyDetailInput { Id = input.ImportDetailId });
        if (detail != null)
        {
            WmsImportLabelPrint boxstream = null;
            try
            {
                boxstream = await _wmsImportLabelPrintRep.AsQueryable().OrderByDescending(a => a.CreateTime).FirstAsync(a => a.ImportDetailId == detail.Id);
            }
            catch
            {
                boxstream = null;
            }
            string Total = detail.ImportQuantity.Value.ToString();
            decimal index = Math.Round((decimal)detail.ImportQuantity, 2);
            string index1 = Convert.ToString(index);
            int item;
            if (!int.TryParse(index1, out item))
            {
                Total = (Math.Ceiling(Convert.ToDouble(Total))).ToString();
            }
            if (boxstream != null)
            {
                // 验证BoxCode格式是否正确
                var boxCodeParts = boxstream.BoxCode.Split('/');
                if (boxCodeParts.Length >= 2 && boxCodeParts[1] != Total)
                {
                    var LabelPrint = await _wmsImportLabelPrintRep.GetFirstAsync(a => a.ImportDetailId == input.ImportDetailId);
                    await _wmsImportLabelPrintRep.DeleteAsync(LabelPrint);
                }
            }
        }

    }
    /// <summary>
    /// 根据标签ID打印
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<List<WmsImportLabelPrintOutput>> PrintByLabelId(WmsImportLabelPrintInput input)
    {
        var label = await _wmsImportLabelPrintRep.GetFirstAsync(x => x.LabelID == input.LabelID.ToString());
        if (label == null)
            throw new Exception("标签不存在");

        var detail = await _wmsImportNotifyDetailService.DetailJoinMaterial(new QueryByIdWmsImportNotifyDetailInput { Id = label.ImportDetailId });
        if (detail == null)
            throw new Exception("入库明细不存在");

        var printModels = new List<WmsImportLabelPrintOutput>();

        string boxCode = "";
        string total = "";
        int count = 0;
        bool isFullBox = true;
        decimal boxQuantity = 0;

        // 获取箱数量
        if (string.IsNullOrEmpty(detail.BoxQuantity?.ToString()))
        {
            var material = await _wmsBaseMaterialRep.GetFirstAsync(a => a.Id == detail.MaterialId);
            boxQuantity = Convert.ToDecimal(material?.BoxQuantity);
        }
        else
        {
            boxQuantity = Convert.ToDecimal(detail.BoxQuantity);
        }

        // 处理箱号
        if (!string.IsNullOrEmpty(input.BoxCode))
        {
            boxCode = input.BoxCode;
        }
        else
        {
            WmsImportLabelPrint lastBox = null;
            try
            {
                lastBox = await _wmsImportLabelPrintRep.AsQueryable()
                    .Where(a => a.ImportDetailId == detail.Id)
                    .OrderBy(a => a.CreateTime, OrderByType.Desc)
                    .FirstAsync();
            }
            catch
            {
                lastBox = null;
            }

            total = Math.Ceiling(detail.ImportQuantity.Value / boxQuantity).ToString();
            decimal index = detail.ImportQuantity.Value / boxQuantity;
            string indexStr = index.ToString();

            if (!int.TryParse(indexStr, out int item))
            {
                total = Math.Ceiling(Convert.ToDouble(total)).ToString();
                isFullBox = false;
            }

            if (lastBox != null)
            {
                boxCode = lastBox.BoxCode;
            }
            else
            {
                boxCode = Convert.ToString(0).PadLeft(total.Length, '0') + "/" + total;
            }
        }

        int boxAdd = 0;
        for (int i = 1; i <= input.Number; i++)
        {
            if (string.IsNullOrEmpty(input.BoxCode))
            {
                int index = boxCode.IndexOf("/");
                if (index > 0)
                {
                    int box = Convert.ToInt16(boxCode.Substring(0, index)) + 1;
                    if (box > int.Parse(total))
                    {
                        boxAdd = box;
                        box = int.Parse(total);
                    }
                    boxCode = Convert.ToString(box).PadLeft(total.Length, '0') + "/" + total;
                    count = int.Parse(total) - box + 1;
                }
                else
                {
                    // BoxCode格式不正确，使用默认值
                    boxCode = Convert.ToString(1).PadLeft(total.Length, '0') + "/" + total;
                    count = int.Parse(total);
                }
            }

            var model = new WmsImportLabelPrintOutput()
            {
                ImportBillCode = detail.ImportBillCode,
                ImportId = detail.ImportId,
                ImportDetailId = detail.Id,
                LotNo = detail.LotNo,
                MaterialCode = detail.MaterialCode,
                MaterialName = detail.MaterialName,
                MaterialStandard = detail.MaterialStandard,
                LabelStream = label.LabelStream,
                LabelID = label.LabelID,
                BoxCode = boxCode,
                Number = count,
                ManufacturerName = detail.ManufacturerName,
                ImportProductionDate = detail.ImportProductionDate,
                ImportLostDate = detail.ImportLostDate

            };
            printModels.Add(model);

            // 保存打印记录
            if (input.Isprint == "1" && string.IsNullOrEmpty(input.BoxCode))
            {
                if (boxAdd <= int.Parse(total))
                {
                    var printRecord = new WmsImportLabelPrint
                    {
                        LabelID = label.LabelID,
                        ImportId = detail.ImportId,
                        ImportDetailId = label.ImportDetailId,
                        MaterialCode = detail.MaterialCode,
                        MaterialName = detail.MaterialName,
                        MaterialStandard = detail.MaterialStandard,
                        LotNo = detail.LotNo,
                        CreateTime = DateTime.Now,
                        IsUse = 0,
                        LabelStream = label.LabelStream,
                        ImportBillCode = detail.ImportBillCode,
                        BoxCode = boxCode
                    };

                    var boxCodeParts = boxCode.Split('/');
                    if (boxCodeParts.Length >= 2 && boxCodeParts[0] == boxCodeParts[1] && !isFullBox)
                    {
                        printRecord.Quantity = detail.ImportQuantity - boxQuantity * (int.Parse(boxCodeParts[1]) - 1);
                        printRecord.MxFlag = 1;
                    }
                    else
                    {
                        printRecord.Quantity = boxQuantity;
                        printRecord.MxFlag = 0;
                    }

                    await _wmsImportLabelPrintRep.InsertAsync(printRecord);


                }
            }
        }

        return printModels;
    }

    /// <summary>
    /// 根据入库明细ID打印
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task<List<WmsImportLabelPrintOutput>> PrintByImportDetailId(WmsImportLabelPrintInput input)
    {
        var detail = await _wmsImportNotifyDetailService.DetailJoinMaterial(new QueryByIdWmsImportNotifyDetailInput { Id = input.ImportDetailId });
        if (detail == null)
            throw new Exception("入库明细不存在");

        var printModels = new List<WmsImportLabelPrintOutput>();

        string labelStream = "";
        string labelId = "";
        string time = DateTime.Now.ToString("yyMMdd");
        string boxCode = "";
        string total = "";
        int count = 0;
        bool isFullBox = true;
        decimal boxQuantity = 0;

        // 获取箱数量
        if (string.IsNullOrEmpty(detail.BoxQuantity?.ToString()))
        {
            var material = await _wmsBaseMaterialRep.GetFirstAsync(a => a.Id == detail.MaterialId);
            boxQuantity = Convert.ToDecimal(material?.BoxQuantity);
        }
        else
        {
            boxQuantity = Convert.ToDecimal(detail.BoxQuantity);
        }

        // 生成标签ID
        WmsImportLabelPrint lastCode = null;
        try
        {
            lastCode = await _wmsImportLabelPrintRep.AsQueryable()
                .Where(a => a.MaterialCode == detail.MaterialCode)
                .OrderBy(a => a.CreateTime, OrderByType.Desc)
                .FirstAsync();
        }
        catch
        {
            lastCode = null;
        }

        if (lastCode == null)
        {
            labelId = "000";
        }
        else
        {
            // 验证LabelID长度是否足够
            if (lastCode.LabelID.Length >= detail.MaterialCode.Length + 9)
            {
                string riqi = lastCode.LabelID.Substring(detail.MaterialCode.Length, 6);
                if (riqi == time)
                {
                    labelId = lastCode.LabelID.Substring(detail.MaterialCode.Length + 6, 3);
                }
                else
                {
                    labelId = "000";
                }
            }
            else
            {
                labelId = "000";
            }
        }

        // 处理箱号
        if (!string.IsNullOrEmpty(input.BoxCode))
        {
            boxCode = input.BoxCode;
        }
        else
        {
            WmsImportLabelPrint lastBox = null;
            try
            {
                lastBox = await _wmsImportLabelPrintRep.AsQueryable()
                    .Where(a => a.ImportDetailId == detail.Id)
                    .OrderBy(a => a.CreateTime, OrderByType.Desc)
                    .FirstAsync();
            }
            catch
            {
                lastBox = null;
            }

            total = Math.Ceiling(detail.ImportQuantity.Value / boxQuantity).ToString();
            decimal index = detail.ImportQuantity.Value / boxQuantity;
            string indexStr = index.ToString();

            if (!int.TryParse(indexStr, out int item))
            {
                total = Math.Ceiling(Convert.ToDouble(total)).ToString();
                isFullBox = false;
            }

            if (lastBox != null)
            {
                boxCode = lastBox.BoxCode;
            }
            else
            {
                boxCode = Convert.ToString(0).PadLeft(total.Length, '0') + "/" + total;
            }
        }

        // 生成流水号
        string newTime = DateTime.Now.ToString("MMyyyy");
        WmsImportLabelPrint lastStream = null;
        try
        {
            lastStream = await _wmsImportLabelPrintRep.AsQueryable()
                .Where(a => a.ImportDetailId == detail.Id)
                .OrderBy(a => a.CreateTime, OrderByType.Desc)
                .FirstAsync();
        }
        catch
        {
            lastStream = null;
        }

        if (lastStream == null)
        {
            var lotCount = await _wmsImportLabelPrintRep.AsQueryable()
                .Where(a => a.ImportDetailId == detail.Id)
                .GroupBy(a => new { a.ImportId })
                .CountAsync();

            if (lotCount > 0)
            {
                int stream = lotCount + 1;
                labelStream = newTime + Convert.ToString(stream).PadLeft(4, '0');
            }
            else
            {
                labelStream = newTime + "0001";
            }
        }
        else
        {
            // 验证LabelStream长度是否足够
            if (lastStream.LabelStream.Length >= 10)
            {
                string riqi = lastStream.LabelStream.Substring(0, 6);
                string streamNumber = lastStream.LabelStream.Substring(6, 4);

                if (newTime.Equals(riqi))
                {
                    labelStream = lastStream.LabelStream;
                }
                else
                {
                    string newStream = newTime + streamNumber;
                    WmsImportLabelPrint lotNo = null;
                    try
                    {
                        lotNo = await _wmsImportLabelPrintRep.AsQueryable()
                            .Where(a => a.ImportDetailId == detail.Id && a.LabelStream == newStream)
                            .OrderBy(a => a.CreateTime, OrderByType.Desc)
                            .FirstAsync();
                    }
                    catch
                    {
                        lotNo = null;
                    }

                    if (lotNo != null)
                    {
                        labelStream = lotNo.LabelStream;
                    }
                    else
                    {
                        WmsImportLabelPrint lotNo1 = null;
                        try
                        {
                            lotNo1 = await _wmsImportLabelPrintRep.AsQueryable()
                                .Where(a => a.ImportDetailId == detail.Id && a.LabelStream.Contains(newTime))
                                .OrderBy(a => a.CreateTime, OrderByType.Desc)
                                .FirstAsync();
                        }
                        catch
                        {
                            lotNo1 = null;
                        }

                        if (lotNo1 != null)
                        {
                            labelStream = lotNo1.LabelStream;
                        }
                        else
                        {
                            labelStream = newTime + "0001";
                        }
                    }
                }
            }
            else
            {
                // LabelStream格式不正确，使用默认值
                labelStream = newTime + "0001";
            }
        }

        int boxAdd = 0;
        for (int i = 1; i <= input.Number; i++)
        {
            int liushuiId = Convert.ToInt16(labelId) + 1;
            labelId = Convert.ToString(liushuiId).PadLeft(3, '0');
            var codeId = detail.MaterialCode + time + Convert.ToString(liushuiId).PadLeft(3, '0');

            if (string.IsNullOrEmpty(input.BoxCode))
            {
                int index = boxCode.IndexOf("/");
                if (index > 0)
                {
                    int box = Convert.ToInt16(boxCode.Substring(0, index)) + 1;
                    if (box > int.Parse(total))
                    {
                        boxAdd = box;
                        box = int.Parse(total);
                    }
                    boxCode = Convert.ToString(box).PadLeft(total.Length, '0') + "/" + total;
                    count = int.Parse(total) - box + 1;
                }
                else
                {
                    // BoxCode格式不正确，使用默认值
                    boxCode = Convert.ToString(1).PadLeft(total.Length, '0') + "/" + total;
                    count = int.Parse(total);
                }
            }

            var model = new WmsImportLabelPrintOutput()
            {
                ImportBillCode = detail.ImportBillCode,
                ImportId = detail.ImportId,
                ImportDetailId = detail.Id,
                LotNo = detail.LotNo,
                MaterialCode = detail.MaterialCode,
                MaterialStandard = detail.MaterialStandard,
                MaterialName = detail.MaterialName,
                LabelStream = labelStream,
                LabelID = codeId,
                BoxCode = boxCode,
                Number = count,
                ManufacturerName = detail.ManufacturerName,
                ImportProductionDate = detail.ImportProductionDate,
                ImportLostDate = detail.ImportLostDate
            };
            printModels.Add(model);

            // 保存打印记录
            if (input.Isprint == "1")
            {
                var printRecord = new WmsImportLabelPrint
                {
                    LabelID = codeId,
                    ImportId = detail.ImportId,
                    ImportDetailId = detail.Id,
                    MaterialCode = detail.MaterialCode,
                    MaterialName = detail.MaterialName,
                    MaterialStandard = detail.MaterialStandard,
                    LotNo = detail.LotNo,
                    CreateTime = DateTime.Now,
                    IsUse = 0,
                    LabelStream = labelStream,
                    ImportBillCode = detail.ImportBillCode,
                    BoxCode = boxCode
                };

                var boxCodeParts = boxCode.Split('/');
                if (boxCodeParts.Length >= 2 && boxCodeParts[0] == boxCodeParts[1] && !isFullBox)
                {
                    printRecord.Quantity = detail.ImportQuantity - boxQuantity * (int.Parse(boxCodeParts[1]) - 1);
                    printRecord.MxFlag = 1;
                }
                else
                {
                    printRecord.Quantity = boxQuantity;
                    printRecord.MxFlag = 0;
                }
                // 更新入库明细标签状态
                var importDetail = await _wmsImportNotifyDetailRep.GetFirstAsync(a => a.Id == detail.Id);
                if (importDetail != null)
                {
                    importDetail.LabelJudgment = 1;
                    await _wmsImportNotifyDetailRep.UpdateAsync(importDetail);
                }
                if (string.IsNullOrEmpty(input.BoxCode))
                {
                    if (boxAdd <= int.Parse(total))
                    {
                        await _wmsImportLabelPrintRep.InsertAsync(printRecord);
                    }
                }
                else
                {
                    await _wmsImportLabelPrintRep.InsertAsync(printRecord);
                }
            }
        }

        return printModels;
    }
    /// <summary>
    /// 获取入库标签打印详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取入库标签打印详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsImportLabelPrint> Detail([FromQuery] QueryByIdWmsImportLabelPrintInput input)
    {
        return await _wmsImportLabelPrintRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 增加入库标签打印 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加入库标签打印")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsImportLabelPrintInput input)
    {
        var entity = input.Adapt<WmsImportLabelPrint>();
        return await _wmsImportLabelPrintRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新入库标签打印 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新入库标签打印")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsImportLabelPrintInput input)
    {
        var entity = input.Adapt<WmsImportLabelPrint>();
        await _wmsImportLabelPrintRep.AsUpdateable(entity)
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 重置打印 ❌
    /// 重构自《ImportController》《DelLabelPrint》接口
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除入库标签打印")]
    [ApiDescriptionSettings(Name = "Reset"), HttpPost]
    public async Task<bool> Reset(BathDeleteWmsImportLabelPrintInput input)
    {
        var list = await _wmsImportLabelPrintRep.AsQueryable().Where(u => u.ImportDetailId == input.ImportDetailId).ToListAsync();
        //更新入库明细打印状态
        var detail = await _wmsImportNotifyDetailRep.GetFirstAsync(u => u.Id == input.ImportDetailId);
        detail.LabelJudgment = 2;
        await _wmsImportNotifyDetailRep.AsUpdateable(detail).ExecuteCommandAsync();
        return await _wmsImportLabelPrintRep.DeleteAsync(list);   
    }

    /// <summary>
    /// 批量删除入库标签打印 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除入库标签打印")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<int> BatchDelete([Required(ErrorMessage = "主键列表不能为空")] List<DeleteWmsImportLabelPrintInput> input)
    {
        var exp = Expressionable.Create<WmsImportLabelPrint>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsImportLabelPrintRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

        return await _wmsImportLabelPrintRep.FakeDeleteAsync(list);   //假删除
        //return await _wmsImportLabelPrintRep.DeleteAsync(list);   //真删除
    }

    /// <summary>
    /// 导出入库标签打印记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出入库标签打印记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsImportLabelPrintInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsImportLabelPrintOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        return ExcelHelper.ExportTemplate(list, "入库标签打印导出记录");
    }

    /// <summary>
    /// 下载入库标签打印数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载入库标签打印数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsImportLabelPrintOutput>(), "入库标签打印导入模板");
    }

    private static readonly object _wmsImportLabelPrintImportLock = new object();
    /// <summary>
    /// 导入入库标签打印记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入入库标签打印记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsImportLabelPrintImportLock)
        {
            var stream = ExcelHelper.ImportData<ImportWmsImportLabelPrintInput, WmsImportLabelPrint>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {

                    // 校验并过滤必填基本类型为null的字段
                    var rows = pageItems.Where(x =>
                    {
                        if (!string.IsNullOrWhiteSpace(x.Error)) return false;
                        return true;
                    }).Adapt<List<WmsImportLabelPrint>>();

                    var storageable = _wmsImportLabelPrintRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.LabelStream?.Length > 32, "标签序号长度不能超过32个字符")
                        .SplitError(it => it.Item.MaterialCode?.Length > 32, "物料编号长度不能超过32个字符")
                        .SplitError(it => it.Item.MaterialName?.Length > 32, "物料名称长度不能超过32个字符")
                        .SplitError(it => it.Item.MaterialStandard?.Length > 32, "物料规格长度不能超过32个字符")
                        .SplitError(it => it.Item.LotNo?.Length > 32, "批次长度不能超过32个字符")
                        .SplitError(it => it.Item.BoxCode?.Length > 32, "箱编码长度不能超过32个字符")
                        .SplitInsert(_ => true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();

                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.ImportId,
                        it.LabelStream,
                        it.MaterialCode,
                        it.MaterialName,
                        it.MaterialStandard,
                        it.LotNo,
                        it.IsUse,
                        it.BoxCode,
                        it.Quantity,
                        it.MxFlag,
                    }).ExecuteCommand();// 存在更新

                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });

            return stream;
        }
    }
}
