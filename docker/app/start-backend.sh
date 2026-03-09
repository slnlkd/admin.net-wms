#!/usr/bin/env bash
set -euo pipefail

MARIADB_HOST="${MARIADB_HOST:-8.155.168.138}"
MARIADB_PORT="${MARIADB_PORT:-3306}"
MARIADB_DATABASE="${MARIADB_DATABASE:-jc44}"
MARIADB_USER="${MARIADB_USER:-app_user}"
MARIADB_PASSWORD="${MARIADB_PASSWORD:-CHANGE_ME}"
REDIS_PASSWORD="${REDIS_PASSWORD:-AdminNetRedis@2026}"

mkdir -p /app/Configuration
cp -f /config-template/*.json /app/Configuration/

sed -i "s|__MARIADB_HOST__|${MARIADB_HOST}|g" /app/Configuration/Database.json
sed -i "s|__MARIADB_PORT__|${MARIADB_PORT}|g" /app/Configuration/Database.json
sed -i "s|__MARIADB_DATABASE__|${MARIADB_DATABASE}|g" /app/Configuration/Database.json
sed -i "s|__MARIADB_USER__|${MARIADB_USER}|g" /app/Configuration/Database.json
sed -i "s|__MARIADB_PASSWORD__|${MARIADB_PASSWORD}|g" /app/Configuration/Database.json

sed -i "s|__REDIS_PASSWORD__|${REDIS_PASSWORD}|g" /app/Configuration/Cache.json

/bin/bash /app/wait-for-it.sh "${MARIADB_HOST}:${MARIADB_PORT}" -t 180 -- \
  /bin/bash /app/wait-for-it.sh redis:6379 -t 180 -- \
  dotnet Admin.NET.Web.Entry.dll
