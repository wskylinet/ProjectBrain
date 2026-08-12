# Project Brain

面向研发、运维人员的项目档案管理平台。当前为 **V1 初始骨架**，已实现登录认证流程。

## 目录结构

```text
ProjectBrain/
├── ProjectBrain.sln   解决方案文件（VS 2022 可直接打开）
├── backend/           后端：.NET 8 WebAPI + SqlSugar + SQL Server
├── frontend/          前端：Vue3 + TypeScript + Vite + Element Plus + Pinia + Axios
└── docs/              产品与架构文档
```

## 技术栈

| 层    | 技术                                                        |
| ----- | ----------------------------------------------------------- |
| 前端  | Vue3、TypeScript、Vite、Element Plus、Pinia、Vue Router、Axios |
| 后端  | .NET 8 WebAPI、SqlSugar、JWT、BCrypt                          |
| 数据库 | SQL Server                                                  |

## 已实现功能

- 后端 JWT 登录接口 `POST /api/auth/login`、当前用户 `GET /api/auth/me`
- SqlSugar CodeFirst 自动建表，启动时写入种子管理员账号
- 统一返回结构（`code == 0` 表示成功）、CORS、Swagger
- 前端登录页、路由守卫、Pinia 鉴权 store、Axios 拦截器、主框架与工作台

> 默认账号：**admin / admin123**（首次登录后请尽快修改）。

## 启动方式

### 1. 后端（backend）

先在 `backend/appsettings.json` 中配置 `ConnectionStrings:Default` 指向你的 SQL Server。

```bash
cd backend
dotnet run
```

> 也可直接用 Visual Studio 2022 打开根目录的 `ProjectBrain.sln`，按 F5 启动。

- 默认地址：`http://localhost:5087`
- Swagger 文档：`http://localhost:5087/swagger`
- 启动时会自动创建数据库与表，但不会创建任何默认账号；若数据库暂不可用，服务仍可启动（仅记录警告）。

首次部署且用户表为空时，手动执行一次初始管理员命令。密码不会写入配置或代码：

```powershell
$env:PROJECTBRAIN_INITIAL_ADMIN_USERNAME = 'admin'
$env:PROJECTBRAIN_INITIAL_ADMIN_PASSWORD = '请替换为至少12位的强密码'
dotnet run -- --init-admin
Remove-Item Env:PROJECTBRAIN_INITIAL_ADMIN_USERNAME
Remove-Item Env:PROJECTBRAIN_INITIAL_ADMIN_PASSWORD
```

该命令仅在用户表完全为空时有效，成功后会立即退出；以后请登录系统，通过用户管理功能创建其他用户。

### 2. 前端（frontend）

```bash
cd frontend
npm install
npm run dev
```

- 默认地址：`http://localhost:5173`
- 开发环境通过 Vite 代理将 `/api` 转发到后端 `http://localhost:5087`（见 `vite.config.ts`）。

## 安全提示

- 生产环境务必通过安全环境变量提供足够长的随机 `Jwt__SecretKey`。
- 数据库连接字符串、密钥等敏感信息不应提交到版本库，建议使用环境变量或用户机密。

生产部署时，`appsettings.json` 不保存数据库连接、JWT 密钥和业务密码加密主密钥。IIS 服务器本地保管并填写
`backend/set-production-env.local.ps1`，使用管理员 PowerShell 将变量写入应用池配置：

```powershell
.\set-production-env.local.ps1 -AppPoolName 'ProjectBrain'
```

脚本会设置 `ASPNETCORE_ENVIRONMENT=Production`、`DOTNET_ENVIRONMENT=Production`、
`ConnectionStrings__Default`、`Jwt__SecretKey` 和 `Encryption__MasterKey`，随后回收指定应用池。它不会把密钥写入
站点发布目录的 `web.config`。`*.local.ps1` 已被 Git 忽略，该脚本需要单独安全传到服务器并限制管理员读取。

## 一键远程桌面

连接信息中的 Windows 远程桌面/RDP 类型支持从网页直接调用系统 `mstsc.exe`，无需重复下载 `.rdp` 文件。使用前需在每台 Windows 电脑上注册一次自定义协议，详见 [一键远程桌面说明](docs/rdp-protocol.md)。
