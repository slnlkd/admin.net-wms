// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Admin.NET.Application.Entity;
using Admin.NET.Core;
using Admin.NET.Core.Service;
using Furion;
using Magicodes.ExporterAndImporter.Core;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using SqlSugar;

namespace Admin.NET.Application;

/// <summary>
/// 入库单导入导出助手类
/// </summary>
public class WmsImportNotifyHelper : ITransient
{
    #region 常量定义

    // Excel列配置
    private const int MAIN_TABLE_START_COL = 1;
    private const int MAIN_TABLE_END_COL = 8;
    private const int SEPARATOR_COL = 9;
    private const int DETAIL_TABLE_START_COL = 10;
    private const int DETAIL_TABLE_END_COL = 17;
    private const int HEADER_ROW = 1;
    private const int DATA_START_ROW = 2;

    // 默认值常量
    private const string DEFAULT_SOURCE = "WMS";
    private const string DEFAULT_EXECUTING_FLAG = "01";
    private const string DEFAULT_SAMPLE_IDENTIFY_CODE = "A001";
    private const string DEFAULT_SAMPLE_BILL_CODE = "EXT001";
    private const string DEFAULT_SAMPLE_REMARK = "示例备注";
    private const string DEFAULT_MATERIAL_CODE_FALLBACK = "01010001";
    private const string DEFAULT_LOT_NO = "1";
    private const int DEFAULT_QUANTITY = 100;
    private const int EXPIRY_DAYS = 180;

    // 字典编码
    private const string DICT_CODE_MATERIAL_STATUS = "MaterialStatus";

    // 日期格式
    private const string DATE_FORMAT = "yyyy-mm-dd";

    // 批量插入大小
    private const int BATCH_INSERT_SIZE = 2048;

    // 验证规则
    private const int MAX_LENGTH_OUTER_DETAIL_ID = 32;
    private const int MAX_LENGTH_LIST_NO = 32;
    private const int MAX_LENGTH_LOT_NO = 32;
    private const int MAX_LENGTH_MATERIAL_STATUS = 32;
    private const int MAX_LENGTH_EXECUTE_FLAG = 2;
    private const int MAX_LENGTH_REMARK = 50;
    private const int MAX_LENGTH_LANEWAY_CODE = 32;
    private const int MAX_LENGTH_XJ_HOUSE_CODE = 50;

    #endregion

    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly ImportNotifyRepos _repos;
    private readonly ILogger<WmsImportNotifyHelper> _logger;
    private readonly SysDictTypeService _dictTypeService;

    public WmsImportNotifyHelper(
        ISqlSugarClient sqlSugarClient,
        ImportNotifyRepos repos,
        ILogger<WmsImportNotifyHelper> logger,
        SysDictTypeService dictTypeService)
    {
        _sqlSugarClient = sqlSugarClient;
        _repos = repos;
        _logger = logger;
        _dictTypeService = dictTypeService;
    }

    /// <summary>
    /// 写入表头并返回列映射（基于ExporterHeaderAttribute）
    /// </summary>
    public Dictionary<string, int> WriteHeadersByExporterAttribute(ExcelWorksheet sheet, List<PropertyInfo> props, int startCol, double columnWidth)
    {
        var headerColMap = new Dictionary<string, int>();
        int col = startCol;
        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<ExporterHeaderAttribute>();
            if (attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.DisplayName))
            {
                sheet.Cells[1, col].Value = attr.DisplayName;
                headerColMap[attr.DisplayName] = col;
                sheet.Column(col).Width = attr.Width > 0 ? attr.Width : columnWidth;
                col++;
            }
        }
        return headerColMap;
    }

    /// <summary>
    /// 写入主表数据行（基于导出DTO）
    /// </summary>
    public void WriteMainTableExportRowCustom(ExcelWorksheet sheet, int row,
        WmsImportNotifyOutput mainData, Dictionary<string, int> headerColMap, string identifyCode)
    {
        var mainProps = typeof(ExportWmsImportNotifyDto).GetProperties()
            .Where(p => {
                var attr = p.GetCustomAttribute<ExporterHeaderAttribute>();
                return attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.DisplayName);
            })
            .ToList();

        foreach (var prop in mainProps)
        {
            var attr = prop.GetCustomAttribute<ExporterHeaderAttribute>();
            if (attr == null || string.IsNullOrWhiteSpace(attr.DisplayName)) continue;

            if (!headerColMap.TryGetValue(attr.DisplayName, out int colIndex)) continue;

            object value = attr.DisplayName switch
            {
                "入库单号" => identifyCode,
                "所属仓库" => mainData.WarehouseFkDisplayName,
                "单据类型" => mainData.BillTypeDisplayName,
                "供货单位" => mainData.SupplierName,
                "制造商单位" => mainData.ManufacturerName,
                "来源" => mainData.Source,
                "外部单号" => mainData.OuterBillCode,
                "备注" => mainData.Remark,
                _ => null
            };

            if (value != null)
                sheet.Cells[row, colIndex].Value = value;
        }
    }

    /// <summary>
    /// 写入明细表数据行（基于导出DTO）
    /// </summary>
    public void WriteDetailTableExportRowCustom(ExcelWorksheet sheet, int row,
        dynamic detailData, Dictionary<string, int> headerColMap,
        string identifyCode, Dictionary<string, string> materialStatusDict, Dictionary<string, string> executeFlagDict = null)
    {
        var detailProps = typeof(ExportWmsImportNotifyDetailDto).GetProperties()
            .Where(p => {
                var attr = p.GetCustomAttribute<ExporterHeaderAttribute>();
                return attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.DisplayName);
            })
            .ToList();

        foreach (var prop in detailProps)
        {
            var attr = prop.GetCustomAttribute<ExporterHeaderAttribute>();
            if (attr == null || string.IsNullOrWhiteSpace(attr.DisplayName)) continue;

            if (!headerColMap.TryGetValue(attr.DisplayName, out int colIndex)) continue;

            object value = attr.DisplayName switch
            {
                "入库单号" => identifyCode,
                "物料编码" => detailData.MaterialCode,
                "物料名称" => detailData.MaterialName,
                "物料规格" => detailData.MaterialStandard,
                "批次" => detailData.LotNo,
                "状态" => GetMaterialStatusLabel(detailData.MaterialStatus, materialStatusDict),
                "计量单位" => detailData.UnitName,
                "计划数量" => detailData.ImportQuantity,
                "生产日期" => detailData.ImportProductionDate != null ? ((DateTime)detailData.ImportProductionDate).ToString("yyyy-MM-dd") : null,
                "有效期至" => detailData.ImportLostDate != null ? ((DateTime)detailData.ImportLostDate).ToString("yyyy-MM-dd") : null,
                "完成数量" => detailData.ImportCompleteQuantity,
                "已组盘数量" => detailData.ImportFactQuantity,
                "执行状态" => GetMaterialStatusLabel(detailData.ImportExecuteFlag, executeFlagDict ?? new Dictionary<string, string>()),
                "箱数量" => detailData.BoxQuantity,
                "贴标" => detailData.Labeling == "1" ? "是" : (detailData.Labeling == "0" ? "否" : detailData.Labeling),
                "物料备注" => detailData.Remark,
                _ => null
            };

            if (value != null)
                sheet.Cells[row, colIndex].Value = value;
        }
    }

    /// <summary>
    /// 写入流水主表数据行（基于导出DTO）
    /// </summary>
    public void WriteMainTableExportRowForOrder(ExcelWorksheet sheet, int row,
        dynamic mainData, Dictionary<string, int> headerColMap, Dictionary<string, string> executeFlagDict = null)
    {
        var mainProps = typeof(ExportWmsImportOrderMainDto).GetProperties()
            .Where(p => {
                var attr = p.GetCustomAttribute<ExporterHeaderAttribute>();
                return attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.DisplayName);
            })
            .ToList();

        foreach (var prop in mainProps)
        {
            var attr = prop.GetCustomAttribute<ExporterHeaderAttribute>();
            if (attr == null || string.IsNullOrWhiteSpace(attr.DisplayName)) continue;

            if (!headerColMap.TryGetValue(attr.DisplayName, out int colIndex)) continue;

            object value = attr.DisplayName switch
            {
                "入库流水号" => mainData.ImportOrderNo,
                "载具条码" => mainData.StockCode,
                "储位地址" => mainData.ImportSlotCode,
                "执行状态" => GetMaterialStatusLabel(mainData.ImportExecuteFlag, executeFlagDict ?? new Dictionary<string, string>()),
                "完成时间" => mainData.CompleteDate,
                "创建人" => mainData.CreateUserName,
                "创建时间" => mainData.CreateTime,
                _ => null
            };

            if (value != null)
                sheet.Cells[row, colIndex].Value = value;
        }
    }

    /// <summary>
    /// 写入流水明细表数据行（基于导出DTO）
    /// </summary>
    public void WriteDetailTableExportRowForOrder(ExcelWorksheet sheet, int row,
        dynamic detailData, Dictionary<string, int> headerColMap,
        Dictionary<string, string> materialStatusDict, Dictionary<string, string> executeFlagDict = null, Dictionary<string, string> bulkTankDict = null)
    {
        var detailProps = typeof(ExportWmsImportOrderSubDto).GetProperties()
            .Where(p => {
                var attr = p.GetCustomAttribute<ExporterHeaderAttribute>();
                return attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.DisplayName);
            })
            .ToList();

        foreach (var prop in detailProps)
        {
            var attr = prop.GetCustomAttribute<ExporterHeaderAttribute>();
            if (attr == null || string.IsNullOrWhiteSpace(attr.DisplayName)) continue;

            if (!headerColMap.TryGetValue(attr.DisplayName, out int colIndex)) continue;

            object value = attr.DisplayName switch
            {
                "入库流水号" => detailData.ImportOrderNo,
                "箱码编号" => detailData.BoxCode,
                "物料编码" => detailData.MaterialCode,
                "物料名称" => detailData.MaterialName,
                "物料规格" => detailData.MaterialStandard,
                "批次" => detailData.LotNo,
                "状态" => GetMaterialStatusLabel(detailData.Status, materialStatusDict),
                "实际数量" => detailData.Qty,
                "零箱标识" => GetMaterialStatusLabel(detailData.BulkTank, bulkTankDict ?? new Dictionary<string, string>()),
                "生产日期" => detailData.ProductionDate != null ? ((DateTime)detailData.ProductionDate).ToString("yyyy-MM-dd") : null,
                "有效期至" => detailData.ValidateDay != null ? ((DateTime)detailData.ValidateDay).ToString("yyyy-MM-dd") : null,
                _ => null
            };

            if (value != null)
                sheet.Cells[row, colIndex].Value = value;
        }
    }

    #region DownloadTemplate 模板下载相关方法

    /// <summary>
    /// 写入表头并返回列映射
    /// </summary>
    /// <param name="sheet">Excel工作表对象</param>
    /// <param name="props">属性列表，用于提取ImporterHeaderAttribute</param>
    /// <param name="startCol">起始列索引（从1开始）</param>
    /// <param name="columnWidth">列宽度（字符数）</param>
    /// <returns>表头名称到列索引的映射字典</returns>
    /// <remarks>
    /// 注意事项：
    /// - 只处理带有ImporterHeaderAttribute且Name不为空的属性
    /// - 列索引从startCol开始递增
    /// - 表头固定写入第1行
    /// </remarks>
    public Dictionary<string, int> WriteHeaders(ExcelWorksheet sheet, List<PropertyInfo> props, int startCol, double columnWidth)
    {
        var headerColMap = new Dictionary<string, int>();
        int col = startCol;
        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<ImporterHeaderAttribute>();
            if (attr != null && !string.IsNullOrWhiteSpace(attr.Name))
            {
                sheet.Cells[1, col].Value = attr.Name;
                headerColMap[attr.Name] = col;
                sheet.Column(col).Width = columnWidth;
                col++;
            }
        }
        return headerColMap;
    }

    /// <summary>
    /// 写入主表示例数据
    /// </summary>
    /// <param name="sheet">Excel工作表对象</param>
    /// <param name="props">属性列表，用于匹配表头</param>
    /// <param name="row">写入的行号</param>
    /// <param name="headerMap">表头名称到列索引的映射字典</param>
    /// <param name="billTypeList">单据类型列表</param>
    /// <param name="warehouseList">仓库列表</param>
    /// <param name="supplierList">供货单位列表</param>
    /// <param name="manufacturerList">制造商单位列表</param>
    /// <remarks>
    /// 业务规则：
    /// - 使用列表中的第一项作为示例值，如果列表为空则使用默认示例值
    /// - 示例数据包括：标识列、所属仓库、单据类型、供货单位、制造商单位、来源、外部单号、备注
    /// </remarks>
    public void WriteMainTableSampleData(ExcelWorksheet sheet, List<PropertyInfo> props, int row, Dictionary<string, int> headerMap,
        List<string> billTypeList, List<string> warehouseList, List<string> supplierList, List<string> manufacturerList)
    {
        var sampleData = new Dictionary<string, object>
        {
            { "标识列", DEFAULT_SAMPLE_IDENTIFY_CODE },
            { "所属仓库", warehouseList.FirstOrDefault() ?? "示例仓库" },
            { "单据类型", billTypeList.FirstOrDefault() ?? "示例单据类型" },
            { "供货单位", supplierList.FirstOrDefault() ?? "示例供货单位" },
            { "制造商单位", manufacturerList.FirstOrDefault() ?? "示例制造商单位" },
            { "来源", DEFAULT_SOURCE },
            { "外部单号", DEFAULT_SAMPLE_BILL_CODE },
            { "备注", DEFAULT_SAMPLE_REMARK }
        };
        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<ImporterHeaderAttribute>();
            if (attr != null && headerMap.ContainsKey(attr.Name) && sampleData.ContainsKey(attr.Name))
                sheet.Cells[row, headerMap[attr.Name]].Value = sampleData[attr.Name];
        }
    }

    /// <summary>
    /// 写入明细表示例数据
    /// </summary>
    /// <param name="sheet">Excel工作表对象</param>
    /// <param name="props">属性列表，用于匹配表头</param>
    /// <param name="row">写入的行号</param>
    /// <param name="headerMap">表头名称到列索引的映射字典</param>
    /// <param name="materialStatusList">物料状态字典（Label -> Value）</param>
    /// <remarks>
    /// 示例数据字段：
    /// - 标识列：A001
    /// - 物料编码：从数据库查询第一条物料编码
    /// - 批次：1
    /// - 状态：使用字典第一项或"待检"
    /// - 数量：100
    /// - 生产日期：当前日期
    /// - 有效期至：当前日期+180天
    /// - 备注：示例备注
    /// </remarks>
    public void WriteDetailTableSampleData(ExcelWorksheet sheet, List<PropertyInfo> props, int row, Dictionary<string, int> headerMap, Dictionary<string, string> materialStatusList)
    {
        // 从数据库查询第一条物料编码作为示例
        var sampleMaterialCode = _repos.Material.AsQueryable()
            .Where(x => x.IsDelete == false)
            .OrderBy(x => x.Id)
            .Select(x => x.MaterialCode)
            .ToList()
            .FirstOrDefault() ?? DEFAULT_MATERIAL_CODE_FALLBACK;

        // 使用策略字典替代switch语句
        var sampleDataActions = new Dictionary<string, Action<int>>
        {
            { "标识列", col => sheet.Cells[row, col].Value = DEFAULT_SAMPLE_IDENTIFY_CODE },
            { "物料编码", col => sheet.Cells[row, col].Value = sampleMaterialCode },
            { "批次", col => sheet.Cells[row, col].Value = DEFAULT_LOT_NO },
            { "状态", col => sheet.Cells[row, col].Value = materialStatusList?.Values.FirstOrDefault() ?? "待检" },
            { "数量", col => sheet.Cells[row, col].Value = DEFAULT_QUANTITY },
            { "生产日期", col =>
                {
                    sheet.Cells[row, col].Value = DateTime.Now.Date;
                    sheet.Cells[row, col].Style.Numberformat.Format = DATE_FORMAT;
                }
            },
            { "有效期至", col =>
                {
                    sheet.Cells[row, col].Value = DateTime.Now.Date.AddDays(EXPIRY_DAYS);
                    sheet.Cells[row, col].Style.Numberformat.Format = DATE_FORMAT;
                }
            },
            { "备注", col => sheet.Cells[row, col].Value = DEFAULT_SAMPLE_REMARK }
        };

        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<ImporterHeaderAttribute>();
            if (attr == null || !headerMap.ContainsKey(attr.Name)) continue;

            int col = headerMap[attr.Name];
            if (sampleDataActions.TryGetValue(attr.Name, out var action))
            {
                action(col);
            }
        }
    }

    /// <summary>
    /// 设置工作表样式
    /// </summary>
    /// <param name="sheet">Excel工作表对象</param>
    /// <param name="separatorCol">分隔列索引（用于设置黄色背景）</param>
    /// <remarks>
    /// 样式设置：
    /// - 所有单元格居中对齐
    /// - 第1行（表头）字体加粗
    /// - 分隔列设置黄色背景（#FFFF00）
    /// </remarks>
    public void SetSheetStyle(ExcelWorksheet sheet, int separatorCol)
    {
        if (sheet.Dimension == null) return;
        var style = sheet.Cells[HEADER_ROW, HEADER_ROW, sheet.Dimension.End.Row, sheet.Dimension.End.Column].Style;
        style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
        sheet.Cells[HEADER_ROW, HEADER_ROW, HEADER_ROW, sheet.Dimension.End.Column].Style.Font.Bold = true;
        // 设置整列为黄色背景 (#FFFF00)
        var columnStyle = sheet.Column(separatorCol).Style.Fill;
        columnStyle.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
        // EPPlus SetColor使用ARGB格式：(Alpha, Red, Green, Blue)，黄色RGB(255,255,0)对应ARGB(255,255,255,0)
        columnStyle.BackgroundColor.SetColor(255, 255, 255, 0);
    }

    /// <summary>
    /// 添加下拉数据验证
    /// </summary>
    /// <param name="dropdownSheet">下拉数据工作表（存储选项列表）</param>
    /// <param name="mainSheet">主工作表（应用数据验证）</param>
    /// <param name="headerName">表头名称</param>
    /// <param name="dataList">下拉选项列表</param>
    /// <param name="headerMap">表头名称到列索引的映射字典</param>
    /// <param name="dataRow">应用数据验证的行号</param>
    /// <param name="dropdownCol">下拉数据列索引（引用传递，用于递增）</param>
    /// <remarks>
    /// 业务逻辑：
    /// - 在dropdownSheet中写入下拉选项数据
    /// - 在mainSheet的指定单元格添加列表验证
    /// - 验证失败时显示错误提示
    /// </remarks>
    public void AddDropdownValidation(ExcelWorksheet dropdownSheet, ExcelWorksheet mainSheet, string headerName,
        List<string> dataList, Dictionary<string, int> headerMap, int dataRow, ref int dropdownCol)
    {
        if (dataList == null || !dataList.Any() || !headerMap.ContainsKey(headerName)) return;

        dropdownSheet.Cells[1, dropdownCol].Value = headerName;
        int row = 2;
        foreach (var item in dataList.Where(x => !string.IsNullOrWhiteSpace(x)))
            dropdownSheet.Cells[row++, dropdownCol].Value = item;

        int lastRow = row - 1;
        if (lastRow >= 2)
        {
            var validation = mainSheet.DataValidations.AddListValidation(
                mainSheet.Cells[dataRow, headerMap[headerName], dataRow, headerMap[headerName]].Address);
            validation.Formula.ExcelFormula = "=" + dropdownSheet.Cells[2, dropdownCol, lastRow, dropdownCol].FullAddressAbsolute;
            validation.ShowErrorMessage = true;
            validation.ErrorTitle = "无效输入";
            validation.Error = "请从列表中选择一个有效的选项";
        }
        dropdownCol++;
    }

    #endregion

    #region Export 数据导出相关方法

    /// <summary>
    /// 写入主表数据行（用于导出）
    /// </summary>
    /// <param name="sheet">Excel工作表</param>
    /// <param name="row">行号</param>
    /// <param name="mainData">主表数据</param>
    /// <param name="headerColMap">表头列映射</param>
    /// <param name="identifyCode">标识列值（使用入库单号）</param>
    /// <remarks>
    /// 业务逻辑：
    /// - 根据ImportWmsImportNotifyInput的ImporterHeader特性，将主表数据写入对应列
    /// - 标识列使用入库单号（ImportBillCode）
    /// </remarks>
    public void WriteMainTableExportRow(ExcelWorksheet sheet, int row,
        WmsImportNotifyOutput mainData, Dictionary<string, int> headerColMap, string identifyCode)
    {
        var mainProps = typeof(ImportWmsImportNotifyInput).GetProperties()
            .Where(p => {
                var attr = p.GetCustomAttribute<ImporterHeaderAttribute>();
                return attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.Name);
            })
            .ToList();

        foreach (var prop in mainProps)
        {
            var attr = prop.GetCustomAttribute<ImporterHeaderAttribute>();
            if (attr == null || string.IsNullOrWhiteSpace(attr.Name)) continue;

            if (!headerColMap.TryGetValue(attr.Name, out int colIndex)) continue;

            object value = attr.Name switch
            {
                "标识列" => identifyCode,
                "所属仓库" => mainData.WarehouseFkDisplayName,
                "单据类型" => mainData.BillTypeDisplayName,
                "供货单位" => mainData.SupplierName,
                "制造商单位" => mainData.ManufacturerName,
                "来源" => mainData.Source,
                "外部单号" => mainData.OuterBillCode,
                "备注" => mainData.Remark,
                _ => null
            };

            if (value != null)
                sheet.Cells[row, colIndex].Value = value;
        }
    }

    /// <summary>
    /// 写入明细表数据行（用于导出）
    /// </summary>
    /// <param name="sheet">Excel工作表</param>
    /// <param name="row">行号</param>
    /// <param name="detailData">明细数据（包含物料编码和名称）</param>
    /// <param name="headerColMap">表头列映射</param>
    /// <param name="identifyCode">标识列值（与主表一致）</param>
    /// <param name="materialStatusDict">物料状态字典（用于翻译）</param>
    /// <param name="executeFlagDict">执行状态字典（用于翻译）</param>
    /// <remarks>
    /// 业务逻辑：
    /// - 根据ImportWmsImportNotifyDetailInput的ImporterHeader特性，将明细数据写入对应列
    /// - 物料状态需要字典翻译
    /// - 日期格式化为 yyyy-MM-dd
    /// </remarks>
    public void WriteDetailTableExportRow(ExcelWorksheet sheet, int row,
        dynamic detailData, Dictionary<string, int> headerColMap,
        string identifyCode, Dictionary<string, string> materialStatusDict, Dictionary<string, string> executeFlagDict = null)
    {
        var detailProps = typeof(ImportWmsImportNotifyDetailInput).GetProperties()
            .Where(p => {
                var attr = p.GetCustomAttribute<ImporterHeaderAttribute>();
                return attr != null && !attr.IsIgnore && !string.IsNullOrWhiteSpace(attr.Name);
            })
            .ToList();

        foreach (var prop in detailProps)
        {
            var attr = prop.GetCustomAttribute<ImporterHeaderAttribute>();
            if (attr == null || string.IsNullOrWhiteSpace(attr.Name)) continue;

            if (!headerColMap.TryGetValue(attr.Name, out int colIndex)) continue;

            object value = attr.Name switch
            {
                "标识列" => identifyCode,
                "物料编码" => detailData.MaterialCode,
                "物料名称" => detailData.MaterialName,
                "物料规格" => detailData.MaterialStandard,
                "批次" => detailData.LotNo,
                "状态" => GetMaterialStatusLabel(detailData.MaterialStatus, materialStatusDict),
                "计量单位" => detailData.UnitName,
                "数量" => detailData.ImportQuantity,
                "生产日期" => detailData.ImportProductionDate != null ? ((DateTime)detailData.ImportProductionDate).ToString("yyyy-MM-dd") : null,
                "有效期至" => detailData.ImportLostDate != null ? ((DateTime)detailData.ImportLostDate).ToString("yyyy-MM-dd") : null,
                "完成数量" => detailData.ImportCompleteQuantity,
                "已组盘数量" => detailData.ImportFactQuantity,
                "已上传数量" => detailData.UploadQuantity,
                "执行状态" => GetMaterialStatusLabel(detailData.ImportExecuteFlag, executeFlagDict ?? new Dictionary<string, string>()),
                "箱数量" => detailData.BoxQuantity,
                "贴标" => detailData.Labeling == "1" ? "是" : (detailData.Labeling == "0" ? "否" : detailData.Labeling),
                "备注" => detailData.Remark,
                _ => null
            };

            if (value != null)
                sheet.Cells[row, colIndex].Value = value;
        }
    }

    /// <summary>
    /// 获取物料状态标签（用于处理dynamic类型的字典查询）
    /// </summary>
    private static string GetMaterialStatusLabel(dynamic materialStatus, Dictionary<string, string> dict)
    {
        string statusValue = materialStatus?.ToString() ?? "";
        if (string.IsNullOrWhiteSpace(statusValue))
            return "";

        if (dict.TryGetValue(statusValue, out string label))
            return label;

        return statusValue;
    }

    #endregion

    #region ImportData 数据导入相关方法

    #region 外键关联方法

    /// <summary>
    /// 批量构建外键映射字典
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="displayNames">待映射的显示名称列表</param>
    /// <param name="filter">数据库查询过滤条件</param>
    /// <param name="displayNameSelector">显示名称选择器（用于提取实体的显示名称）</param>
    /// <param name="idSelector">ID选择器（用于提取实体的ID）</param>
    /// <returns>显示名称到ID的映射字典</returns>
    /// <remarks>
    /// 业务逻辑：
    /// - 对displayNames去重并过滤空值
    /// - 根据filter查询数据库获取实体列表
    /// - 只返回displayNames中存在的实体映射
    /// </remarks>
    private Dictionary<string, long?> BuildForeignKeyLinkMap<TEntity>(
        List<string> displayNames,
        Expression<Func<TEntity, bool>> filter,
        Func<TEntity, string> displayNameSelector,
        Func<TEntity, long> idSelector) where TEntity : class, new()
    {
        var distinctNames = displayNames.Distinct().Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (!distinctNames.Any())
            return new Dictionary<string, long?>();

        var items = _sqlSugarClient.Queryable<TEntity>()
            .Where(filter)
            .ToList()
            .Where(x => distinctNames.Contains(displayNameSelector(x)))
            .ToList();

        return items.ToDictionary(displayNameSelector, x => (long?)idSelector(x));
    }

    /// <summary>
    /// 验证并设置外键ID
    /// </summary>
    /// <typeparam name="TInput">输入类型</typeparam>
    /// <param name="input">输入对象</param>
    /// <param name="displayName">显示名称</param>
    /// <param name="linkMap">显示名称到ID的映射字典</param>
    /// <param name="setIdAction">设置ID的委托</param>
    /// <param name="fieldDisplayName">字段显示名称（用于错误提示）</param>
    /// <param name="appendErrorAction">追加错误的委托</param>
    /// <remarks>
    /// 业务逻辑：
    /// - 如果displayName为空，直接返回
    /// - 从linkMap中查找对应的ID
    /// - 如果ID不存在，调用appendErrorAction记录错误
    /// </remarks>
    private void LinkForeignKeyField<TInput>(
        TInput input,
        string displayName,
        Dictionary<string, long?> linkMap,
        Action<TInput, long?> setIdAction,
        string fieldDisplayName,
        Action<TInput, string> appendErrorAction)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return;

        var id = linkMap.GetValueOrDefault(displayName);
        setIdAction(input, id);

        if (id == null)
            appendErrorAction(input, $"{fieldDisplayName} [{displayName}] 不存在");
    }

    #endregion

    #region 字典映射方法

    /// <summary>
    /// 批量加载字典映射（Label -> Value）
    /// </summary>
    /// <param name="dictCode">字典编码</param>
    /// <returns>字典Label到Value的映射字典</returns>
    /// <remarks>
    /// 业务逻辑：
    /// - 通过SysDictTypeService查询指定编码的字典数据
    /// - 返回Label到Value的映射关系
    /// - 如果字典不存在或查询失败，返回空字典
    /// </remarks>
    private Dictionary<string, string> LoadDictMap(string dictCode)
    {
        return _dictTypeService.GetDataList(new GetDataDictTypeInput { Code = dictCode })
            .Result?.ToDictionary(x => x.Label, x => x.Value) ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// 映射字典字段并验证
    /// </summary>
    /// <typeparam name="TInput">输入类型</typeparam>
    /// <param name="input">输入对象</param>
    /// <param name="labelValue">Label值（用户输入的显示值）</param>
    /// <param name="dictMap">字典Label到Value的映射</param>
    /// <param name="setValueAction">设置Value的委托</param>
    /// <param name="fieldDisplayName">字段显示名称（用于错误提示）</param>
    /// <param name="appendErrorAction">追加错误的委托</param>
    /// <remarks>
    /// 业务逻辑：
    /// - 如果labelValue为空，直接返回
    /// - 从dictMap中查找对应的Value
    /// - 如果Value不存在，调用appendErrorAction记录错误
    /// </remarks>
    private void MapDictField<TInput>(
        TInput input,
        string labelValue,
        Dictionary<string, string> dictMap,
        Action<TInput, string> setValueAction,
        string fieldDisplayName,
        Action<TInput, string> appendErrorAction)
    {
        if (string.IsNullOrWhiteSpace(labelValue))
            return;

        var value = dictMap.GetValueOrDefault(labelValue);
        setValueAction(input, value);

        if (value == null)
            appendErrorAction(input, $"{fieldDisplayName} [{labelValue}] 不是有效的选项");
    }

    #endregion

    #region 字段长度验证方法

    /// <summary>
    /// 验证字段长度
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="entity">实体对象</param>
    /// <param name="getValue">获取字段值的函数</param>
    /// <param name="maxLength">最大长度</param>
    /// <param name="fieldName">字段显示名称（用于错误提示）</param>
    /// <param name="appendErrorAction">追加错误的委托</param>
    /// <remarks>
    /// 验证规则：
    /// - 如果字段值不为null且长度超过maxLength，则记录错误
    /// </remarks>
    private void ValidateFieldLength<TEntity>(
        TEntity entity,
        Func<TEntity, string> getValue,
        int maxLength,
        string fieldName,
        Action<TEntity, string> appendErrorAction)
    {
        var value = getValue(entity);
        if (value != null && value.Length > maxLength)
        {
            appendErrorAction(entity, $"{fieldName}长度不能超过{maxLength}个字符");
        }
    }

    #endregion

    #region Excel文件处理方法

    /// <summary>
    /// 上传Excel文件并获取物理路径
    /// </summary>
    /// <param name="file">上传的文件</param>
    /// <returns>文件ID和文件物理路径</returns>
    /// <remarks>
    /// 业务逻辑：
    /// - 通过SysFileService上传文件到服务器
    /// - 拼接完整的物理路径（WebRootPath + FilePath + 文件名+后缀）
    /// </remarks>
    public async Task<(long FileId, string FilePath)> UploadAndGetExcelPath(IFormFile file)
    {
        var fileService = App.GetService<SysFileService>();
        var tempFile = await fileService.UploadFile(new UploadFileInput { File = file });
        var filePath = Path.Combine(App.WebHostEnvironment.WebRootPath, tempFile.FilePath!, tempFile.Id + tempFile.Suffix);
        return (tempFile.Id, filePath);
    }

    /// <summary>
    /// 清理临时文件（带异常日志，不抛出异常）
    /// </summary>
    /// <param name="fileId">文件ID</param>
    /// <param name="filePath">文件物理路径</param>
    /// <remarks>
    /// 业务逻辑：
    /// - 删除物理文件
    /// - 通过SysFileService删除数据库中的文件记录
    /// - 异常处理：捕获所有异常并记录日志，不向上抛出
    /// </remarks>
    public async Task CleanupTempExcelFile(long fileId, string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var fileService = App.GetService<SysFileService>();
            await fileService.DeleteFile(new BaseIdInput { Id = fileId });
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"清理临时文件失败: FileId={fileId}, Path={filePath}, Error={ex.Message}");
        }
    }

    #endregion

    /// <summary>
    /// 批量加载主表关联数据映射
    /// </summary>
    /// <param name="mainTableList">主表数据项列表</param>
    /// <returns>
    /// 返回一个元组，包含四个映射字典：
    /// - Warehouse：仓库名称到仓库ID的映射
    /// - BillType：单据类型名称到单据类型ID的映射
    /// - Supplier：供货单位名称到供货单位ID的映射
    /// - Manufacturer：制造商单位名称到制造商单位ID的映射
    /// </returns>
    /// <remarks>
    /// 过滤条件：
    /// - 仓库：未删除的记录
    /// - 单据类型：未删除且BillType=1（入库单）的记录
    /// - 供货单位：未删除的记录
    /// - 制造商单位：未删除的记录
    /// </remarks>
    public (Dictionary<string, long?> Warehouse, Dictionary<string, long?> BillType,
             Dictionary<string, long?> Supplier, Dictionary<string, long?> Manufacturer)
        LoadMainTableLinkMaps(List<ImportWmsImportNotifyInput> mainTableList)
    {
        var warehouseMap = BuildForeignKeyLinkMap(
            mainTableList.Select(x => x.WarehouseFkDisplayName).ToList(),
            (WmsBaseWareHouse x) => x.IsDelete == false,
            x => x.WarehouseName,
            x => x.Id);

        var billTypeMap = BuildForeignKeyLinkMap(
            mainTableList.Select(x => x.BillTypeName).ToList(),
            (WmsBaseBillType x) => x.IsDelete == false && x.BillType == 1,
            x => x.BillTypeName,
            x => x.Id);

        var supplierMap = BuildForeignKeyLinkMap(
            mainTableList.Select(x => x.SupplierFkDisplayName).ToList(),
            (WmsBaseSupplier x) => x.IsDelete == false,
            x => x.CustomerName,
            x => x.Id);

        var manufacturerMap = BuildForeignKeyLinkMap(
            mainTableList.Select(x => x.ManufacturerFkDisplayName).ToList(),
            (WmsBaseManufacturer x) => x.IsDelete == false,
            x => x.CustomerName,
            x => x.Id);

        return (warehouseMap, billTypeMap, supplierMap, manufacturerMap);
    }

    /// <summary>
    /// 链接主表字段
    /// </summary>
    /// <param name="pageItem">主表数据项</param>
    /// <param name="linkMaps">外键映射字典元组</param>
    /// <remarks>
    /// 业务逻辑：
    /// - 将主表的显示名称字段（仓库名称、单据类型名称等）转换为对应的ID
    /// - 如果对应ID不存在，则记录错误信息
    /// </remarks>
    public void LinkMainTableFields(ImportWmsImportNotifyInput pageItem,
        (Dictionary<string, long?> Warehouse, Dictionary<string, long?> BillType,
         Dictionary<string, long?> Supplier, Dictionary<string, long?> Manufacturer) linkMaps)
    {
        LinkForeignKeyField(
            pageItem,
            pageItem.WarehouseFkDisplayName,
            linkMaps.Warehouse,
            (input, id) => input.WarehouseId = id,
            "所属仓库",
            (input, error) =>
            {
                if (string.IsNullOrWhiteSpace(input.Error))
                    input.Error = error;
            });

        LinkForeignKeyField(
            pageItem,
            pageItem.BillTypeName,
            linkMaps.BillType,
            (input, id) => input.BillType = id,
            "单据类型",
            (input, error) => AppendError(input, true, error));

        LinkForeignKeyField(
            pageItem,
            pageItem.SupplierFkDisplayName,
            linkMaps.Supplier,
            (input, id) => input.SupplierId = id,
            "供货单位",
            (input, error) => AppendError(input, true, error));

        LinkForeignKeyField(
            pageItem,
            pageItem.ManufacturerFkDisplayName,
            linkMaps.Manufacturer,
            (input, id) => input.ManufacturerId = id,
            "制造商单位",
            (input, error) => AppendError(input, true, error));
    }

    /// <summary>
    /// 设置主表默认值
    /// </summary>
    /// <param name="row">入库单实体</param>
    /// <param name="orgId">组织ID</param>
    /// <param name="userId">用户ID</param>
    /// <param name="userName">用户名称</param>
    /// <remarks>
    /// 默认值设置：
    /// - DepartmentId：组织ID
    /// - ImportExecuteFlag：01（待执行）
    /// - Source：WMS
    /// - CreateUserId/UpdateUserId：当前用户ID
    /// - UpdateUserName：当前用户名称
    /// - UpdateTime：当前时间
    /// </remarks>
    public void SetMainTableDefaults(WmsImportNotify row, long orgId, long userId, string userName)
    {
        row.DepartmentId = orgId;
        row.ImportExecuteFlag = DEFAULT_EXECUTING_FLAG;
        row.Source = DEFAULT_SOURCE;
        row.CreateUserId = userId;
        row.UpdateUserId = userId;
        row.UpdateUserName = userName;
        row.UpdateTime = DateTime.Now;
    }

    /// <summary>
    /// 添加错误信息（主表）
    /// </summary>
    public void AppendError(ImportWmsImportNotifyInput item, bool condition, string message)
    {
        if (condition)
        {
            item.Error = string.IsNullOrWhiteSpace(item.Error) ? message : $"{item.Error}; {message}";
        }
    }

    /// <summary>
    /// 添加错误信息（明细表）
    /// </summary>
    public void AppendError(ImportWmsImportNotifyDetailInput item, bool condition, string message)
    {
        if (condition)
            item.Error = string.IsNullOrWhiteSpace(item.Error) ? message : $"{item.Error}; {message}";
    }

    /// <summary>
    /// 建立标识列到主表ID的映射
    /// </summary>
    /// <param name="pageItems">主表数据项列表（Excel导入数据）</param>
    /// <param name="rows">主表实体列表（已插入数据库的数据）</param>
    /// <returns>标识列到主表ID的映射字典</returns>
    /// <remarks>
    /// 业务逻辑：
    /// - 标识列：用于关联主表和明细表的唯一标识（Excel中的自定义字段）
    /// - 通过两层映射建立关联：标识列 -> 入库单号 -> 主表ID
    /// - 同一标识列的多条数据只取第一条的入库单号
    /// </remarks>
    public Dictionary<string, long> BuildIdentifyCodeToIdMap(List<ImportWmsImportNotifyInput> pageItems, List<WmsImportNotify> rows)
    {
        var identifyCodeToIdMap = new Dictionary<string, long>();
        var identifyCodeToBillCodeMap = pageItems
            .Where(x => !string.IsNullOrWhiteSpace(x.IdentifyCode) && !string.IsNullOrWhiteSpace(x.ImportBillCode))
            .GroupBy(x => x.IdentifyCode)
            .ToDictionary(g => g.Key, g => g.First().ImportBillCode);

        var billCodeToIdMap = rows
            .Where(x => !string.IsNullOrWhiteSpace(x.ImportBillCode) && x.Id > 0)
            .ToDictionary(x => x.ImportBillCode, x => x.Id);

        foreach (var kvp in identifyCodeToBillCodeMap)
        {
            if (billCodeToIdMap.TryGetValue(kvp.Value, out var id))
                identifyCodeToIdMap[kvp.Key] = id;
        }

        return identifyCodeToIdMap;
    }

    /// <summary>
    /// 链接明细表到主表
    /// </summary>
    /// <param name="detailPageItems">明细表数据项列表</param>
    /// <param name="identifyCodeToIdMap">标识列到主表ID的映射字典</param>
    /// <remarks>
    /// 业务逻辑：
    /// - 通过标识列将明细表关联到主表
    /// - 如果标识列为空，记录错误
    /// - 如果标识列在主表中不存在，记录错误
    /// </remarks>
    public void LinkDetailTableToMainTable(List<ImportWmsImportNotifyDetailInput> detailPageItems, Dictionary<string, long> identifyCodeToIdMap)
    {
        foreach (var e in detailPageItems)
        {
            if (string.IsNullOrWhiteSpace(e.IdentifyCode))
            {
                AppendError(e, true, "标识列不能为空");
                continue;
            }

            if (identifyCodeToIdMap.TryGetValue(e.IdentifyCode, out var importId))
                e.ImportId = importId;
            else
                AppendError(e, true, $"标识列 [{e.IdentifyCode}] 在主表中未找到");
        }
    }

    /// <summary>
    /// 链接物料信息
    /// </summary>
    /// <param name="detailPageItems">明细表数据项列表</param>
    /// <remarks>
    /// 业务逻辑：
    /// - 根据物料编码查询物料信息
    /// - 如果物料编码重复，使用第一条记录
    /// - 如果物料编码不存在，记录错误
    /// </remarks>
    public void LinkMaterialInfo(List<ImportWmsImportNotifyDetailInput> detailPageItems)
    {
        var materialCodes = detailPageItems.Where(x => string.IsNullOrWhiteSpace(x.Error) && !string.IsNullOrWhiteSpace(x.MaterialCode))
            .Select(x => x.MaterialCode).Distinct().ToList();
        if (!materialCodes.Any()) return;

        var materialList = _repos.Material.AsQueryable()
            .Where(u => materialCodes.Contains(u.MaterialCode))
            .ToList();

        // 检查并记录物料编码重复的情况
        var duplicateMaterials = materialList
            .GroupBy(u => u.MaterialCode)
            .Where(g => g.Count() > 1)
            .Select(g => new { MaterialCode = g.Key, Count = g.Count() })
            .ToList();

        if (duplicateMaterials.Any())
        {
            _logger.LogWarning($"数据库中存在重复的物料编码: {string.Join(", ", duplicateMaterials.Select(x => $"{x.MaterialCode}({x.Count}条)"))}，将使用第一条记录");
        }

        // 处理物料编码重复的情况，按MaterialCode分组取第一条
        var materialMap = materialList
            .GroupBy(u => u.MaterialCode)
            .ToDictionary(g => g.Key, g => (long?)g.First().Id);

        foreach (var e in detailPageItems)
        {
            if (string.IsNullOrWhiteSpace(e.Error))
            {
                if (string.IsNullOrWhiteSpace(e.MaterialCode))
                    AppendError(e, true, "物料编码不能为空");
                else if (!materialMap.TryGetValue(e.MaterialCode, out var materialId) || materialId == null)
                    AppendError(e, true, $"物料编码 [{e.MaterialCode}] 不存在");
                else
                    e.MaterialId = materialId.Value;
            }
        }
    }

    /// <summary>
    /// 验证明细表数据
    /// </summary>
    /// <param name="detailPageItems">明细表数据项列表</param>
    /// <remarks>
    /// 验证规则：
    /// - 批号：如果不为空，必须为纯数字
    /// - 数量：不能为空，且必须大于0
    /// </remarks>
    public void ValidateDetailTableData(List<ImportWmsImportNotifyDetailInput> detailPageItems)
    {
        foreach (var e in detailPageItems.Where(x => string.IsNullOrWhiteSpace(x.Error)))
        {
            if (!string.IsNullOrWhiteSpace(e.LotNo) && !System.Text.RegularExpressions.Regex.IsMatch(e.LotNo, @"^\d+$"))
                AppendError(e, true, "批次只能输入数字");

            if (!e.ImportQuantity.HasValue)
                AppendError(e, true, "数量不能为空");
            else if (e.ImportQuantity.Value <= 0)
                AppendError(e, true, "数量必须大于0");
        }
    }

    /// <summary>
    /// 映射物料状态
    /// </summary>
    /// <param name="detailPageItems">明细表数据项列表</param>
    /// <remarks>
    /// 业务逻辑：
    /// - 从字典"MaterialStatus"加载状态映射
    /// - 将用户输入的状态Label转换为对应的Value
    /// </remarks>
    public void MapMaterialStatus(List<ImportWmsImportNotifyDetailInput> detailPageItems)
    {
        var materialStatusMap = LoadDictMap(DICT_CODE_MATERIAL_STATUS);
        if (!materialStatusMap.Any()) return;

        foreach (var e in detailPageItems.Where(x => string.IsNullOrWhiteSpace(x.Error) && !string.IsNullOrWhiteSpace(x.MaterialStatus)))
        {
            MapDictField(
                e,
                e.MaterialStatus,
                materialStatusMap,
                (input, value) => input.MaterialStatus = value,
                "状态",
                (input, error) => AppendError(input, true, error));
        }
    }

    /// <summary>
    /// 检查明细表重复数据
    /// </summary>
    /// <param name="detailPageItems">明细表数据项列表</param>
    /// <remarks>
    /// 重复判断规则：
    /// - 同一主表（ImportId）下，物料编码和批次的组合不能重复
    /// - 如果发现重复，只保留第一条，后续记录标记为错误
    /// </remarks>
    public void CheckDuplicateDetails(List<ImportWmsImportNotifyDetailInput> detailPageItems)
    {
        var validItems = detailPageItems.Where(x => string.IsNullOrWhiteSpace(x.Error) && x.ImportId != null && x.ImportId > 0).ToList();
        var groups = validItems.GroupBy(x => x.ImportId);

        foreach (var group in groups)
        {
            var seen = new HashSet<string>();
            foreach (var item in group)
            {
                if (string.IsNullOrWhiteSpace(item.MaterialCode) || string.IsNullOrWhiteSpace(item.LotNo)) continue;
                var key = $"{item.MaterialCode}_{item.LotNo}";
                if (!seen.Add(key))
                    AppendError(item, true, $"物料编码 [{item.MaterialCode}] 和批次 [{item.LotNo}] 的组合重复，数据库中只保存第一条");
            }
        }
    }

    /// <summary>
    /// 将Excel单元格的日期值转换为DateTime
    /// </summary>
    /// <param name="worksheet">Excel工作表对象</param>
    /// <param name="row">行号</param>
    /// <param name="col">列号</param>
    /// <param name="cellValue">单元格值</param>
    /// <returns>转换后的DateTime，如果转换失败则返回null</returns>
    /// <remarks>
    /// 转换策略（按优先级）：
    /// 1. 如果值已经是DateTime类型，直接返回
    /// 2. 如果值是double类型（Excel日期序列号），使用FromOADate转换
    /// 3. 如果单元格格式包含日期格式字符（d/m/y），使用EPPlus的GetValue方法
    /// 4. 尝试将值作为字符串解析
    /// </remarks>
    public DateTime? ConvertExcelDateToDateTime(ExcelWorksheet worksheet, int row, int col, object cellValue)
    {
        if (cellValue is DateTime dateTimeValue)
            return dateTimeValue;

        if (cellValue is double doubleValue)
        {
            try { return DateTime.FromOADate(doubleValue); }
            catch { /* 继续尝试其他方式 */ }
        }

        var cell = worksheet.Cells[row, col];
        if (cell.Style.Numberformat.Format != null &&
            (cell.Style.Numberformat.Format.Contains("d") || cell.Style.Numberformat.Format.Contains("m") || cell.Style.Numberformat.Format.Contains("y")))
        {
            try { return cell.GetValue<DateTime?>(); }
            catch { /* 继续尝试字符串解析 */ }
        }

        if (DateTime.TryParse(cellValue?.ToString(), out var parsedDate))
            return parsedDate;

        return null;
    }

    /// <summary>
    /// 导入子表数据（明细表）
    /// </summary>
    /// <param name="filePath">Excel文件路径（已在事务外上传）</param>
    /// <param name="pageItems">主表数据项列表（用于建立标识列映射）</param>
    /// <param name="rows">主表实体列表（已插入数据库，包含生成的ID）</param>
    /// <exception cref="FileNotFoundException">文件不存在时抛出</exception>
    /// <exception cref="InvalidOperationException">Excel格式不正确时抛出</exception>
    /// <remarks>
    /// 业务逻辑：
    /// 1. 从Excel第10-17列读取明细表数据（横向布局）
    /// 2. 通过标识列将明细表关联到主表
    /// 3. 验证物料编码、批次、数量等字段
    /// 4. 分批插入数据库（每批2048条）
    ///
    /// 注意事项：
    /// - Excel格式要求：第1行为表头，第2行开始为数据
    /// - 主表和明细表通过标识列进行关联
    /// - 重复的物料编码+批次组合只保存第一条
    /// </remarks>
    public void ImportDetailData(string filePath, List<ImportWmsImportNotifyInput> pageItems, List<WmsImportNotify> rows)
    {
        try
        {
            var detailList = ReadDetailDataFromExcel(filePath);

            _logger.LogInformation($"明细表读取完成，共读取 {detailList.Count} 条数据");

            if (!detailList.Any() || rows == null || !rows.Any() || pageItems == null)
            {
                _logger.LogWarning($"明细表数据为空或条件不满足 - detailList.Count: {detailList.Count}, rows: {rows?.Count ?? 0}, pageItems: {pageItems?.Count ?? 0}");
                return;
            }

            var identifyCodeToIdMap = BuildIdentifyCodeToIdMap(pageItems, rows);
            _logger.LogInformation($"标识列映射建立完成，共 {identifyCodeToIdMap.Count} 个映射");

            // 处理子表数据
            _sqlSugarClient.Utilities.PageEach(detailList, BATCH_INSERT_SIZE, detailPageItems =>
            {
                ProcessDetailPageItems(detailPageItems, identifyCodeToIdMap);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"导入明细表数据失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 从Excel文件读取明细表数据
    /// </summary>
    private List<ImportWmsImportNotifyDetailInput> ReadDetailDataFromExcel(string filePath)
    {
        var detailList = new List<ImportWmsImportNotifyDetailInput>();
        using (var excelPackage = new ExcelPackage(new FileInfo(filePath)))
        {
            var worksheet = excelPackage.Workbook.Worksheets[0];
            if (worksheet == null || worksheet.Dimension == null)
                return detailList;

            // 读取明细表表头映射
            var detailHeaderColMap = ReadDetailHeaderMap(worksheet);

            // 从第2行开始读取明细表数据
            _logger.LogInformation($"开始读取明细表数据，总行数: {worksheet.Dimension.End.Row}");
            for (int row = DATA_START_ROW; row <= worksheet.Dimension.End.Row; row++)
            {
                var detailItem = ReadDetailRow(worksheet, row, detailHeaderColMap);
                if (detailItem != null)
                {
                    detailList.Add(detailItem);
                }
            }
        }
        return detailList;
    }

    /// <summary>
    /// 读取明细表表头映射
    /// </summary>
    private Dictionary<string, int> ReadDetailHeaderMap(ExcelWorksheet worksheet)
    {
        var headerMap = new Dictionary<string, int>();
        for (int col = DETAIL_TABLE_START_COL; col <= DETAIL_TABLE_END_COL; col++)
        {
            var headerValue = worksheet.Cells[HEADER_ROW, col].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                headerMap[headerValue] = col;
            }
        }
        return headerMap;
    }

    /// <summary>
    /// 读取单行明细表数据
    /// </summary>
    private ImportWmsImportNotifyDetailInput ReadDetailRow(ExcelWorksheet worksheet, int row, Dictionary<string, int> headerMap)
    {
        // 检查明细表标识列是否为空
        var firstDetailCellValue = worksheet.Cells[row, DETAIL_TABLE_START_COL].Value?.ToString();
        if (string.IsNullOrWhiteSpace(firstDetailCellValue))
        {
            _logger.LogDebug($"第{row}行明细表标识列为空，跳过");
            return null;
        }

        _logger.LogDebug($"第{row}行明细表标识列: {firstDetailCellValue}");
        var detailItem = new ImportWmsImportNotifyDetailInput();

        // 读取明细表各列数据
        foreach (var prop in typeof(ImportWmsImportNotifyDetailInput).GetProperties())
        {
            var importerAttr = prop.GetCustomAttribute<ImporterHeaderAttribute>();
            if (importerAttr == null || importerAttr.IsIgnore || string.IsNullOrWhiteSpace(importerAttr.Name))
                continue;

            if (headerMap.TryGetValue(importerAttr.Name, out var colIndex))
            {
                var cellValue = worksheet.Cells[row, colIndex].Value;
                SetPropertyValue(detailItem, prop, cellValue, worksheet, row, colIndex);
            }
        }

        return detailItem;
    }

    /// <summary>
    /// 根据属性类型设置属性值
    /// </summary>
    private void SetPropertyValue(ImportWmsImportNotifyDetailInput detailItem, PropertyInfo prop, object cellValue, ExcelWorksheet worksheet, int row, int colIndex)
    {
        if (cellValue == null) return;

        var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        if (propType == typeof(string))
        {
            prop.SetValue(detailItem, cellValue.ToString());
        }
        else if (propType == typeof(decimal) && decimal.TryParse(cellValue.ToString(), out var decValue))
        {
            prop.SetValue(detailItem, decValue);
        }
        else if (propType == typeof(DateTime))
        {
            var dtValue = ConvertExcelDateToDateTime(worksheet, row, colIndex, cellValue);
            if (dtValue.HasValue)
                prop.SetValue(detailItem, dtValue.Value);
        }
        else if (propType == typeof(long) && long.TryParse(cellValue.ToString(), out var longValue))
        {
            prop.SetValue(detailItem, longValue);
        }
    }

    /// <summary>
    /// 处理明细表数据页
    /// </summary>
    private void ProcessDetailPageItems(List<ImportWmsImportNotifyDetailInput> detailPageItems, Dictionary<string, long> identifyCodeToIdMap)
    {
        LinkDetailTableToMainTable(detailPageItems, identifyCodeToIdMap);
        LinkMaterialInfo(detailPageItems);
        ValidateDetailTableData(detailPageItems);
        MapMaterialStatus(detailPageItems);
        CheckDuplicateDetails(detailPageItems);

        var validDetailPageItems = detailPageItems.Where(x =>
            string.IsNullOrWhiteSpace(x.Error) && x.ImportId != null && x.ImportId > 0).ToList();

        LogValidationResults(detailPageItems, validDetailPageItems);

        if (validDetailPageItems.Any())
        {
            InsertValidDetailRows(validDetailPageItems);
        }
    }

    /// <summary>
    /// 记录验证结果日志
    /// </summary>
    private void LogValidationResults(List<ImportWmsImportNotifyDetailInput> allItems, List<ImportWmsImportNotifyDetailInput> validItems)
    {
        var errorItems = allItems.Where(x => !string.IsNullOrWhiteSpace(x.Error)).ToList();
        if (errorItems.Any())
        {
            _logger.LogWarning($"明细表有 {errorItems.Count} 条数据校验失败: {string.Join("; ", errorItems.Select(x => $"[{x.MaterialCode}] {x.Error}"))}");
        }
        _logger.LogInformation($"明细表有效数据: {validItems.Count} 条");
    }

    /// <summary>
    /// 插入有效的明细表数据
    /// </summary>
    private void InsertValidDetailRows(List<ImportWmsImportNotifyDetailInput> validDetailPageItems)
    {
        // 转换为实体列表，保持顺序以便后续匹配错误
        var detailRows = validDetailPageItems.Adapt<List<WmsImportNotifyDetail>>();

        // 确保ImportId正确设置
        for (int i = 0; i < detailRows.Count; i++)
        {
            if (detailRows[i].ImportId == null || detailRows[i].ImportId == 0)
                detailRows[i].ImportId = validDetailPageItems[i].ImportId;
        }

        // 设置子表默认值
        SetDetailTableDefaults(detailRows);

        // 校验数据长度并过滤错误数据
        ValidateDetailTableLengths(detailRows, validDetailPageItems);
        var validDetailRows = detailRows.Where((row, index) =>
            string.IsNullOrWhiteSpace(validDetailPageItems[index].Error)).ToList();

        _logger.LogInformation($"准备插入明细表数据: {validDetailRows.Count} 条");
        if (validDetailRows.Any())
        {
            _repos.ImportNotifyDetail.InsertRange(validDetailRows);
            _logger.LogInformation($"明细表数据插入成功: {validDetailRows.Count} 条");
        }
    }

    /// <summary>
    /// 设置明细表默认值
    /// </summary>
    /// <param name="detailRows">明细表实体列表</param>
    /// <remarks>
    /// 默认值设置：
    /// - ImportListNo：序号（从1开始递增）
    /// - ImportExecuteFlag：01（待执行）
    /// - ImportTaskStatus/TaskStatus：0（未开始）
    /// - PrintFlag：0（未打印）
    /// - LabelJudgment：2（无需标签）
    /// - ImportCompleteQuantity/ImportFactQuantity/UploadQuantity/outQty：0
    /// - ImportLostDate：生产日期+180天（如果生产日期存在）
    /// - UpdateUserId/UpdateUserName：当前用户
    /// - UpdateTime：当前时间
    /// </remarks>
    public void SetDetailTableDefaults(List<WmsImportNotifyDetail> detailRows)
    {
        var userId = Convert.ToInt64(App.User?.FindFirst(ClaimConst.UserId)?.Value);
        var userName = App.User?.FindFirst(ClaimConst.RealName)?.Value;
        var now = DateTime.Now;

        for (int i = 0; i < detailRows.Count; i++)
        {
            var e = detailRows[i];
            e.ImportListNo = (i + 1).ToString();
            e.ImportExecuteFlag = DEFAULT_EXECUTING_FLAG;
            e.ImportTaskStatus = 0;
            e.TaskStatus = 0;
            e.PrintFlag = 0;
            e.LabelJudgment = 2;
            e.ImportCompleteQuantity = 0;
            e.ImportFactQuantity = 0;
            e.UploadQuantity = 0;
            e.outQty = 0;
            if (e.ImportProductionDate.HasValue)
                e.ImportLostDate = e.ImportProductionDate.Value.AddDays(EXPIRY_DAYS);
            e.UpdateUserId = userId;
            e.UpdateUserName = userName;
            e.UpdateTime = now;
        }
    }

    /// <summary>
    /// 验证明细表数据长度
    /// </summary>
    /// <param name="detailRows">明细表实体列表</param>
    /// <param name="detailPageItems">明细表数据项列表（用于记录错误）</param>
    /// <remarks>
    /// 字段长度限制：
    /// - 外部单据明细ID：32字符
    /// - 序号：32字符
    /// - 批次：32字符
    /// - 物料状态：32字符
    /// - 执行状态：2字符
    /// - 备注：50字符
    /// - 巷道编码：32字符
    /// - 采浆公司：50字符
    /// </remarks>
    public void ValidateDetailTableLengths(List<WmsImportNotifyDetail> detailRows, List<ImportWmsImportNotifyDetailInput> detailPageItems)
    {
        for (int i = 0; i < detailRows.Count; i++)
        {
            var row = detailRows[i];
            ValidateFieldLength(row, r => r.OuterDetailId, MAX_LENGTH_OUTER_DETAIL_ID, "外部单据明细ID", (entity, error) => AppendError(detailPageItems[i], true, error));
            ValidateFieldLength(row, r => r.ImportListNo, MAX_LENGTH_LIST_NO, "序号", (entity, error) => AppendError(detailPageItems[i], true, error));
            ValidateFieldLength(row, r => r.LotNo, MAX_LENGTH_LOT_NO, "批次", (entity, error) => AppendError(detailPageItems[i], true, error));
            ValidateFieldLength(row, r => r.MaterialStatus, MAX_LENGTH_MATERIAL_STATUS, "物料状态", (entity, error) => AppendError(detailPageItems[i], true, error));
            ValidateFieldLength(row, r => r.ImportExecuteFlag, MAX_LENGTH_EXECUTE_FLAG, "执行状态", (entity, error) => AppendError(detailPageItems[i], true, error));
            ValidateFieldLength(row, r => r.Remark, MAX_LENGTH_REMARK, "备注", (entity, error) => AppendError(detailPageItems[i], true, error));
            ValidateFieldLength(row, r => r.LanewayCode, MAX_LENGTH_LANEWAY_CODE, "巷道编码", (entity, error) => AppendError(detailPageItems[i], true, error));
            ValidateFieldLength(row, r => r.Xj_HouseCode, MAX_LENGTH_XJ_HOUSE_CODE, "采浆公司", (entity, error) => AppendError(detailPageItems[i], true, error));
        }
    }

    #endregion
}
