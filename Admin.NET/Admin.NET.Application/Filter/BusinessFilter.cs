using Admin.NET.Application.Entity;
using SqlSugar;

using System.Linq.Expressions;

/// <summary>
/// 自定义业务实体过滤器示例
/// </summary>
public class BusinessFilter : IEntityFilter
{
    /// <summary>
    /// 超管在权限更改时, 将被更改的用户权限进行更新
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IEnumerable<TableFilterItem<object>> AddEntityFilter(string userId)
    {
        return SetFilter(userId);
    }

    private IEnumerable<TableFilterItem<object>> SetFilter(string userId = "")
    {
        var db = new SqlSugarRepository<WmsPrmissionScope>(); // 修正了类型名拼写错误
        if (string.IsNullOrEmpty(userId))
        {
            userId = App.User?.FindFirst(ClaimConst.UserId)?.Value;
        }
        if (string.IsNullOrEmpty(userId))
            return new List<TableFilterItem<object>>();

        var scopes = db.GetList(x => x.UserId.ToString() == userId);

        var result = new List<TableFilterItem<object>>();

        foreach (var scope in scopes)
        {
            // 获取实体类型
            var entityType = Type.GetType($"Admin.NET.Application.Entity.{scope.TableName}, Admin.NET.Application");
            if (entityType == null)
                continue;

            // 构造表达式 u => new[] { value1, value2 }.Contains(u.FieldName)
            var lambda = BuildFilterExpression(entityType, scope.FieldName, scope.FieldValue);
            if (lambda == null)
                continue;

            // 直接创建 TableFilterItem<object>
            var baseItem = new TableFilterItem<object>(entityType, lambda);
            result.Add(baseItem);
        }

        return result;
    }

    private LambdaExpression BuildFilterExpression(Type entityType, string fieldName, string fieldValue)
    {
        if (string.IsNullOrWhiteSpace(fieldValue))
            return null;

        try
        {
            // 参数 u =>
            var parameter = Expression.Parameter(entityType, "u");

            // 属性 u.FieldName
            var property = Expression.PropertyOrField(parameter, fieldName);
            var propType = property.Type;

            // 拆分多个值
            var rawValues = fieldValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(v => v.Trim())
                                      .ToArray();

            // 转换为属性类型
            var typedValues = Array.CreateInstance(propType, rawValues.Length);
            for (int i = 0; i < rawValues.Length; i++)
            {
                try
                {
                    var convertedValue = Convert.ChangeType(rawValues[i], propType);
                    typedValues.SetValue(convertedValue, i);
                }
                catch (Exception ex)
                {
                    // 处理类型转换异常
                    Console.WriteLine($"值转换失败: {rawValues[i]} -> {propType.Name}, 错误: {ex.Message}");
                    return null;
                }
            }

            // 常量数组
            var constantArray = Expression.Constant(typedValues, typedValues.GetType());

            // Enumerable.Contains<T>(IEnumerable<T>, T)
            var containsMethod = typeof(Enumerable)
                .GetMethods()
                .First(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                .MakeGenericMethod(propType);

            // 调用 Contains：array.Contains(u.FieldName)
            var body = Expression.Call(containsMethod, constantArray, property);

            // 返回 Lambda 表达式： u => array.Contains(u.FieldName)
            return Expression.Lambda(body, parameter);
        }
        catch (Exception ex)
        {
            // 处理表达式构建过程中的异常
            Console.WriteLine($"构建表达式失败: {entityType.Name}.{fieldName}, 错误: {ex.Message}");
            return null;
        }
    }
}