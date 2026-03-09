# Docker 部署

本目录已经整理为适合云服务器直接部署的版本，不再依赖宿主机先编译前后端产物。

当前默认部署方式：

- 宿主机已安装 `MariaDB/MySQL`
- Docker 中仅运行 `frontend`、`backend`、`redis`

## 服务组成

- `frontend`: Vue 3 前端，构建后由 Nginx 提供静态资源和反向代理
- `backend`: Admin.NET 后端，容器内直接 `dotnet publish`
- `redis`: Redis 7

## 首次部署

1. 进入目录

```bash
cd docker
```

2. 按需修改环境变量

```bash
cp .env.example .env
```

重点变量：

- `MARIADB_HOST`
- `MARIADB_PORT`
- `MARIADB_DATABASE`
- `MARIADB_USER`
- `MARIADB_PASSWORD`
- `REDIS_PASSWORD`
- `HTTP_PORT`
- `BACKEND_PORT`

3. `docker/app/Configuration/Database.json` 和 `docker/app/Configuration/Cache.json` 已改成模板，容器启动时会自动套用 `.env` 中的数据库和 Redis 参数。

4. 启动

```bash
docker compose up -d --build
```

## 访问地址

- 前端：`http://你的服务器IP:${HTTP_PORT}`，默认 `9100`
- 后端直连：`http://你的服务器IP:${BACKEND_PORT}`，默认 `9102`
- Redis：宿主机 `6379`

## 常用命令

```bash
docker compose ps
docker compose logs -f backend
docker compose logs -f frontend
docker compose down
docker compose up -d --build
```

## 数据持久化目录

- `docker/redis/data`
- `docker/data/backend/upload`
- `docker/data/backend/avatar`
- `docker/data/backend/signature`
- `docker/data/backend/codeGen`
- `docker/data/backend/logs`

## 阿里云部署建议

- ECS 安全组放行 `9100`、`9102`
- 如果只想对外暴露前端，`9102` 不要开放公网
- 正式环境建议在阿里云 SLB / Nginx / 宝塔上额外做 HTTPS
- 首次启动会自动初始化数据库表和种子数据，时间会比平时稍长
- 当前默认示例直接连接外部 MariaDB 地址；生产密码请只保存在服务器 `.env`，不要提交到公开仓库
