# 一键调用 Windows 远程桌面

Project Brain 使用 `projectbrain-rdp://` 自定义协议从连接信息页面调用 Windows 自带的 `mstsc.exe`。此方案不需要安装独立客户端，也不会为每次连接生成或下载 `.rdp` 文件。

## 首次安装

1. 打开项目详情，点击右上角“远程工具”，选择“下载初始化脚本”。
2. 浏览器会下载 `Install-ProjectBrainRdpProtocol.cmd`。
3. 双击该文件，等待窗口显示安装成功，然后按任意键关闭。
4. 返回网页，在“Windows 远程桌面”或 RDP 类型的连接上点击“一键远程”。
5. 浏览器首次询问是否打开外部应用时请选择允许；可按浏览器提示记住选择。

浏览器或 Windows 可能对下载的脚本显示安全确认，需要用户允许执行。注册信息只写入当前用户的 `HKCU`，不要求管理员权限。

安装后，协议处理脚本位于：

```text
%LOCALAPPDATA%\ProjectBrain\RdpProtocol
```

开发或运维人员也可以从仓库手动安装：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\rdp\Install-ProjectBrainRdpProtocol.ps1
```

## 安全说明

- 协议链接只包含服务器地址和端口，不包含用户名或密码。
- 本地处理脚本只接受主机名、IPv4/IPv6 地址以及 1–65535 范围内的端口。
- 处理脚本使用参数数组启动系统 `mstsc.exe`，不会把链接内容交给命令解释器执行。
- 远程桌面凭据仍由 Windows 远程桌面窗口获取和管理。

## 卸载

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\rdp\Uninstall-ProjectBrainRdpProtocol.ps1
```
