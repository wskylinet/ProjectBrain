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
- 启动时会自动创建数据库与表并写入管理员账号；若数据库暂不可用，服务仍可启动（仅记录警告）。

### 2. 前端（frontend）

```bash
cd frontend
npm install
npm run dev
```

- 默认地址：`http://localhost:5173`
- 开发环境通过 Vite 代理将 `/api` 转发到后端 `http://localhost:5087`（见 `vite.config.ts`）。

## 安全提示

- 生产环境务必修改 `appsettings.json` 中的 `Jwt:SecretKey` 为足够长的随机值。
- 数据库连接字符串、密钥等敏感信息不应提交到版本库，建议使用环境变量或用户机密。
