# XuguDB 官方文档索引（Agent 必读）

> 文档根目录：`E:\BaiduSyncdisk\docs\content\`  
> **任何 SQL 方言实现前，必须打开对应文档阅读，不得跳过。**

## 一、EF Core 生态（优先阅读）

| 文档 | 路径 |
|------|------|
| EF Core 使用手册 | `ecosystem/orm/dotnet/efcore.md` |
| FreeSql | `ecosystem/orm/dotnet/freesql.md` |
| Dapper | `ecosystem/orm/dotnet/dapper.md` |

## 二、兼容模式（MySQL 开发必看）

| 文档 | 路径 | 用途 |
|------|------|------|
| COMPATIBLE_MODE 会话参数 | `reference/system-configuration-parameter/session-parameter/compatible_mode.md` | MySQL/Oracle/PG 兼容模式 |
| def_compatible_mode | `reference/system-configuration-parameter/xugu.ini/compatible/def_compatible_mode.md` | 系统默认兼容模式 |
| IDENTITY_MODE | `reference/system-configuration-parameter/session-parameter/identity_mode.md` | 自增列插入行为 |
| def_identity_mode | `reference/system-configuration-parameter/xugu.ini/compatible/def_identity_mode.md` | 自增列默认模式 |

## 三、SQL 基础

| 文档 | 路径 | Provider 模块 |
|------|------|--------------|
| 标识符 | `reference/sql/identifier.md` | Storage/SqlGenerationHelper |
| 关键字 | `reference/sql/keyword.md` | QuerySqlGenerator |
| 字面量 | `reference/sql/literal.md` | QuerySqlGenerator |
| 字符集 | `reference/sql/charset.md` | TypeMapping |
| 类型转换 | `reference/sql/type_conversion.md` | TypeMapping |

## 四、数据类型（TypeMapping 必看）

| 文档 | 路径 |
|------|------|
| 数值类型 | `reference/sql/datatype/numerical.md` |
| 字符类型 | `reference/sql/datatype/character.md` |
| 时间类型 | `reference/sql/datatype/datetime.md` |
| 布尔类型 | `reference/sql/datatype/bool.md` |
| 二进制类型 | `reference/sql/datatype/binary.md` |
| GUID | `reference/sql/datatype/guid.md` |
| JSON | `reference/sql/datatype/json.md` |
| BIT | `reference/sql/datatype/bit.md` |

## 五、DML（Update 模块必看）

| 文档 | 路径 |
|------|------|
| INSERT | `reference/sql/dml/insert.md` |
| UPDATE | `reference/sql/dml/update.md`（若存在） |
| DELETE | `reference/sql/dml/delete.md`（若存在） |

## 六、SELECT / Query（Query 模块必看）

| 文档 | 路径 |
|------|------|
| SELECT 主语法 | `reference/sql/select/select.md` |
| WHERE | `reference/sql/select/where.md` |
| JOIN | `reference/sql/select/join.md` |
| ORDER BY | `reference/sql/select/order-by.md` |
| GROUP BY | `reference/sql/select/group-by.md` |
| LIMIT/TOP | `reference/sql/select/resultset-restricted.md` |
| 子查询 | `reference/sql/select/subquery.md` |
| SET 操作 | `reference/sql/select/set.md` |
| WITH | `reference/sql/select/with.md` |

## 七、表达式与函数（Translator 必看）

| 文档 | 路径 |
|------|------|
| 函数调用概述 | `reference/sql/expression/function.md` |
| 常量表达式 | `reference/sql/expression/constant.md` |
| 数学函数 | `reference/function/mathematical-functions/` |
| 字符串函数 | `reference/function/string-functions/` |
| 日期时间函数 | `reference/function/datetime-functions/` |
| 聚合函数 | `reference/function/aggregate-functions/` |

## 八、DDL / Migrations

| 文档 | 路径 |
|------|------|
| DDL 概述 | `reference/sql/ddl/README.md` |
| CREATE | `reference/sql/ddl/create-dir/`（目录下各文件） |

## 九、按 Provider 模块速查

| 模块 | 必读文档 |
|------|---------|
| **Infrastructure** | `ecosystem/orm/dotnet/efcore.md`, `compatible_mode.md` |
| **Storage/TypeMapping** | `reference/sql/datatype/*`, `type_conversion.md` |
| **Storage/SqlHelper** | `reference/sql/identifier.md`, `charset.md` |
| **Query/SqlGenerator** | `reference/sql/select/*`, `resultset-restricted.md` |
| **Query/Translators** | `reference/sql/expression/function.md`, `reference/function/**` |
| **Update** | `reference/sql/dml/insert.md`, `identity_mode.md` |
| **Migrations** | `reference/sql/ddl/**`, `datatype/*` |

## 十、Handoff 文档引用格式

完工时在 Handoff 中必须注明：

```
文档依据: E:\BaiduSyncdisk\docs\content\{相对路径}
```

示例：

```
文档依据: reference/sql/select/resultset-restricted.md — LIMIT 语法使用 LIMIT offset, count 和 LIMIT count OFFSET offset 两种形式
```
