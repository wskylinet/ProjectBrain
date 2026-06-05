# Project Brain V1 - 数据库设计文档

## 1. 文档说明

### 项目名称

Project Brain

### 数据库

SQL Server

### 设计目标

用于统一管理各地区项目档案信息，包括：

* 项目基础信息
* VPN信息
* 服务器信息
* 数据库信息
* 部署信息
* 附件资料
* 项目备注

---

# 2. 数据模型

## 实体关系

```text
ProjectInfo
│
├── ProjectAccessInfo      (1:1)
│
├── ProjectServer          (1:N)
│
├── ProjectDatabase        (1:N)
│
├── ProjectDeployment      (1:N)
│
├── ProjectAttachment      (1:N)
│
└── ProjectRemark          (1:N)
```

---

# 3. 表设计

## 3.1 ProjectInfo（项目主表）

### 表说明

存储项目基础信息。

### 字段设计

| 字段名           | 类型            | 说明   |
| ------------- | ------------- | ---- |
| Id            | bigint        | 主键   |
| ProjectName   | nvarchar(100) | 项目名称 |
| Region        | nvarchar(50)  | 所属地区 |
| SystemName    | nvarchar(100) | 系统名称 |
| ProjectStatus | nvarchar(50)  | 项目状态 |
| OwnerName     | nvarchar(50)  | 负责人  |
| ContactInfo   | nvarchar(100) | 联系方式 |
| Description   | nvarchar(max) | 项目描述 |
| CreateTime    | datetime      | 创建时间 |
| UpdateTime    | datetime      | 更新时间 |
| IsDeleted     | bit           | 是否删除 |

### 示例数据

| 项目名称   | 地区 |
| ------ | -- |
| 智水家园   | 深圳 |
| 智慧水务平台 | 阜阳 |
| 数字供水平台 | 晋江 |

---

## 3.2 ProjectAccessInfo（访问信息表）

### 表说明

记录项目访问方式。

### 字段设计

| 字段名         | 类型            | 说明     |
| ----------- | ------------- | ------ |
| Id          | bigint        | 主键     |
| ProjectId   | bigint        | 项目ID   |
| VpnType     | nvarchar(50)  | VPN类型  |
| VpnName     | nvarchar(100) | VPN名称  |
| VpnRemark   | nvarchar(max) | VPN说明  |
| TestUrl     | nvarchar(500) | 测试环境地址 |
| ProdUrl     | nvarchar(500) | 生产环境地址 |
| DocumentUrl | nvarchar(500) | 文档地址   |
| CreateTime  | datetime      | 创建时间   |
| UpdateTime  | datetime      | 更新时间   |
| IsDeleted   | bit           | 是否删除   |

### 说明

密码不存储。

仅记录连接方式及使用说明。

---

## 3.3 ProjectServer（服务器信息表）

### 表说明

记录项目服务器信息。

### 字段设计

| 字段名             | 类型            | 说明    |
| --------------- | ------------- | ----- |
| Id              | bigint        | 主键    |
| ProjectId       | bigint        | 项目ID  |
| ServerName      | nvarchar(100) | 服务器名称 |
| ServerIp        | nvarchar(100) | IP地址  |
| RemotePort      | nvarchar(20)  | 远程端口  |
| ServerUsage     | nvarchar(100) | 服务器用途 |
| OperatingSystem | nvarchar(100) | 操作系统  |
| Remark          | nvarchar(max) | 备注    |
| CreateTime      | datetime      | 创建时间  |
| UpdateTime      | datetime      | 更新时间  |
| IsDeleted       | bit           | 是否删除  |

### 用途示例

* Web服务器
* 数据库服务器
* Redis服务器
* 文件服务器
* 时序库服务器

---

## 3.4 ProjectDatabase（数据库信息表）

### 表说明

记录项目数据库信息。

### 字段设计

| 字段名        | 类型            | 说明    |
| ---------- | ------------- | ----- |
| Id         | bigint        | 主键    |
| ProjectId  | bigint        | 项目ID  |
| DbType     | nvarchar(50)  | 数据库类型 |
| DbHost     | nvarchar(200) | 数据库地址 |
| DbPort     | nvarchar(20)  | 端口    |
| DbName     | nvarchar(100) | 数据库名称 |
| Remark     | nvarchar(max) | 备注    |
| CreateTime | datetime      | 创建时间  |
| UpdateTime | datetime      | 更新时间  |
| IsDeleted  | bit           | 是否删除  |

### 数据库类型

* SQL Server
* MySQL
* PostgreSQL
* SQLite
* Redis
* InfluxDB
* TDengine

---

## 3.5 ProjectDeployment（部署信息表）

### 表说明

记录项目部署方式。

### 字段设计

| 字段名         | 类型            | 说明      |
| ----------- | ------------- | ------- |
| Id          | bigint        | 主键      |
| ProjectId   | bigint        | 项目ID    |
| SiteName    | nvarchar(100) | IIS站点名称 |
| DeployPath  | nvarchar(500) | 部署目录    |
| LogPath     | nvarchar(500) | 日志目录    |
| PublishType | nvarchar(50)  | 发布方式    |
| Remark      | nvarchar(max) | 备注      |
| CreateTime  | datetime      | 创建时间    |
| UpdateTime  | datetime      | 更新时间    |
| IsDeleted   | bit           | 是否删除    |

### 发布方式

* IIS
* Docker
* Windows服务

---

## 3.6 ProjectAttachment（附件资料表）

### 表说明

存储项目相关附件。

### 字段设计

| 字段名              | 类型            | 说明    |
| ---------------- | ------------- | ----- |
| Id               | bigint        | 主键    |
| ProjectId        | bigint        | 项目ID  |
| FileName         | nvarchar(255) | 文件名称  |
| OriginalFileName | nvarchar(255) | 原始文件名 |
| FilePath         | nvarchar(500) | 文件路径  |
| FileType         | nvarchar(50)  | 文件类型  |
| FileSize         | bigint        | 文件大小  |
| Remark           | nvarchar(500) | 备注    |
| CreateTime       | datetime      | 创建时间  |
| UpdateTime       | datetime      | 更新时间  |
| IsDeleted        | bit           | 是否删除  |

### 支持格式

* Word
* Excel
* PDF
* 图片

---

## 3.7 ProjectRemark（项目备注表）

### 表说明

记录项目运维经验及补充说明。

### 字段设计

| 字段名        | 类型            | 说明   |
| ---------- | ------------- | ---- |
| Id         | bigint        | 主键   |
| ProjectId  | bigint        | 项目ID |
| Content    | nvarchar(max) | 备注内容 |
| CreateTime | datetime      | 创建时间 |
| UpdateTime | datetime      | 更新时间 |
| IsDeleted  | bit           | 是否删除 |

### 示例

```text
需先连接 EasyConnect VPN

使用堡垒机访问服务器

InfluxDB部署于独立服务器

生产环境发布需先停止Hangfire任务
```

---

# 4. 索引设计

```sql
ProjectInfo(ProjectName)

ProjectInfo(Region)

ProjectServer(ProjectId)

ProjectDatabase(ProjectId)

ProjectDeployment(ProjectId)

ProjectAttachment(ProjectId)

ProjectRemark(ProjectId)
```

---

# 5. 文件存储规范

## 上传目录

```text
/uploads

/uploads/project

/uploads/project/2026

/uploads/project/2026/05
```

## 存储内容

* 部署文档
* 项目截图
* 操作手册
* 交付资料
* 客户文档

---

# 6. V2扩展规划

后续版本计划新增：

* 用户权限管理
* AI问答助手
* 项目知识库
* 运维经验库
* 需求池
* 操作日志
* 数据库连接检测
* 服务器状态检测

当前V1暂不实现。
