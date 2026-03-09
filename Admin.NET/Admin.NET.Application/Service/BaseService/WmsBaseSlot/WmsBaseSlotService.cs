// Admin.NET 项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE-MIT 和 LICENSE-APACHE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Admin.NET.Application.Entity;
using Admin.NET.Core.Service;
using Dm.util;
using DocumentFormat.OpenXml.Drawing;
using Furion.DatabaseAccessor;
using Furion.FriendlyException;
using Mapster;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static SKIT.FlurlHttpClient.Wechat.Api.Models.ChannelsECWarehouseGetResponse.Types;
namespace Admin.NET.Application;

/// <summary>
/// 储位管理服务 🧩
/// </summary>
[ApiDescriptionSettings(ApplicationConst.GroupName, Order = 100)]
public partial class WmsBaseSlotService : IDynamicApiController, ITransient
{
    private readonly SqlSugarRepository<WmsBaseSlot> _wmsBaseSlotRep;
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SysDictTypeService _sysDictTypeService;
    private readonly SysCacheService _sysCacheService;
    private readonly SqlSugarRepository<WmsBaseWareHouse> _wmsBaseWareHouseRep;

    public WmsBaseSlotService(SqlSugarRepository<WmsBaseSlot> wmsBaseSlotRep, ISqlSugarClient sqlSugarClient, SysDictTypeService sysDictTypeService, SysCacheService sysCacheService, SqlSugarRepository<WmsBaseWareHouse> wmsBaseWareHouseRep)
    {
        _wmsBaseSlotRep = wmsBaseSlotRep;
        _sqlSugarClient = sqlSugarClient;
        _sysDictTypeService = sysDictTypeService;
        _sysCacheService = sysCacheService;
        _wmsBaseWareHouseRep = wmsBaseWareHouseRep;
    }

    /// <summary>
    /// 分页查询储位管理 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("分页查询储位管理")]
    [ApiDescriptionSettings(Name = "Page"), HttpPost]
    public async Task<SqlSugarPagedList<WmsBaseSlotOutput>> Page(PageWmsBaseSlotInput input)
    {
        input.Keyword = input.Keyword?.Trim();
        var query = _wmsBaseSlotRep.AsQueryable()
            .LeftJoin<WmsBaseWareHouse>((u, warehouse) => u.WarehouseId == warehouse.Id)
            .LeftJoin<WmsBaseArea>((u, warehouse, slotArea) => u.SlotAreaId == slotArea.Id)
            .LeftJoin<WmsBaseLaneway>((u, warehouse, slotArea, slotLaneway) => u.SlotLanewayId == slotLaneway.Id)
            .WhereIF(!string.IsNullOrWhiteSpace(input.Keyword), (u,warehouse,slotArea, slotLaneway) =>
             warehouse.WarehouseName.Contains(input.Keyword)
            || slotArea.AreaCode.Contains(input.Keyword)
            || slotArea.AreaName.Contains(input.Keyword)
            || slotLaneway.LanewayCode.Contains(input.Keyword)
            || slotLaneway.LanewayName.Contains(input.Keyword)
            || u.SlotCode.Contains(input.Keyword) || u.Make.Contains(input.Keyword) || u.Property.Contains(input.Keyword) || u.Handle.Contains(input.Keyword) || u.Environment.Contains(input.Keyword) || u.Remark.Contains(input.Keyword) || u.SlotWeight.Contains(input.Keyword) || u.EndTransitLocation.Contains(input.Keyword))
            .WhereIF(!string.IsNullOrWhiteSpace(input.SlotCode), u => u.SlotCode.Contains(input.SlotCode.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Make), u => u.Make.Contains(input.Make.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Property), u => u.Property.Contains(input.Property.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Handle), u => u.Handle.Contains(input.Handle.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Environment), u => u.Environment.Contains(input.Environment.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.Remark), u => u.Remark.Contains(input.Remark.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.SlotWeight), u => u.SlotWeight.Contains(input.SlotWeight.Trim()))
            .WhereIF(!string.IsNullOrWhiteSpace(input.EndTransitLocation), u => u.EndTransitLocation.Contains(input.EndTransitLocation.Trim()))
            .WhereIF(input.WarehouseId != null, u => u.WarehouseId == input.WarehouseId)
            .WhereIF(input.SlotAreaId != null, u => u.SlotAreaId == input.SlotAreaId)
            .WhereIF(input.SlotLanewayId != null, u => u.SlotLanewayId == input.SlotLanewayId)
            .WhereIF(input.SlotRow != null, u => u.SlotRow == input.SlotRow)
            .WhereIF(input.SlotColumn != null, u => u.SlotColumn == input.SlotColumn)
            .WhereIF(input.SlotLayer != null, u => u.SlotLayer == input.SlotLayer)
            .WhereIF(input.SlotInout != null, u => u.SlotInout == input.SlotInout)
            .WhereIF(input.SlotStatus != null, u => u.SlotStatus == input.SlotStatus)
            .WhereIF(input.SlotImlockFlag != null, u => u.SlotImlockFlag == input.SlotImlockFlag)
            .WhereIF(input.SlotExlockFlag != null, u => u.SlotExlockFlag == input.SlotExlockFlag)
            .WhereIF(input.SlotCloseFlag != null, u => u.SlotCloseFlag == input.SlotCloseFlag)
            .WhereIF(input.SlotHigh != null, u => u.SlotHigh == input.SlotHigh)
            .WhereIF(input.IsSxcLocation != null, u => u.IsSxcLocation == input.IsSxcLocation)
            .Select((u, warehouse, slotArea, slotLaneway) => new WmsBaseSlotOutput
            {
                Id = u.Id,
                WarehouseId = u.WarehouseId,
                WarehouseFkDisplayName = $"{warehouse.WarehouseName}",
                SlotAreaId = u.SlotAreaId,
                SlotAreaFkDisplayName = $"{slotArea.AreaCode}-{slotArea.AreaName}",
                SlotLanewayId = u.SlotLanewayId,
                SlotLanewayFkDisplayName = $"{slotLaneway.LanewayName}",
                SlotRow = u.SlotRow,
                SlotColumn = u.SlotColumn,
                SlotLayer = u.SlotLayer,
                SlotInout = u.SlotInout,
                SlotCode = u.SlotCode,
                SlotStatus = u.SlotStatus,
                Make = u.Make,
                Property = u.Property,
                Handle = u.Handle,
                Environment = u.Environment,
                SlotCodeIndex = u.SlotCodeIndex,
                SlotImlockFlag = u.SlotImlockFlag,
                SlotExlockFlag = u.SlotExlockFlag,
                SlotCloseFlag = u.SlotCloseFlag,
                Remark = u.Remark,
                SlotHigh = u.SlotHigh,
                SlotWeight = u.SlotWeight,
                EndTransitLocation = u.EndTransitLocation,
                IsSxcLocation = u.IsSxcLocation,
                CreateUserName = u.CreateUserName,
                UpdateUserName = u.UpdateUserName,
                CreateUserId = u.CreateUserId,
                UpdateUserId = u.UpdateUserId,
                CreateTime = u.CreateTime,
                UpdateTime = u.UpdateTime,
            });
		return await query.OrderBuilder(input,"u.").ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取储位管理详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取储位管理详情")]
    [ApiDescriptionSettings(Name = "Detail"), HttpGet]
    public async Task<WmsBaseSlot> Detail([FromQuery] QueryByIdWmsBaseSlotInput input)
    {
        return await _wmsBaseSlotRep.GetFirstAsync(u => u.Id == input.Id);
    }

    /// <summary>
    /// 获取储位管理详情 ℹ️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("储位图例获取当前储位具体信息")]
    [ApiDescriptionSettings(Name = "GetSlotDetail"), HttpGet]
    public async Task<List<WmsBaseSlotDto>> GetSlotDetail([FromQuery] QueryByIdWmsBaseSlotInput input)
    {
        // 获取字典数据（带缓存）
        var slotStatusDict = await GetDictData("SlotStatus");
        var slotInoutDict = await GetDictData("SlotInout");
        var makeDict = await GetDictData("Make");
        var propertyDict = await GetDictData("Property");
        var handleDict = await GetDictData("Handle");
        var environmentDict = await GetDictData("Environment");

        //点墙
        //var entity = _wmsBaseSlotRep.GetById(input.Id);
        //if (entity != null)
        //{
        //    if (entity.SlotStatus == 8)
        //    {
        //        entity.SlotStatus = 0;
        //        entity.Make = "02";
        //    }
        //    else if(entity.Make == "02")
        //    {
        //        entity.SlotStatus = 8;
        //        entity.Make = "01";
        //    }
        //    await _wmsBaseSlotRep.AsUpdateable(entity).ExecuteCommandAsync();
        //}

        // 查询主数据
        var list = await _wmsBaseSlotRep.AsQueryable()
            .LeftJoin<WmsStockTray>((u, tray) => u.SlotCode == tray.StockSlotCode)
            .LeftJoin<WmsBaseMaterial>((u, tray, material) => tray.MaterialId == material.Id.ToString() && !material.IsDelete)
            .LeftJoin<WmsBaseLaneway>((u, tray, material, laneway) => u.SlotLanewayId == laneway.Id)
            .Where((u, tray, material, laneway) => u.Id == input.Id)
            .Select((u, tray, material, laneway) => new WmsBaseSlotDto()
            {
                BatchNo = tray.LotNo,
                CreateTime = u.CreateTime,
                CreateUserId = u.CreateUserId,
                CreateUserName = u.CreateUserName,
                EndTransitLocation = u.EndTransitLocation,
                Environment = u.Environment,
                Id = u.Id,
                Make = u.Make,
                MaterialName = $"{material.MaterialCode} {material.MaterialName}",
                Property = u.Property,
                Remark = u.Remark,
                SlotAreaId = u.SlotAreaId,
                Handle = u.Handle,
                SlotCode = u.SlotCode,
                SlotCodeIndex = u.SlotCodeIndex,
                SlotCloseFlag = u.SlotCloseFlag,
                SlotExlockFlag = u.SlotExlockFlag,
                SlotImlockFlag = u.SlotImlockFlag,
                SlotInout = u.SlotInout,
                SlotLanewayId = u.SlotLanewayId,
                SlotStatus = u.SlotStatus,
                SlotWeight = u.SlotWeight,
                WarehouseId = u.WarehouseId,
                IsSxcLocation = u.IsSxcLocation,
                SlotColumn = u.SlotColumn,
                SlotHigh = u.SlotHigh,
                SlotLayer = u.SlotLayer,
                SlotRow = u.SlotRow,
                StockCode = tray.StockCode,
                UpdateTime = u.UpdateTime,
                UpdateUserId = u.UpdateUserId,
                Weight = tray.StockQuantity,
                SlotLanewayIdFkColumn = laneway.LanewayName
            })
            .ToListAsync();
        foreach (var detail in list)
        {
            // 设置字典显示名称
            detail.SlotStatusName = GetDictLabel(slotStatusDict, detail.SlotStatus?.ToString());
            detail.SlotInoutName = GetDictLabel(slotInoutDict, detail.SlotInout?.ToString());
            detail.MakeName = GetDictLabel(makeDict, detail.Make);
            detail.PropertyName = GetDictLabel(propertyDict, detail.Property);
            detail.HandleName = GetDictLabel(handleDict, detail.Handle);
            detail.EnvironmentName = GetDictLabel(environmentDict, detail.Environment);
        }
        return list;
    }

    // 获取字典数据（带缓存）
    private async Task<Dictionary<string, string>> GetDictData(string dictCode)
    {
        var cacheKey = $"{CacheConst.KeyDict}{dictCode}";
        var dictData = _sysCacheService.Get<Dictionary<string, string>>(cacheKey);

        if (dictData == null)
        {
            var data = await _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = dictCode });
            dictData = data.ToDictionary(x => x.Value, x => x.Label);
            _sysCacheService.Set(cacheKey, dictData);
        }

        return dictData;
    }

    // 获取字典标签
    private string GetDictLabel(Dictionary<string, string> dict, string value)
    {
        return value != null && dict.ContainsKey(value) ? dict[value] : "";
    }

    /// <summary>
    /// 增加储位管理 ➕
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("增加储位管理")]
    [ApiDescriptionSettings(Name = "Add"), HttpPost]
    public async Task<long> Add(AddWmsBaseSlotInput input)
    {
        var entity = input.Adapt<WmsBaseSlot>();
        return await _wmsBaseSlotRep.InsertAsync(entity) ? entity.Id : 0;
    }

    /// <summary>
    /// 更新储位管理 ✏️
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("更新储位管理")]
    [ApiDescriptionSettings(Name = "Update"), HttpPost]
    public async Task Update(UpdateWmsBaseSlotInput input)
    {
        var entity = input.Adapt<WmsBaseSlot>();
        await _wmsBaseSlotRep.AsUpdateable(entity)
        .IgnoreColumns(u => new {
            u.SlotCodeIndex,
            u.Remark,
            u.SlotHigh,
            u.SlotWeight,
            u.EndTransitLocation,
            u.IsSxcLocation,
        })
        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除储位管理 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("删除储位管理")]
    [ApiDescriptionSettings(Name = "Delete"), HttpPost]
    public async Task Delete(DeleteWmsBaseSlotInput input)
    {
        var entity = await _wmsBaseSlotRep.GetFirstAsync(u => u.Id == input.Id) ?? throw Oops.Oh(ErrorCodeEnum.D1002);
        await _wmsBaseSlotRep.DeleteAsync(entity);   //真删除
    }

    /// <summary>
    /// 批量删除储位管理 ❌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("批量删除储位管理")]
    [ApiDescriptionSettings(Name = "BatchDelete"), HttpPost]
    public async Task<bool> BatchDelete([Required(ErrorMessage = "主键列表不能为空")]List<DeleteWmsBaseSlotInput> input)
    {
        var exp = Expressionable.Create<WmsBaseSlot>();
        foreach (var row in input) exp = exp.Or(it => it.Id == row.Id);
        var list = await _wmsBaseSlotRep.AsQueryable().Where(exp.ToExpression()).ToListAsync();

        return await _wmsBaseSlotRep.DeleteAsync(list);   //真删除
    }
    
    /// <summary>
    /// 获取下拉列表数据 🔖
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取下拉列表数据")]
    [ApiDescriptionSettings(Name = "DropdownData"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownData(DropdownDataWmsBaseSlotInput input)
    {
        var warehouseIdData = await _wmsBaseSlotRep.Context.Queryable<WmsBaseWareHouse>()
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.WarehouseName}",
                Type = u.WarehouseType
            }).ToListAsync();
        var slotAreaIdData = await _wmsBaseSlotRep.Context.Queryable<WmsBaseArea>()
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.AreaCode}-{u.AreaName}"
            }).ToListAsync();
        var slotLanewayIdData = await _wmsBaseSlotRep.Context.Queryable<WmsBaseLaneway>()
            .Select(u => new {
                Value = u.Id,
                Label = $"{u.LanewayCode}-{u.LanewayName}"
            }).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "warehouseId", warehouseIdData },
            { "slotAreaId", slotAreaIdData },
            { "slotLanewayId", slotLanewayIdData },
        };
    }

    /// <summary>
    /// 获取储位图例
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取储位图例")]
    [ApiDescriptionSettings(Name = "GetSlotLegendList"), HttpPost]
    public async Task<dynamic> GetSlotGridData(WmsBaseSlotBaseQueryInput input)
    {
        List<WmsBaseSlot> slotList;
        // 获取当前仓库详细信息
        var warehouseInfo = _wmsBaseWareHouseRep.GetById(input.WarehouseId);
        // 立库（ID取自字典，字典ID不变）
        if (warehouseInfo.WarehouseType == "01")
        {
            var slotStatusDictMap = _sysCacheService.Get<List<SysDictData>>($"{CacheConst.KeyDict}SlotStatus");
            if (slotStatusDictMap == null)
            {
                slotStatusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "SlotStatus" }).Result.ToList();
                _sysCacheService.Set($"{CacheConst.KeyDict}SlotStatus", slotStatusDictMap);
            }
            //按层展示
            if (input.ShowType == "layer")
            {
                slotList = await _wmsBaseSlotRep.AsQueryable().Where(
                    u => u.WarehouseId == input.WarehouseId
                    && u.SlotLayer == input.SlotLayer
                ).ToListAsync();
                // 进行分组汇总 获得统计数据
                var statusList = slotList.GroupBy(u => u.SlotStatus);
                return GroupCount(input, slotList, slotStatusDictMap, statusList);

            }
            // 按排展示
            else if (input.ShowType == "row")
            {
                slotList = await _wmsBaseSlotRep.AsQueryable().Where(
                    u => u.WarehouseId == input.WarehouseId
                    && u.SlotLanewayId == input.SlotLanewayId
                    && u.SlotRow == input.SlotRow
                    && u.SlotInout == input.SlotInout
                ).ToListAsync();

                // 进行分组汇总 获得统计数据
                var statusList = slotList.GroupBy(u => u.SlotStatus);
                return GroupCount(input, slotList, slotStatusDictMap, statusList, false);
            }
            else
            {
                return new SlotGridRes();
            }
        }
        // 平库
        else if (warehouseInfo.WarehouseType == "05")
        {
            // 平库只有 针对层的筛选
            slotList = await _wmsBaseSlotRep.AsQueryable().Where(
                    u => u.WarehouseId == input.WarehouseId
                    && u.SlotLayer == input.SlotLayer
                ).ToListAsync();
            FlatSlotGridRes data = new FlatSlotGridRes();
            // 设置最大行和列

            int rowMax = Convert.ToInt32(slotList.Select(m => m.SlotRow).DefaultIfEmpty(0).Max());
            int colMax = Convert.ToInt32(slotList.Select(m => m.SlotColumn).DefaultIfEmpty(0).Max());
            data.Row = rowMax;
            data.Col = colMax;
            // 排序后返回集合
            data.Data = slotList.OrderByDescending(m => m.SlotCode).ToList();

            return data;
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// 获取载具状态分组情况
    /// 柱状图.饼图
    /// </summary>
    /// <param name="input"></param>
    /// <param name="slotList"></param>
    /// <param name="Dic"></param>
    /// <param name="statusList"></param>
    /// <param name="isLayer">是否层</param>
    /// <returns></returns>
    private SlotGridRes GroupCount(WmsBaseSlotBaseQueryInput input, List<WmsBaseSlot> slotList, List<SysDictData> Dic, IEnumerable<IGrouping<int?, WmsBaseSlot>> statusList,bool isLayer = true)
    {
        // 柱状图数据结构
        var chartData = new { categories = new List<string>(), values = new List<int>() ,color = new List<string>()};
        // 饼图数据结构
        var pieData = new List<dynamic>();
        // 构建饼图数据
        foreach (var item in statusList)
        {
            var dic = Dic.FirstOrDefault(m=>m.Value==item.Key.ToString());
            if (dic != null)
            {
                // 饼图增加数据
                pieData.Add(new { name = dic.Label, value = item.Count(),color = dic.StyleSetting });
                // 柱状图增加数据
                chartData.categories.Add(dic.Label);
                chartData.values.Add(item.Count());
                chartData.color.Add(dic.StyleSetting);
            }
        }
        // 补全其他数据类型 - 处理值为0的项
        foreach (var item in Dic)
        {
            // 检查这个名称是否已经在饼图数据中存在
            var exists = pieData.FirstOrDefault(m => m.name == item.Label);

            if (exists == null)
            {
                // 如果不存在，添加值为0的数据 (补齐状态)
                pieData.Add(new { name = item.Label, value = 0,color = item.StyleSetting });
                chartData.categories.Add(item.Label);
                chartData.values.Add(0);
                chartData.color.add(item.StyleSetting);
            }
        }

        var data = isLayer ? BuildLayerGrid(slotList, input.SlotLayer) : BuildRowGrid(slotList,input.SlotLanewayId,input.SlotRow);
        return new SlotGridRes()
        {
            ChartData = chartData,
            PieData = pieData,
            Data = data,
            SlotStatusList = Dic
        };
    }

    // 按层构建网格 - 按巷道、排和深度组织
    private List<List<SlotGridItem>> BuildLayerGrid(List<WmsBaseSlot> slotList, int? layer)
    {
        // 按巷道、排和深度分组
        var lanewayGroups = slotList
            .Where(s => s.SlotLanewayId.HasValue && s.SlotRow.HasValue && s.SlotInout.HasValue)
            .GroupBy(s => s.SlotLanewayId.Value)
            .OrderByDescending(g => g.Key)
            .ToList();

        var grid = new List<List<SlotGridItem>>();
        // 深度颠倒,前两排深度按照 2,1,后两排按照 1,2
        bool isRevert = true;
        foreach (var lanewayGroup in lanewayGroups)
        {
            // 获取该巷道的所有排
            var rowGroups = lanewayGroup
                .GroupBy(s => s.SlotRow.Value)
                .OrderByDescending(g => g.Key)
                .ToList();
            foreach (var rowGroup in rowGroups)
            {
                List<IGrouping<int, WmsBaseSlot>> depthGroups = rowGroup
                    .GroupBy(s => s.SlotInout.Value)
                    .OrderBy(g => isRevert ? -g.Key : g.Key)
                    .ToList();

                isRevert = !isRevert;

                foreach (var depthGroup in depthGroups)
                {
                    var rowItems = new List<SlotGridItem>();
                    var columnsInDepth = depthGroup.OrderBy(s => s.SlotColumn.Value).ToList();

                    // 计算该深度下的最大列数
                    var maxCol = columnsInDepth.Any() ? columnsInDepth.Max(s => s.SlotColumn.Value) : 0;

                    for (int col = maxCol; col >= 0; col--)
                    {
                        if (rowItems.Count == maxCol)
                        {
                            break;
                        }
                        var slot = columnsInDepth.FirstOrDefault(s => s.SlotColumn == col);
                        rowItems.Add(new SlotGridItem
                        {
                            Id = slot?.Id ?? 0,
                            Status = slot?.SlotStatus ?? 8,
                        });
                    }

                    if (rowItems.Any())
                        grid.Add(rowItems);
                }
            }
        }

        return grid;
    }

    // 按排构建网格 - 按层组织行
    private List<List<SlotGridItem>> BuildRowGrid(List<WmsBaseSlot> slotList, long? lanewayId, int? row)
    {
        var grid = new List<List<SlotGridItem>>();

        if (!slotList.Any())
            return grid;

        // 按层分组
        var layerGroups = slotList
            .Where(s => s.SlotLayer.HasValue)
            .GroupBy(s => s.SlotLayer.Value)
            .OrderByDescending(g => g.Key)
            .ToList();

        foreach (var layerGroup in layerGroups)
        {
            var rowItems = new List<SlotGridItem>();

            // 按列排序
            var sortedSlots = layerGroup
                .Where(s => s.SlotColumn.HasValue)
                .OrderByDescending(s => s.SlotColumn.Value)
                .ToList();

            // 计算该层的最大列数
            var maxCol = sortedSlots.Any() ? sortedSlots.Max(s => s.SlotColumn.Value) : 0;

            for (int col = maxCol; col >= 0; col--)
            {
                var slot = sortedSlots.FirstOrDefault(s => s.SlotColumn == col);

                if (rowItems.Count == maxCol)
                {
                    break;
                }
                rowItems.Add(new SlotGridItem
                {
                    Id = slot?.Id ?? 0,
                    Status = slot?.SlotStatus ?? 8,
                });
            }

            if (rowItems.Any())
                grid.Add(rowItems);
        }

        return grid;
    }

    /// <summary>
    /// 获取根据仓库储位获取层数
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取根据仓库储位获取层数")]
    [ApiDescriptionSettings(Name = "DropdownDataLayer"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownDataLayer(DropdownDataWmsBaseSlotInput input)
    {
        var layerData = await _wmsBaseSlotRep.Context.Queryable<WmsBaseSlot>().Where(u => u.WarehouseId == input.WarehouseId).GroupBy(u => u.SlotLayer)
            .Select(u => new
            {
                Value = u.SlotLayer,
                Label = $"{u.SlotLayer}层"
            }).OrderBy(u => u.Value).ToListAsync();
        return new Dictionary<string, dynamic> 
        {
            { "layerData" , layerData }
        };
    }

    /// <summary>
    /// 获取根据巷道获取 排 数据
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取根据巷道获取 排 数据")]
    [ApiDescriptionSettings(Name = "DropdownDataRow"), HttpPost]
    public async Task<Dictionary<string, dynamic>> DropdownDataRow(DropdownDataWmsBaseSlotInput input)
    {
        var rowData = await _wmsBaseSlotRep.Context.Queryable<WmsBaseSlot>().Where(u => u.WarehouseId == input.WarehouseId && u.SlotLanewayId == input.SlotLanewayId).GroupBy(u => u.SlotRow)
            .Select(u => new
            {
                Value = u.SlotRow,
                Label = $"{u.SlotRow}排"
            }).OrderBy(u => u.Value).ToListAsync();
        return new Dictionary<string, dynamic>
        {
            { "rowData" , rowData }
        };
    }

    /// <summary>
    /// 导出储位管理记录 🔖
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("导出储位管理记录")]
    [ApiDescriptionSettings(Name = "Export"), HttpPost, NonUnify]
    public async Task<IActionResult> Export(PageWmsBaseSlotInput input)
    {
        var list = (await Page(input)).Items?.Adapt<List<ExportWmsBaseSlotOutput>>() ?? new();
        if (input.SelectKeyList?.Count > 0) list = list.Where(x => input.SelectKeyList.Contains(x.Id)).ToList();
        var slotInoutDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "SlotInout" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var slotStatusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "SlotStatus" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var makeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Make" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var propertyDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Property" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var handleDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Handle" }).Result.ToDictionary(x => x.Value, x => x.Label);
        var environmentDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Environment" }).Result.ToDictionary(x => x.Value, x => x.Label);
        list.ForEach(e => {
            e.SlotInoutDictLabel = slotInoutDictMap.GetValueOrDefault(e.SlotInout.ToString() ?? "", e.SlotInout.ToString());
            e.SlotStatusDictLabel = slotStatusDictMap.GetValueOrDefault(e.SlotStatus.ToString() ?? "", e.SlotStatus.ToString());
            e.MakeDictLabel = makeDictMap.GetValueOrDefault(e.Make ?? "", e.Make);
            e.PropertyDictLabel = propertyDictMap.GetValueOrDefault(e.Property ?? "", e.Property);
            e.HandleDictLabel = handleDictMap.GetValueOrDefault(e.Handle ?? "", e.Handle);
            e.EnvironmentDictLabel = environmentDictMap.GetValueOrDefault(e.Environment ?? "", e.Environment);
        });
        return ExcelHelper.ExportTemplate(list, "储位管理导出记录");
    }
    
    /// <summary>
    /// 下载储位管理数据导入模板 ⬇️
    /// </summary>
    /// <returns></returns>
    [DisplayName("下载储位管理数据导入模板")]
    [ApiDescriptionSettings(Name = "Import"), HttpGet, NonUnify]
    public IActionResult DownloadTemplate()
    {
        return ExcelHelper.ExportTemplate(new List<ExportWmsBaseSlotOutput>(), "储位管理导入模板", (_, info) =>
        {
            if (nameof(ExportWmsBaseSlotOutput.WarehouseFkDisplayName) == info.Name) return _wmsBaseSlotRep.Context.Queryable<WmsBaseWareHouse>().Select(u => $"{u.WarehouseName}").Distinct().ToList();
            if (nameof(ExportWmsBaseSlotOutput.SlotAreaFkDisplayName) == info.Name) return _wmsBaseSlotRep.Context.Queryable<WmsBaseArea>().Select(u => $"{u.AreaCode}-{u.AreaName}").Distinct().ToList();
            if (nameof(ExportWmsBaseSlotOutput.SlotLanewayFkDisplayName) == info.Name) return _wmsBaseSlotRep.Context.Queryable<WmsBaseLaneway>().Select(u => $"{u.LanewayCode}-{u.LanewayName}").Distinct().ToList();
            return null;
        });
    }
    
    private static readonly object _wmsBaseSlotImportLock = new object();
    /// <summary>
    /// 导入储位管理记录 💾
    /// </summary>
    /// <returns></returns>
    [DisplayName("导入储位管理记录")]
    [ApiDescriptionSettings(Name = "Import"), HttpPost, NonUnify, UnitOfWork]
    public IActionResult ImportData([Required] IFormFile file)
    {
        lock (_wmsBaseSlotImportLock)
        {
            var slotInoutDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "SlotInout" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var slotStatusDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "SlotStatus" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var makeDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Make" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var propertyDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Property" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var handleDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Handle" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var environmentDictMap = _sysDictTypeService.GetDataList(new GetDataDictTypeInput { Code = "Environment" }).Result.ToDictionary(x => x.Label!, x => x.Value);
            var stream = ExcelHelper.ImportData<ImportWmsBaseSlotInput, WmsBaseSlot>(file, (list, markerErrorAction) =>
            {
                _sqlSugarClient.Utilities.PageEach(list, 2048, pageItems =>
                {
                    // 链接 所属仓库
                    var warehouseIdLabelList = pageItems.Where(x => x.WarehouseFkDisplayName != null).Select(x => x.WarehouseFkDisplayName).Distinct().ToList();
                    if (warehouseIdLabelList.Any()) {
                        var warehouseIdLinkMap = _wmsBaseSlotRep.Context.Queryable<WmsBaseWareHouse>().Where(u => warehouseIdLabelList.Contains($"{u.WarehouseName}")).ToList().ToDictionary(u => $"{u.WarehouseName}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.WarehouseId = warehouseIdLinkMap.GetValueOrDefault(e.WarehouseFkDisplayName ?? "");
                            if (e.WarehouseId == null) e.Error = "所属仓库链接失败";
                        });
                    }
                    // 链接 所属区域
                    var slotAreaIdLabelList = pageItems.Where(x => x.SlotAreaFkDisplayName != null).Select(x => x.SlotAreaFkDisplayName).Distinct().ToList();
                    if (slotAreaIdLabelList.Any()) {
                        var slotAreaIdLinkMap = _wmsBaseSlotRep.Context.Queryable<WmsBaseArea>().Where(u => slotAreaIdLabelList.Contains($"{u.AreaCode}-{u.AreaName}")).ToList().ToDictionary(u => $"{u.AreaCode}-{u.AreaName}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.SlotAreaId = slotAreaIdLinkMap.GetValueOrDefault(e.SlotAreaFkDisplayName ?? "");
                            if (e.SlotAreaId == null) e.Error = "所属区域链接失败";
                        });
                    }
                    // 链接 所属巷道
                    var slotLanewayIdLabelList = pageItems.Where(x => x.SlotLanewayFkDisplayName != null).Select(x => x.SlotLanewayFkDisplayName).Distinct().ToList();
                    if (slotLanewayIdLabelList.Any()) {
                        var slotLanewayIdLinkMap = _wmsBaseSlotRep.Context.Queryable<WmsBaseLaneway>().Where(u => slotLanewayIdLabelList.Contains($"{u.LanewayCode}-{u.LanewayName}")).ToList().ToDictionary(u => $"{u.LanewayCode}-{u.LanewayName}", u => u.Id  as long?);
                        pageItems.ForEach(e => {
                            e.SlotLanewayId = slotLanewayIdLinkMap.GetValueOrDefault(e.SlotLanewayFkDisplayName ?? "");
                            if (e.SlotLanewayId == null) e.Error = "所属巷道链接失败";
                        });
                    }
                    
                    // 映射字典值
                    foreach(var item in pageItems) {
                        System.Text.StringBuilder sbError = new System.Text.StringBuilder();
                        if (!string.IsNullOrWhiteSpace(item.SlotInoutDictLabel)) {
                            item.SlotInout = Convert.ToInt32(slotInoutDictMap.GetValueOrDefault(item.SlotInoutDictLabel));
                            if (item.SlotInout == null) sbError.AppendLine("储位深度字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.SlotStatusDictLabel)) {
                            item.SlotStatus = Convert.ToInt32(slotStatusDictMap.GetValueOrDefault(item.SlotStatusDictLabel));
                            if (item.SlotStatus == null) sbError.AppendLine("储位状态字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.MakeDictLabel)) {
                            item.Make = makeDictMap.GetValueOrDefault(item.MakeDictLabel);
                            if (item.Make == null) sbError.AppendLine("库位类型字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.PropertyDictLabel)) {
                            item.Property = propertyDictMap.GetValueOrDefault(item.PropertyDictLabel);
                            if (item.Property == null) sbError.AppendLine("储位属性字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.HandleDictLabel)) {
                            item.Handle = handleDictMap.GetValueOrDefault(item.HandleDictLabel);
                            if (item.Handle == null) sbError.AppendLine("储位处理字典映射失败");
                        }
                        if (!string.IsNullOrWhiteSpace(item.EnvironmentDictLabel)) {
                            item.Environment = environmentDictMap.GetValueOrDefault(item.EnvironmentDictLabel);
                            if (item.Environment == null) sbError.AppendLine("储位环境字典映射失败");
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
                        if (x.SlotAreaId == null){
                            x.Error = "所属区域不能为空";
                            return false;
                        }
                        if (x.SlotLanewayId == null){
                            x.Error = "所属巷道不能为空";
                            return false;
                        }
                        return true;
                    }).Adapt<List<WmsBaseSlot>>();
                    
                    var storageable = _wmsBaseSlotRep.Context.Storageable(rows)
                        .SplitError(it => it.Item.WarehouseId == null, "所属仓库不能为空")
                        .SplitError(it => it.Item.SlotAreaId == null, "所属区域不能为空")
                        .SplitError(it => it.Item.SlotLanewayId == null, "所属巷道不能为空")
                        .SplitError(it => it.Item.SlotCode?.Length > 20, "储位编码长度不能超过20个字符")
                        .SplitError(it => it.Item.Make?.Length > 10, "库位类型长度不能超过10个字符")
                        .SplitError(it => it.Item.Property?.Length > 10, "储位属性长度不能超过10个字符")
                        .SplitError(it => it.Item.Handle?.Length > 10, "储位处理长度不能超过10个字符")
                        .SplitError(it => it.Item.Environment?.Length > 10, "储位环境长度不能超过10个字符")
                        .SplitInsert(_=> true) // 没有设置唯一键代表插入所有数据
                        .ToStorage();
                    
                    storageable.AsInsertable.ExecuteCommand();// 不存在插入
                    storageable.AsUpdateable.UpdateColumns(it => new
                    {
                        it.WarehouseId,
                        it.SlotAreaId,
                        it.SlotLanewayId,
                        it.SlotRow,
                        it.SlotColumn,
                        it.SlotLayer,
                        it.SlotInout,
                        it.SlotCode,
                        it.SlotStatus,
                        it.Make,
                        it.Property,
                        it.Handle,
                        it.Environment,
                        it.SlotImlockFlag,
                        it.SlotExlockFlag,
                        it.SlotCloseFlag,
                    }).ExecuteCommand();// 存在更新
                    
                    // 标记错误信息
                    markerErrorAction.Invoke(storageable, pageItems, rows);
                });
            });
            
            return stream;
        }
    }
}
