#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
WmsBaseSlot表处理脚本
功能：按层和巷道分组，在同一行内为每个巷道组内的储位找到最近的跑道
跑道在整个层和行内共享，储位按巷道分组
"""

import pyodbc
import math
from typing import List, Dict, Optional
import time
from collections import defaultdict


class WmsBaseSlotProcessor:
    def __init__(self, connection_string: str):
        self.connection_string = connection_string
        self.conn = None
        self.cursor = None
        
    def connect(self):
        """连接到数据库"""
        try:
            self.conn = pyodbc.connect(self.connection_string)
            self.cursor = self.conn.cursor()
            print("✅ 数据库连接成功")
        except Exception as e:
            print(f"❌ 数据库连接失败: {e}")
            raise
    
    def close(self):
        """关闭数据库连接"""
        if self.cursor:
            self.cursor.close()
        if self.conn:
            self.conn.close()
        print("✅ 数据库连接已关闭")
    
    def execute_query(self, sql: str) -> List[Dict]:
        """执行查询并返回结果集"""
        try:
            self.cursor.execute(sql)
            columns = [column[0] for column in self.cursor.description]
            results = []
            for row in self.cursor.fetchall():
                row_dict = {}
                for i, col in enumerate(columns):
                    row_dict[col] = row[i]
                results.append(row_dict)
            
            print(f"✅ 查询成功，返回 {len(results)} 行数据")
            return results
            
        except Exception as e:
            print(f"❌ 查询执行失败: {e}")
            raise
    
    def calculate_horizontal_distance(self, slot1: Dict, slot2: Dict) -> float:
        """计算两个储位之间的水平距离（只考虑列）"""
        col1 = int(slot1.get('SlotColumn', 0) or 0)
        col2 = int(slot2.get('SlotColumn', 0) or 0)
        return abs(col2 - col1)
    
    def find_nearest_runway_in_same_row(self, slot: Dict, runways_in_same_row: List[Dict]) -> Optional[Dict]:
        """在同一行内找到离给定储位最近的跑道"""
        if not runways_in_same_row:
            return None
        
        nearest_runway = None
        min_distance = float('inf')
        
        for runway in runways_in_same_row:
            distance = self.calculate_horizontal_distance(slot, runway)
            if distance < min_distance:
                min_distance = distance
                nearest_runway = runway
        
        return nearest_runway
    
    def process_by_layer_laneway_and_row(self, data: List[Dict]) -> List[Dict]:
        """
        按层、巷道和行分组处理数据
        跑道在整个层和行内共享，储位按巷道分组
        """
        # 结构：按层分组，每层内按行分组，每行内跑道共享，储位按巷道分组
        # {layer: {row: {'runways': [], 'slots_by_laneway': {laneway_id: []}}}}
        layer_groups = defaultdict(lambda: defaultdict(lambda: {'runways': [], 'slots_by_laneway': defaultdict(list)}))
        
        # 首先分离跑道和储位数据
        for item in data:
            make_value = str(item.get('Make', '')).strip()
            slot_status = int(item.get('SlotStatus'))
            
            if make_value == '02':
                # 跑道：添加到对应的层和行
                layer = item.get('SlotLayer')
                row = item.get('SlotRow')
                if layer is not None and row is not None:
                    layer_groups[layer][row]['runways'].append(item)
            elif make_value == '01' and slot_status == 0:
                # 储位：按巷道分组
                layer = item.get('SlotLayer')
                row = item.get('SlotRow')
                laneway_id = item.get('SlotLanewayId')
                if layer is not None and row is not None and laneway_id is not None:
                    layer_groups[layer][row]['slots_by_laneway'][laneway_id].append(item)
        
        # 统计信息
        total_runways = sum(len(group['runways']) for rows in layer_groups.values() for group in rows.values())
        total_slots = sum(len(slots) for rows in layer_groups.values() for group in rows.values() for slots in group['slots_by_laneway'].values())
        print(f"📊 找到 {total_runways} 个跑道(Make='02')")
        print(f"📊 找到 {total_slots} 个待处理储位(Make='01'且SlotStatus=0)")
        print(f"📊 按层、行分组完成，共 {len(layer_groups)} 层")
        
        # 处理每个层、每行、每个巷道的数据
        updates_needed = []
        
        for layer, row_dict in layer_groups.items():
            for row, group in row_dict.items():
                runways_in_row = group['runways']
                slots_by_laneway = group['slots_by_laneway']
                
                if not runways_in_row:
                    # 如果该行没有跑道，跳过
                    continue
                
                # 对于每个巷道组内的储位
                for laneway_id, slots in slots_by_laneway.items():
                    # 为每个储位找到同一行内最近的跑道
                    for slot in slots:
                        nearest_runway = self.find_nearest_runway_in_same_row(slot, runways_in_row)
                        
                        if nearest_runway:
                            runway_code = nearest_runway.get('SlotCode')
                            current_transit = slot.get('EndTransitLocation')
                            
                            # 只更新当前值与跑道Code不同的记录
                            if current_transit != runway_code:
                                updates_needed.append({
                                    'slot_id': slot.get('Id'),
                                    'slot_code': slot.get('SlotCode'),
                                    'nearest_runway_code': runway_code,
                                    'runway_id': nearest_runway.get('Id'),
                                    'distance': self.calculate_horizontal_distance(slot, nearest_runway),
                                    'layer': layer,
                                    'laneway_id': laneway_id,
                                    'row': row,
                                    'old_value': current_transit,
                                    'slot_column': slot.get('SlotColumn'),
                                    'runway_column': nearest_runway.get('SlotColumn')
                                })
        
        print(f"\n🔔 需要更新 {len(updates_needed)} 条记录的EndTransitLocation字段")
        
        # 显示详细统计
        if updates_needed:
            self.print_detailed_stats(updates_needed)
        
        return updates_needed
    
    def print_detailed_stats(self, updates: List[Dict]):
        """打印详细统计信息"""
        print("\n📊 详细统计:")
        print("-" * 80)
        
        # 按层统计
        layer_stats = defaultdict(lambda: {'count': 0, 'laneways': set(), 'rows': set()})
        for update in updates:
            layer = update['layer']
            layer_stats[layer]['count'] += 1
            layer_stats[layer]['laneways'].add(update['laneway_id'])
            layer_stats[layer]['rows'].add(update['row'])
        
        print("按层统计:")
        for layer in sorted(layer_stats.keys()):
            stats = layer_stats[layer]
            print(f"  层 {layer}: {stats['count']} 条更新, 涉及 {len(stats['laneways'])} 个巷道, {len(stats['rows'])} 行")
        
        # 距离分布统计
        distance_stats = {}
        for update in updates:
            distance = update['distance']
            if distance not in distance_stats:
                distance_stats[distance] = 0
            distance_stats[distance] += 1
        
        print("\n距离分布 (前10种):")
        for distance in sorted(distance_stats.keys())[:10]:
            print(f"  距离 {distance}: {distance_stats[distance]} 条")
        
        if len(distance_stats) > 10:
            print(f"  ... 共 {len(distance_stats)} 种距离")
    
    def update_end_transit_location(self, updates: List[Dict], batch_size: int = 100):
        """批量更新EndTransitLocation字段"""
        if not updates:
            print("ℹ️ 没有需要更新的记录")
            return
        
        try:
            success_count = 0
            fail_count = 0
            
            print("🔄 开始执行更新操作...")
            
            for i in range(0, len(updates), batch_size):
                batch = updates[i:i+batch_size]
                
                for update in batch:
                    try:
                        sql = """
                        UPDATE [dbo].[WmsBaseSlot] 
                        SET [EndTransitLocation] = ?,
                            [UpdateTime] = GETDATE(),
                            [UpdateUserId] = ?
                        WHERE [Id] = ?
                        """
                        
                        update_user_id = update.get('update_user_id', 1)
                        self.cursor.execute(sql, update['nearest_runway_code'], update_user_id, update['slot_id'])
                        success_count += 1
                        
                    except Exception as e:
                        print(f"❌ 更新失败 (ID={update['slot_id']}): {e}")
                        fail_count += 1
                
                self.conn.commit()
                print(f"✅ 批次提交完成: {min(i+batch_size, len(updates))}/{len(updates)}")
            
            print(f"\n🎉 更新完成!")
            print(f"  成功: {success_count} 条")
            print(f"  失败: {fail_count} 条")
            
        except Exception as e:
            self.conn.rollback()
            print(f"❌ 批量更新失败: {e}")
            raise
    
    def verify_updates(self, updates: List[Dict]) -> Dict:
        """验证更新结果"""
        if not updates:
            return {"total": 0, "verified": 0, "correct": 0}
        
        total = len(updates)
        verified = 0
        correct = 0
        
        # 随机抽查一部分进行验证
        import random
        sample_size = min(50, len(updates))
        samples = random.sample(updates, sample_size) if len(updates) > 50 else updates
        
        print(f"\n🔍 正在验证 {len(samples)} 条更新记录...")
        
        for update in samples:
            try:
                sql = """
                SELECT [EndTransitLocation] 
                FROM [dbo].[WmsBaseSlot] 
                WHERE [Id] = ?
                """
                
                self.cursor.execute(sql, update['slot_id'])
                result = self.cursor.fetchone()
                
                if result:
                    verified += 1
                    new_value = result[0]
                    if new_value == update['nearest_runway_code']:
                        correct += 1
                    else:
                        print(f"❌ 验证失败: 储位ID={update['slot_id']}")
                        print(f"   期望值(跑道Code): '{update['nearest_runway_code']}'")
                        print(f"   实际值: '{new_value}'")
                else:
                    print(f"⚠️ 记录不存在: ID={update['slot_id']}")
                    
            except Exception as e:
                print(f"❌ 验证查询失败 (ID={update['slot_id']}): {e}")
        
        return {
            "total": total,
            "verified": verified,
            "correct": correct,
            "accuracy": f"{(correct/verified*100):.2f}%" if verified > 0 else "N/A"
        }


def main():
    """主函数"""
    print("=" * 80)
    print("🏢 WmsBaseSlot表处理脚本")
    print("=" * 80)
    print("📋 功能说明:")
    print("  1. 按层(SlotLayer)分组 → 在每层内按行(SlotRow)分组")
    print("  2. 跑道(Make='02')在整个层和行内共享")
    print("  3. 储位(Make='01'且SlotStatus=0)按巷道(SlotLanewayId)分组")
    print("  4. 为每个巷道组内的储位找到同一行内最近的跑道")
    print("  5. 将跑道的SlotCode更新到储位的EndTransitLocation字段")
    print("=" * 80)
    
    # 数据库连接配置
    server = '47.95.120.53'  # 服务器地址
    database = 'JC44_WMS'  # 数据库名
    username = 'sa'  # 用户名
    password = 'boxline!@#'  # 密码
    
    connection_string = (
        f'DRIVER={{ODBC Driver 17 for SQL Server}};'
        f'SERVER={server};'
        f'DATABASE={database};'
        f'UID={username};'
        f'PWD={password}'
    )
    
    # 查询SQL
    query_sql = """
    SELECT 
        [Id],
        [SlotCode],
        [Make],
        [SlotStatus],
        [SlotLayer],
        [SlotLanewayId],
        [SlotRow],
        [SlotColumn],
        [EndTransitLocation]
    FROM [dbo].[WmsBaseSlot]
    WHERE [IsDelete] = 0 
      AND [SlotLayer] IS NOT NULL
      AND [SlotRow] IS NOT NULL 
      AND [SlotColumn] IS NOT NULL
      AND [Make] IN ('01', '02')
    ORDER BY [SlotLayer], [SlotRow], [SlotColumn]
    """
    
    try:
        processor = WmsBaseSlotProcessor(connection_string)
        processor.connect()
        
        print("📥 正在查询数据...")
        start_time = time.time()
        data = processor.execute_query(query_sql)
        query_time = time.time() - start_time
        print(f"⏱️ 数据查询完成，耗时: {query_time:.2f}秒")
        
        if not data:
            print("ℹ️ 没有查询到数据")
            return
        
        print("\n🔄 正在处理数据...")
        print("📊 分组逻辑: 按层 → 在每层内按行 → 跑道整行共享 → 储位按巷道分组")
        print("🎯 更新目标: 为每个巷道组内的储位找到同一行内最近的跑道，更新EndTransitLocation")
        process_start = time.time()
        updates_needed = processor.process_by_layer_laneway_and_row(data)
        process_time = time.time() - process_start
        print(f"⏱️ 数据处理完成，耗时: {process_time:.2f}秒")
        
        if updates_needed:
            print("\n" + "="*80)
            print("📋 更新摘要")
            print("="*80)
            
            # 显示统计信息
            layers = len(set(u['layer'] for u in updates_needed))
            laneways = len(set(u['laneway_id'] for u in updates_needed))
            rows = len(set((u['layer'], u['row']) for u in updates_needed))
            
            print(f"📈 统计信息:")
            print(f"  总更新数: {len(updates_needed)}")
            print(f"  涉及层数: {layers}")
            print(f"  涉及巷道数: {laneways}")
            print(f"  涉及行数: {rows}")
            
            # 确认是否执行更新
            print("\n⚠️ 确认更新操作")
            confirm = input(f"确认更新 {len(updates_needed)} 条记录吗？(yes/no/预览): ")
            
            if confirm.lower() == 'yes':
                # 设置更新用户ID
                update_user_id = input("请输入更新用户ID（默认1）: ").strip()
                if not update_user_id:
                    update_user_id = 1
                else:
                    update_user_id = int(update_user_id)
                
                # 将更新用户ID添加到每条更新记录
                for update in updates_needed:
                    update['update_user_id'] = update_user_id
                
                # 执行批量更新
                print("\n🔄 正在执行更新操作...")
                update_start = time.time()
                processor.update_end_transit_location(updates_needed, batch_size=50)
                update_time = time.time() - update_start
                print(f"⏱️ 更新操作完成，耗时: {update_time:.2f}秒")
                
                # 验证更新结果
                print("\n🔍 正在验证更新结果...")
                verify_result = processor.verify_updates(updates_needed)
                
                print("\n✅ 验证结果:")
                print(f"  总更新数: {verify_result['total']}")
                print(f"  已验证数: {verify_result['verified']}")
                print(f"  正确数: {verify_result['correct']}")
                print(f"  准确率: {verify_result['accuracy']}")
                
            elif confirm.lower() == '预览':
                # 显示前几条更新内容（预览）
                print("\n📋 预览前10条更新内容:")
                for i, update in enumerate(updates_needed[:10]):
                    print(f"{i+1}. 层={update['layer']}, 巷道={update['laneway_id']}, 行={update['row']}")
                    print(f"   储位: ID={update['slot_id']}, Code={update['slot_code']}, 列={update['slot_column']}")
                    print(f"   跑道: Code={update['nearest_runway_code']}, 列={update['runway_column']}")
                    print(f"   距离: {update['distance']}, 原值='{update['old_value'] or '空'}'")
                    print("-"*40)
                
                print(f"\n💡 还有 {len(updates_needed)-10} 条更新记录未显示")
            else:
                print("⏸️ 更新操作已取消")
        else:
            print("ℹ️ 没有需要更新的记录")
        
        # 显示总体统计
        total_time = time.time() - start_time
        print(f"\n⏱️ 总耗时: {total_time:.2f}秒")
        print("="*80)
        print("🎉 脚本执行完成!")
        print("="*80)
        
    except Exception as e:
        print(f"\n❌ 程序执行出错: {e}")
        print("="*80)
    finally:
        # 关闭连接
        if 'processor' in locals():
            processor.close()


if __name__ == "__main__":
    main()