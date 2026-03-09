# Docker 部署

本目录已经整理为适合云服务器直接部署的版本，不再依赖宿主机先编译前后端产物。

## 服务组成

- `frontend`: Vue 3 前端，构建后由 Nginx 提供静态资源和反向代理
- `backend`: Admin.NET 后端，容器内直接 `dotnet publish`
- `mysql`: MySQL 8
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

- `MYSQL_ROOT_PASSWORD`
- `MYSQL_DATABASE`
- `REDIS_PASSWORD`
- `HTTP_PORT`
- `BACKEND_PORT`

3. 如果你修改了数据库或 Redis 密码，需要同步修改这些文件中的固定连接配置：

- `docker/app/Configuration/Database.json`
- `docker/app/Configuration/Cache.json`

4. 启动

```bash
docker compose up -d --build
```

## 访问地址

- 前端：`http://你的服务器IP:${HTTP_PORT}`，默认 `9100`
- 后端直连：`http://你的服务器IP:${BACKEND_PORT}`，默认 `9102`
- MySQL：宿主机 `9101`
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

- `docker/mysql/data`
- `docker/redis/data`
- `docker/data/backend/upload`
- `docker/data/backend/avatar`
- `docker/data/backend/signature`
- `docker/data/backend/codeGen`
- `docker/data/backend/logs`

## 阿里云部署建议

- ECS 安全组放行 `9100`、`9101`、`9102`
- 如果只想对外暴露前端，`9101` 和 `9102` 不要开放公网
- 正式环境建议在阿里云 SLB / Nginx / 宝塔上额外做 HTTPS
- 首次启动会自动初始化数据库表和种子数据，时间会比平时稍长
