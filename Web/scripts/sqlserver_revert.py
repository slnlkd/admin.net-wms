import pyodbc
import logging
from datetime import datetime
import time

# 配置日志
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

class SnowflakeIDGenerator:
    """遵循Admin.Net规则的雪花ID生成器"""
    def __init__(self, datacenter_id=1, worker_id=1):
        # Admin.Net通常使用2020-01-01作为起始时间戳
        self.epoch = 1577808000000  # 2020-01-01 00:00:00
        self.datacenter_id = datacenter_id
        self.worker_id = worker_id
        self.sequence = 0
        self.last_timestamp = -1

    def generate_id(self):
        """生成雪花ID - 遵循Admin.Net规则"""
        timestamp = int(time.time() * 1000)
        
        if timestamp < self.last_timestamp:
            raise Exception("时钟回拨异常")
        
        if timestamp == self.last_timestamp:
            self.sequence = (self.sequence + 1) & 4095  # 12位序列号
            if self.sequence == 0:
                timestamp = self.wait_next_millis(self.last_timestamp)
        else:
            self.sequence = 0
            
        self.last_timestamp = timestamp
        
        # Admin.Net通常的位分配：时间戳(41位) + 数据中心(5位) + 工作机器(5位) + 序列号(12位)
        return ((timestamp - self.epoch) << 22) | (self.datacenter_id << 17) | (self.worker_id << 12) | self.sequence
    
    def wait_next_millis(self, last_timestamp):
        """等待下一毫秒"""
        timestamp = int(time.time() * 1000)
        while timestamp <= last_timestamp:
            timestamp = int(time.time() * 1000)
        return timestamp

class SQLServerIDConverter:
    def __init__(self, connection_string):
        self.connection_string = connection_string
        self.snowflake_generator = SnowflakeIDGenerator()
        self.conn = None
        self.user_mapping = {}  # 存储用户名到雪花ID的映射
        
    def connect(self):
        """连接数据库"""
        try:
            self.conn = pyodbc.connect(self.connection_string)
            logger.info("数据库连接成功")
            return True
        except Exception as e:
            logger.error(f"数据库连接失败: {e}")
            return False
    
    def disconnect(self):
        """断开数据库连接"""
        if self.conn:
            self.conn.close()
            logger.info("数据库连接已关闭")
    
    def check_table_exists(self, table_name):
        """检查表是否存在"""
        cursor = self.conn.cursor()
        try:
            cursor.execute(f"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = ?", table_name)
            return cursor.fetchone()[0] > 0
        except Exception as e:
            logger.error(f"检查表 {table_name} 存在性失败: {e}")
            return False
        finally:
            cursor.close()
    
    def get_table_columns(self, table_name):
        """获取表的所有列信息"""
        cursor = self.conn.cursor()
        columns = []
        try:
            cursor.execute(f"""
                SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH, ORDINAL_POSITION
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = ? 
                ORDER BY ORDINAL_POSITION
            """, table_name)
            
            for row in cursor.fetchall():
                columns.append({
                    'name': row.COLUMN_NAME,
                    'type': row.DATA_TYPE,
                    'nullable': row.IS_NULLABLE,
                    'length': row.CHARACTER_MAXIMUM_LENGTH,
                    'position': row.ORDINAL_POSITION
                })
            return columns
        except Exception as e:
            logger.error(f"获取表 {table_name} 列信息失败: {e}")
            return []
        finally:
            cursor.close()
    
    def get_primary_key_constraint_name(self, table_name):
        """获取主键约束名称"""
        cursor = self.conn.cursor()
        try:
            cursor.execute(f"""
                SELECT CONSTRAINT_NAME 
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS 
                WHERE TABLE_NAME = ? AND CONSTRAINT_TYPE = 'PRIMARY KEY'
            """, table_name)
            result = cursor.fetchone()
            return result[0] if result else None
        except Exception as e:
            logger.error(f"获取主键约束名称失败: {e}")
            return None
        finally:
            cursor.close()
    
    def has_foreign_keys(self, table_name):
        """检查表是否有外键关联"""
        cursor = self.conn.cursor()
        try:
            # 检查是否被其他表引用
            cursor.execute(f"""
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS RC
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU 
                    ON RC.UNIQUE_CONSTRAINT_NAME = KCU.CONSTRAINT_NAME
                WHERE KCU.TABLE_NAME = ?
            """, table_name)
            count = cursor.fetchone()[0]
            return count > 0
        except Exception as e:
            logger.error(f"检查外键失败: {e}")
            return True  # 出错时假定有外键，避免风险
        finally:
            cursor.close()
    
    def backup_table(self, table_name):
        """备份表数据"""
        backup_table = f"{table_name}_backup_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
        cursor = self.conn.cursor()
        try:
            cursor.execute(f"SELECT * INTO {backup_table} FROM {table_name}")
            self.conn.commit()
            logger.info(f"表 {table_name} 已备份到 {backup_table}")
            return backup_table
        except Exception as e:
            self.conn.rollback()
            logger.error(f"备份表 {table_name} 失败: {e}")
            return None
    
    def get_table_indexes(self, table_name):
        """获取表的索引信息"""
        cursor = self.conn.cursor()
        indexes = []
        try:
            cursor.execute(f"""
                SELECT 
                    i.name AS index_name,
                    i.is_unique,
                    i.type_desc,
                    c.name AS column_name,
                    ic.key_ordinal
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                INNER JOIN sys.tables t ON i.object_id = t.object_id
                WHERE t.name = ? AND i.is_primary_key = 0
                ORDER BY i.name, ic.key_ordinal
            """, table_name)
            
            current_index = None
            for row in cursor.fetchall():
                if current_index != row.index_name:
                    if current_index:
                        indexes.append(current_index_data)
                    current_index = row.index_name
                    current_index_data = {
                        'name': row.index_name,
                        'unique': row.is_unique,
                        'type': row.type_desc,
                        'columns': []
                    }
                current_index_data['columns'].append({
                    'name': row.column_name,
                    'ordinal': row.key_ordinal
                })
            
            if current_index:
                indexes.append(current_index_data)
                
            return indexes
        except Exception as e:
            logger.error(f"获取表 {table_name} 索引信息失败: {e}")
            return []
        finally:
            cursor.close()
    
    def check_column_exists(self, table_name, column_name):
        """检查表中是否存在指定列"""
        cursor = self.conn.cursor()
        try:
            cursor.execute(f"""
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = ? AND COLUMN_NAME = ?
            """, table_name, column_name)
            return cursor.fetchone()[0] > 0
        except Exception as e:
            logger.error(f"检查列 {column_name} 失败: {e}")
            return False
        finally:
            cursor.close()
    
    def get_user_id_mapping(self, table_name, createuser_col, updateuser_col):
        """获取表中所有用户的映射关系，相同的用户名映射到同一个雪花ID"""
        cursor = self.conn.cursor()
        try:
            # 获取所有不重复的用户名
            if createuser_col and updateuser_col:
                cursor.execute(f"SELECT DISTINCT {createuser_col}, {updateuser_col} FROM {table_name} WHERE {createuser_col} IS NOT NULL OR {updateuser_col} IS NOT NULL")
            elif createuser_col:
                cursor.execute(f"SELECT DISTINCT {createuser_col} FROM {table_name} WHERE {createuser_col} IS NOT NULL")
            elif updateuser_col:
                cursor.execute(f"SELECT DISTINCT {updateuser_col} FROM {table_name} WHERE {updateuser_col} IS NOT NULL")
            else:
                return {}
            
            users = set()
            for row in cursor.fetchall():
                if createuser_col and getattr(row, createuser_col, None):
                    users.add(getattr(row, createuser_col))
                if updateuser_col and getattr(row, updateuser_col, None):
                    users.add(getattr(row, updateuser_col))
            
            # 为每个用户名生成雪花ID
            user_mapping = {}
            for username in users:
                if username and username not in user_mapping:
                    user_mapping[username] = self.snowflake_generator.generate_id()
                    logger.info(f"为用户 '{username}' 生成雪花ID: {user_mapping[username]}")
            
            return user_mapping
            
        except Exception as e:
            logger.error(f"获取用户映射失败: {e}")
            return {}
        finally:
            cursor.close()
    
    def recreate_table_with_new_id(self, table_name, columns, id_mapping, user_mapping):
        """重新创建表以保证ID列在第一位置，并处理列名重命名和新增列"""
        cursor = self.conn.cursor()
        try:
            # 检查是否已存在目标列
            has_createuserid = self.check_column_exists(table_name, "CreateUserId")
            has_createusername = self.check_column_exists(table_name, "CreateUserName")
            has_updateuserid = self.check_column_exists(table_name, "UpdateUserId")
            has_updateusername = self.check_column_exists(table_name, "UpdateUserName")
            has_isdelete = self.check_column_exists(table_name, "IsDelete")
            
            # 1. 创建新表
            new_table_name = f"_{table_name}_new"
            
            # 构建创建表的SQL
            create_sql = f"CREATE TABLE {new_table_name} (\n"
            
            # 首先添加ID列
            create_sql += "    id BIGINT NOT NULL PRIMARY KEY"
            
            # 添加其他列（排除原ID列）
            for col in columns:
                if col['name'].lower() != 'id':
                    col_name = col['name']
                    col_type = col['type']
                    
                    # 处理列名重命名
                    if col_name.lower() == 'createuser' and not has_createuserid:
                        col_name = 'CreateUserId'
                        # 将类型改为bigint
                        col_type = 'BIGINT'
                        logger.info(f"将列名从 'CreateUser' 重命名为 'CreateUserId'，类型改为 BIGINT")
                    elif col_name.lower() == 'updateuser' and not has_updateuserid:
                        col_name = 'UpdateUserId'
                        # 将类型改为bigint
                        col_type = 'BIGINT'
                        logger.info(f"将列名从 'UpdateUser' 重命名为 'UpdateUserId'，类型改为 BIGINT")
                    elif col_name.lower() == 'isdel' and not has_isdelete:
                        col_name = 'IsDelete'
                        logger.info(f"将列名从 'IsDel' 重命名为 'IsDelete'")
                    
                    create_sql += f",\n    {col_name} "
                    
                    # 处理数据类型
                    if col_type.lower() in ['nvarchar', 'varchar', 'nchar', 'char']:
                        if col['length'] and col['length'] > 0:
                            create_sql += f"{col_type}({col['length']})"
                        else:
                            create_sql += f"{col_type}(MAX)"
                    else:
                        create_sql += col_type
                    
                    # 处理可空性
                    if col['nullable'] == 'NO':
                        create_sql += " NOT NULL"
                    else:
                        create_sql += " NULL"
            
            # 新增CreateUserName列（如果不存在）
            if not has_createusername:
                create_sql += f",\n    CreateUserName NVARCHAR(64) NULL"
                logger.info(f"新增列 'CreateUserName'")
            
            # 新增UpdateUserName列（如果不存在）
            if not has_updateusername:
                create_sql += f",\n    UpdateUserName NVARCHAR(64) NULL"
                logger.info(f"新增列 'UpdateUserName'")
            
            create_sql += "\n)"
            
            cursor.execute(create_sql)
            logger.info(f"已创建新表 {new_table_name}")
            
            # 2. 插入数据
            # 构建列名映射（处理列名重命名）
            column_mapping = {}
            select_columns = []
            insert_columns = []
            
            # 确定原表中的CreateUser和UpdateUser列名
            original_createuser_col = None
            original_updateuser_col = None
            
            for col in columns:
                if col['name'].lower() != 'id':
                    col_name = col['name']
                    new_col_name = col_name
                    
                    # 处理列名重命名
                    if col_name.lower() == 'createuser' and not has_createuserid:
                        new_col_name = 'CreateUserId'
                        original_createuser_col = col_name
                    elif col_name.lower() == 'updateuser' and not has_updateuserid:
                        new_col_name = 'UpdateUserId'
                        original_updateuser_col = col_name
                    elif col_name.lower() == 'isdel' and not has_isdelete:
                        new_col_name = 'IsDelete'
                    
                    column_mapping[col_name] = new_col_name
                    select_columns.append(col_name)
                    insert_columns.append(new_col_name)
            
            # 添加新增列到插入列列表
            if not has_createusername:
                insert_columns.append('CreateUserName')
            if not has_updateusername:
                insert_columns.append('UpdateUserName')
            
            select_clause = ", ".join(select_columns) if select_columns else ""
            insert_clause = "id, " + ", ".join(insert_columns) if insert_columns else "id"
            
            # 构建SELECT语句
            if select_columns:
                select_sql = f"SELECT id, {select_clause} FROM {table_name}"
            else:
                select_sql = f"SELECT id FROM {table_name}"
            
            # 对于每行数据，使用映射的新ID
            cursor.execute(select_sql)
            rows = cursor.fetchall()
            
            for row in rows:
                old_id = row[0]
                new_id = id_mapping.get(old_id)
                if not new_id:
                    new_id = self.snowflake_generator.generate_id()
                    id_mapping[old_id] = new_id
                
                # 构建插入语句和值
                placeholders = "?" + ", ?" * (len(insert_columns)) if insert_columns else "?"
                values = [new_id]
                
                # 按正确的列顺序添加值
                for i, col_name in enumerate(select_columns):
                    col_value = getattr(row, col_name, None)
                    
                    # 处理CreateUser和UpdateUser列的数据类型转换
                    if col_name.lower() == 'createuser' and not has_createuserid:
                        # 将用户名转换为雪花ID
                        if col_value is not None and col_value in user_mapping:
                            values.append(user_mapping[col_value])
                            logger.debug(f"将CreateUser '{col_value}' 转换为雪花ID: {user_mapping[col_value]}")
                        else:
                            values.append(None)
                    elif col_name.lower() == 'updateuser' and not has_updateuserid:
                        # 将用户名转换为雪花ID
                        if col_value is not None and col_value in user_mapping:
                            values.append(user_mapping[col_value])
                            logger.debug(f"将UpdateUser '{col_value}' 转换为雪花ID: {user_mapping[col_value]}")
                        else:
                            values.append(None)
                    else:
                        values.append(col_value)
                
                # 为新增列添加值
                if not has_createusername:
                    # 保留原用户名到CreateUserName列
                    createuser_value = getattr(row, original_createuser_col, None) if original_createuser_col else None
                    values.append(createuser_value)
                
                if not has_updateusername:
                    # 保留原用户名到UpdateUserName列
                    updateuser_value = getattr(row, original_updateuser_col, None) if original_updateuser_col else None
                    values.append(updateuser_value)
                
                cursor.execute(f"INSERT INTO {new_table_name} ({insert_clause}) VALUES ({placeholders})", values)
            
            logger.info(f"已插入 {len(rows)} 行数据到新表")
            
            # 3. 重建索引（处理列名映射）
            indexes = self.get_table_indexes(table_name)
            for index in indexes:
                if index['name'].startswith('PK_'):
                    continue  # 跳过主键，因为已经在建表时创建
                    
                unique_keyword = "UNIQUE" if index['unique'] else ""
                
                # 处理索引列名映射
                index_columns = []
                for col in index['columns']:
                    new_col_name = column_mapping.get(col['name'], col['name'])
                    index_columns.append(new_col_name)
                
                index_columns_clause = ", ".join(index_columns)
                create_index_sql = f"CREATE {unique_keyword} INDEX {index['name']} ON {new_table_name} ({index_columns_clause})"
                cursor.execute(create_index_sql)
                logger.info(f"已重建索引 {index['name']}")
            
            return new_table_name, True
            
        except Exception as e:
            logger.error(f"重新创建表失败: {e}")
            return None, False
        finally:
            cursor.close()
    
    def convert_table_id(self, table_name):
        """转换单个表的ID类型和数据，并处理列名重命名和新增列"""
        logger.info(f"开始处理表: {table_name}")
        
        # 检查表是否存在
        if not self.check_table_exists(table_name):
            logger.error(f"表 {table_name} 不存在")
            return False
        
        # 检查是否有外键关联
        if self.has_foreign_keys(table_name):
            logger.error(f"表 {table_name} 存在外键关联，跳过处理")
            return False
        
        # 获取列信息
        columns = self.get_table_columns(table_name)
        id_column = None
        has_isdel = False
        has_createuser = False
        has_updateuser = False
        
        for col in columns:
            if col['name'].lower() == 'id':
                id_column = col
            elif col['name'].lower() == 'isdel':
                has_isdel = True
            elif col['name'].lower() == 'createuser':
                has_createuser = True
            elif col['name'].lower() == 'updateuser':
                has_updateuser = True
        
        if not id_column:
            logger.error(f"表 {table_name} 没有找到名为'id'的列")
            return False
        
        # 检查是否已存在目标列
        has_isdelete = self.check_column_exists(table_name, "IsDelete")
        has_createuserid = self.check_column_exists(table_name, "CreateUserId")
        has_createusername = self.check_column_exists(table_name, "CreateUserName")
        has_updateuserid = self.check_column_exists(table_name, "UpdateUserId")
        has_updateusername = self.check_column_exists(table_name, "UpdateUserName")
        
        # 检查列重命名冲突
        if has_isdel and has_isdelete:
            logger.warning(f"表 {table_name} 同时存在IsDel和IsDelete列，将保留IsDelete列，忽略IsDel列")
            has_isdel = False
        
        if has_createuser and has_createuserid:
            logger.warning(f"表 {table_name} 同时存在CreateUser和CreateUserId列，将保留CreateUserId列，忽略CreateUser列")
            has_createuser = False
        
        if has_updateuser and has_updateuserid:
            logger.warning(f"表 {table_name} 同时存在UpdateUser和UpdateUserId列，将保留UpdateUserId列，忽略UpdateUser列")
            has_updateuser = False
        
        # 获取主键约束名称
        pk_constraint = self.get_primary_key_constraint_name(table_name)
        if not pk_constraint:
            logger.error(f"表 {table_name} 没有找到主键约束")
            return False
        
        # 获取用户映射（将用户名转换为雪花ID）
        user_mapping = {}
        if has_createuser or has_updateuser:
            createuser_col = 'CreateUser' if has_createuser and not has_createuserid else None
            updateuser_col = 'UpdateUser' if has_updateuser and not has_updateuserid else None
            user_mapping = self.get_user_id_mapping(table_name, createuser_col, updateuser_col)
            logger.info(f"已为 {len(user_mapping)} 个不重复的用户名生成雪花ID映射")
        
        if id_column['type'].lower() != 'nvarchar':
            logger.info(f"表 {table_name} 的ID列类型已经是 {id_column['type']}，无需转换")
            # 但仍然检查是否需要处理其他列
            if has_isdel or has_createuser or has_updateuser or not has_createusername or not has_updateusername:
                logger.info(f"表 {table_name} 需要处理其他列的修改")
        
        # 备份表
        backup_table = self.backup_table(table_name)
        if not backup_table:
            return False
        
        cursor = self.conn.cursor()
        try:
            # 生成ID映射
            cursor.execute(f"SELECT id FROM {table_name}")
            rows = cursor.fetchall()
            
            id_mapping = {}
            for row in rows:
                old_id = row[0]
                new_id = self.snowflake_generator.generate_id()
                id_mapping[old_id] = new_id
            
            logger.info(f"已为 {len(id_mapping)} 行数据生成雪花ID")
            
            # 使用重新创建表的方法来保证ID列顺序并处理列名重命名和新增列
            new_table_name, success = self.recreate_table_with_new_id(table_name, columns, id_mapping, user_mapping)
            
            if not success:
                raise Exception("重新创建表失败")
            
            # 删除原表
            cursor.execute(f"DROP TABLE {table_name}")
            logger.info(f"已删除原表 {table_name}")
            
            # 重命名新表为原表名
            cursor.execute(f"EXEC sp_rename '{new_table_name}', '{table_name}'")
            logger.info(f"已重命名新表为 {table_name}")
            
            self.conn.commit()
            logger.info(f"表 {table_name} 转换完成")
            return True
            
        except Exception as e:
            self.conn.rollback()
            logger.error(f"转换表 {table_name} 失败: {e}")
            
            # 恢复备份
            try:
                cursor.execute(f"DROP TABLE IF EXISTS {table_name}")
                cursor.execute(f"EXEC sp_rename '{backup_table}', '{table_name}'")
                self.conn.commit()
                logger.info(f"已从备份恢复表 {table_name}")
            except Exception as restore_error:
                logger.error(f"恢复备份失败: {restore_error}")
            
            return False
        finally:
            cursor.close()
    
    def batch_convert_tables(self, table_list):
        """批量转换表"""
        if not self.connect():
            return False
        
        success_count = 0
        total_count = len(table_list)
        
        for table_name in table_list:
            if self.convert_table_id(table_name):
                success_count += 1
            logger.info("-" * 50)
        
        logger.info(f"转换完成: 成功 {success_count}/{total_count} 个表")
        self.disconnect()
        return success_count == total_count

def main():
    # 数据库连接配置
    connection_string = (
        "DRIVER={SQL Server};"
        "SERVER=47.95.120.53;"
        "DATABASE=JC44_WMS;"
        "UID=sa;"
        "PWD=boxline!@#;"
    )
    
    # 指定要转换的表列表
    tables_to_convert = [
        "WmsExtractOrder",
        # "WmsImportNotifyDetail",
        # "table3"
        # 添加更多表名...
    ]
    
    if not tables_to_convert:
        logger.error("请指定要转换的表列表")
        return
    
    # 创建转换器并执行转换
    converter = SQLServerIDConverter(connection_string)
    converter.batch_convert_tables(tables_to_convert)

if __name__ == "__main__":
    main()