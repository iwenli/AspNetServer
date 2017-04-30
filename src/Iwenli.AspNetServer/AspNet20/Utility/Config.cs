using Iwenli.Simulateiis.WebHost;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iwenli.Simulateiis.Utility
{
    public static class Config
    {
        public static readonly string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        /// <summary>
        /// 作者
        /// </summary>
        public const string Author = "Iwenli";
        /// <summary>
        /// 标题
        /// </summary>
        public const string Caption = "AspNet网站运行助手";
        /// <summary>
        /// 描述
        /// </summary>
        public const string Description = "Run ASP.NET applications locally.";
        /// <summary>
        /// 自定义端口存储文件路径(带文件名)
        /// </summary>
        public const string PortIniPath = "bin\\port.ini";
        /// <summary>
        /// 右键菜单启动路径(带文件名)
        /// </summary>
        public const string MenuStartPaht = "C:\\Windows\\Iwenli.Simulateiis.EXE";
        /// <summary>
        /// 博客
        /// </summary>
        public const string Blog = "http://blog.iwenli.org";

        /// <summary>
        /// 用法
        /// </summary>
        public static readonly string Usage = "用法：\n" +
                                              "    {0} -port:<port number>  -path:<physical path> -vpath:<virtual path> \n\n" +
                                              "其中：\n" +
                                              "    port number：[可选] 1〜65535之间的未使用端口号.\n" +
                                              "    physical path：Web应用程序为rooted的有效目录名.\n" +
                                              "    virtual path：[可选]虚拟路径或应用程序根<app name>，默认为\"/\"。\n\n" +
                                              "示例：\n" +
                                              "    {1} -port:8080 -path:\"c:\\inetpub\\wwwroot\\MyApp\\\" - vpath:\"/index.htm\"\n\n" +
                                              "您可以访问Web应用程序使用以下形式的网址：\"http://localhost:8080/index.html\"";

    }
}
