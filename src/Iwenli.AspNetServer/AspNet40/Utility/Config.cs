namespace AspNet40.Utility
{
    public static class Config
    {
        /// <summary>
        /// 当前版本号
        /// </summary>
        public static readonly string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        /// <summary>
        /// 右键菜单启动路径(带文件名)
        /// </summary>
        public static readonly string MenuStartPaht = string.Format("C:\\Windows\\{0}.EXE",
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
        /// <summary>
        /// 作者
        /// </summary>
        public const string Author = "Iwenli";
        /// <summary>
        /// 标题
        /// </summary>
        public const string Caption = "AspNet网站运行助手20 GUI";
        /// <summary>
        /// 描述
        /// </summary>
        public const string Description = "Run ASP.NET applications locally.";
        /// <summary>
        /// 自定义端口存储文件路径(带文件名)
        /// </summary>
        public const string PortIniPath = "bin\\port.ini";
        /// <summary>
        /// 博客
        /// </summary>
        public const string Blog = "http://blog.iwenli.org";

    }
}
