<p align="center"><img src="src/OpenLauncher/Assets/icon.png" width="96" style="image-rendering:pixelated"></p>

<h1 align="center">PCL JT Pro Max</h1>
<p align="center">由蒋涛发明 · 去中心化的 Minecraft 启动器 · 无自动更新 · GPL-3.0</p>

<p align="center">
  <a href="https://mc.游戏.fun/">官网 mc.游戏.fun</a> ·
  <a href="https://github.com/jiangtao10016/PCL-JT-Pro-Max">GitHub 仓库</a> ·
  运营方：蒋涛科技技术有限公司
</p>

---

**由蒋涛发明。** 本项目是一个没有中心服务器的 Minecraft 启动器：游戏、加载器、Mod 全部直接从官方源和公益镜像下载，版本列表实时获取，启动器不内置任何版本号。

> **关于更新**：本启动器**不内置自动更新**。最新版请到官网 <https://mc.游戏.fun/> 手动下载。不收集任何数据；账号令牌用 Windows DPAPI 加密，只存在你自己的电脑上。

## 功能

| 模块 | 功能 |
|---|---|
| 启动 | 一键启动；自动补全缺失文件；自动选择 / 下载合适的 Java；自动分配内存；版本隔离；自定义窗口标题、窗口大小、全屏；启动后自动进服；启动后最小化 / 隐藏 / 关闭；实时游戏日志；导出启动脚本 |
| 账号 | 离线账号；微软正版（设备代码登录，Client ID 可配置）；外置登录（authlib-injector，LittleSkin / Blessing Skin 等）；皮肤头像 |
| 下载游戏 | 正式版 / 快照 / 远古版 / 愚人节版；Forge、NeoForge、Fabric（可同时装 Fabric API）、Quilt；自己执行 Forge 安装器的处理器，下载可走镜像 |
| 社区资源 | Modrinth + CurseForge 双源搜索 Mod / 整合包 / 资源包 / 光影包 / 数据包；常见中文名搜索；自动补齐前置 Mod；按版本和加载器筛选 |
| 整合包 | 导入 Modrinth（.mrpack）和 CurseForge（.zip）整合包；导出为 .mrpack |
| 管理 | 多个 .minecraft 目录；版本重命名 / 删除（进回收站）/ 描述；Mod 启用禁用、批量检查更新、一键更新；资源包 / 光影 / 存档 / 截图管理，支持拖放安装 |
| 崩溃分析 | 识别内存不足、Java 版本不对、缺前置、重复 Mod、Mixin 冲突、显卡驱动等常见原因并给建议；一键导出错误报告 |
| 下载 | 多线程、断点换源、全部文件校验；BMCLAPI / MCIM 镜像与官方源错峰竞速，自动避开连不上的源；可自定义镜像；限速 |
| 个性化 | 主题色、背景图 |

## 下载使用

从官网 <https://mc.游戏.fun/> 下载 `PCL JT Pro Max.zip`，解压到一个空文件夹，双击根目录的 `PCL JT Pro Max.exe` 即可（Windows 10 / 11）。

程序采用「根目录启动器 + `runtime\` 运行时」的双层结构：所有 DLL 和依赖都在 `runtime\` 子目录里，根目录保持干净。

游戏文件默认放在 `runtime\.minecraft`，启动器配置放在 `runtime\PCL JT Pro Max` 文件夹里——整个文件夹拷走就能在别的电脑上用。也可以在 **版本选择 → 添加已有的游戏目录** 里直接使用官方启动器、PCL、HMCL 的 `.minecraft`。

## 自己编译

只需要 [.NET SDK](https://dotnet.microsoft.com/download)（8 或更新），不需要 Visual Studio：

```powershell
.\build.ps1        # 输出 dist\PCL JT Pro Max.exe（及配套 runtime\）
```

如需改名字、换主页、换运营方信息，只改 `src/OpenLauncher/Branding.cs` 一个文件。

微软正版登录默认不内置 Client ID（避免所有用户共用一个开关）；需要正版登录时，可在 `Branding.cs` 里填自己的 Client ID，或在 设置 → 账户 里填。申请方法见 [docs/微软登录.md](docs/微软登录.md)。

## 项目结构

```
src/OpenLauncher/
  Core/       JSON、HTTP、镜像表、多线程下载器、设置、任务
  Game/       版本 JSON 合并、安装器（原版 / Fabric / Quilt / Forge / NeoForge）、启动参数、Java 管理、崩溃分析
  Accounts/   离线 / 微软 / 外置登录、authlib-injector、皮肤
  Content/    Modrinth / CurseForge、本地 Mod 管理、整合包导入导出
  UI/         WPF 界面、用户协议、主题
build.ps1            本地一键编译脚本
docs/                维护文档
```

## 贡献

欢迎 Issue 和 Pull Request：报 bug 请附启动器日志（`runtime\PCL JT Pro Max\logs\launcher.log`）；提功能请先开 Issue 讨论。Fork 后改 `Branding.cs` 即可做自己的发行版。

## 声明

- 本项目由**蒋涛**发明，运营方为**蒋涛科技技术有限公司**，官网 <https://mc.游戏.fun/>。
- 本项目与 Mojang、Microsoft 无关，Minecraft 是 Mojang AB 的商标。
- 本项目从零编写，没有使用 PCL 的代码与美术资源；功能设计参考了 PCL、HMCL 等国内启动器前辈，向他们致敬。
- 镜像服务由 bangbang93（BMCLAPI）、MCIM 等社区成员公益提供，请合理使用。
- 本项目以 GPL-3.0 开源，详见 [LICENSE](LICENSE)。
