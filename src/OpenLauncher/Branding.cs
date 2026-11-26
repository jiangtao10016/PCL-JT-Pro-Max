namespace OpenLauncher
{
    /// <summary>
    /// 换名字、换仓库、换维护者，只改这一个文件。
    /// 任何人 fork 之后改掉这里 + 换成自己的签名公钥，就是一个独立发行版。
    /// </summary>
    public static class Branding
    {
        public const string AppName = "PCL JT Pro Max";
        public const string DisplayName = "PCL JT Pro Max";
        public const string Tagline = "去中心化 Minecraft 启动器";

        /// <summary>主页 / 源码下载（仅用于"关于"页面和 User-Agent，不是更新的唯一来源）</summary>
        public const string RepoUrl = "https://mc.游戏.fun/";

        /// <summary>
        /// 微软正版登录用的 Azure 应用 Client ID。
        /// 需要自己在 Azure 注册应用并向 Mojang 申请 Minecraft API 权限后填入；
        /// 留空时用户可在 设置 → 账户 里填自己的。
        /// </summary>
        public const string DefaultMicrosoftClientId = "";

        /// <summary>CurseForge API Key（可选）。留空时自动走无需 Key 的镜像。</summary>
        public const string DefaultCurseForgeApiKey = "";
    }
}
