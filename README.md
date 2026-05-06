# Admin.NET WMS 仓储管理系统

基于 [Admin.NET](https://gitee.com/zuohuaijun/Admin.NET) 通用权限框架构建的企业级 WMS（Warehouse Management System）仓储管理系统，覆盖入库、出库、调拨、盘点、库位管理等全链路仓储业务流程，支持 PC 端后台管理与 PDA 移动端扫码作业。

## 功能模块

### 基础数据管理
- **仓库管理** — 多仓库、多库区、巷道、库位层级维护
- **物料管理** — 物料编码、规格、单位、批次/效期属性
- **客户/供应商管理** — 客户预设、供应商预设，支持出入库单据关联
- **单据类型** — 灵活定义入库/出库/移库/盘点等业务单据类型

### 入库管理
- 入库通知 → 入库单 → 入库任务 → 扫码上架
- 支持采购入库、生产入库、退货入库等多种入库类型
- 入库标签打印、质检状态联动

### 出库管理
- 出库通知 → 出库单 → 出库任务 → 波次拣货
- 支持销售出库、生产领料、退货出库等出库类型
- 先进先出（FIFO）/ 指定批次出库策略
- 箱码管理、发货单生成

### 库存管理
- 实时库存查询（按仓库/库位/物料/批次多维度）
- 库存调整、安全库存预警
- 托盘库存、库位库存、箱库存分层管理

### 移库管理
- 移库通知 → 移库单 → 移库任务
- 库位转移、库存冻结/释放

### 盘点管理
- 盘点通知 → 盘点任务 → 差异处理
- 明盘/盲盘、复盘、差异审核

### PDA 移动端
- 扫码收货、上架、拣货、盘点、库存查询
- 适配工业 PDA 设备（Android）
- 离线缓存与数据同步

### 统计报表
- 出入库汇总、库存周转、库龄分析
- 操作日志、库存流水追溯
- Dashboard 仪表盘

## 技术栈

| 层级 | 技术 |
|------|------|
| 后端框架 | .NET 8/9、Furion |
| ORM | SqlSugar |
| 前端 | Vue 3 + TypeScript + Element Plus + Vite |
| 移动端 (PDA) | Layui + JavaScript |
| 数据库 | SQL Server（可切换 MySQL 等） |
| 缓存 | Redis |
| 部署 | Docker、Windows Server / Linux |

## 项目结构

```
admin.net-wms/
├── Admin.NET/                  # 后端服务
│   └── Admin.NET.Application/  # WMS 业务应用层
│       ├── Entity/             # 数据实体（Wms*）
│       ├── Service/            # 业务服务
│       │   ├── BaseService/    # 基础数据服务
│       │   ├── WmsImport*/     # 入库相关
│       │   ├── WmsExport*/     # 出库相关
│       │   ├── WmsStock*/      # 库存相关
│       │   ├── WmsMove*/       # 移库相关
│       │   ├── WmsStockCheck*/ # 盘点相关
│       │   ├── WmsPda/         # PDA 服务
│       │   ├── WmsDashboard/   # 仪表盘
│       │   └── WmsStatisticalReport/  # 统计报表
│       └── Const/              # 常量定义
├── Web/                        # PC 前端 (Vue 3)
├── PDA/                        # PDA 移动端 (Layui)
├── vite-project/               # Vite 前端工程
└── docker/                     # Docker 部署配置
```

## 快速开始

### 环境要求
- .NET 8.0+
- Node.js 18+
- SQL Server（或 MySQL）
- Redis

### 后端运行

```bash
cd Admin.NET/Admin.NET.Web.Core
# 修改 appsettings.json 中的数据库连接字符串
dotnet run
```

### 前端运行

```bash
cd Web
pnpm install
pnpm run dev
```

### Docker 部署

```bash
# 参考 docker/README.md
docker compose up -d
```

## 仓库地址

- GitHub：[https://github.com/slnlkd/admin.net-wms](https://github.com/slnlkd/admin.net-wms)

## 致谢

本项目基于以下开源项目构建：

- [Admin.NET](https://gitee.com/zuohuaijun/Admin.NET) — .NET 通用权限开发框架
- [Furion](https://gitee.com/dotnetchina/Furion) — .NET 应用框架
- [SqlSugar](https://gitee.com/dotnetchina/SqlSugar) — ORM 框架
- [vue-next-admin](https://lyt-top.gitee.io/vue-next-admin-doc-preview/) — 前端模板
- [Element Plus](https://element-plus.org/) — Vue 3 UI 组件库

## 许可证

本项目基于 Admin.NET 二次开发，遵循 MIT 许可证。
