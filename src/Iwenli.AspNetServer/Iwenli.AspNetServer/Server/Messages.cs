using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;

namespace Iwenli.AspNetServer
{
    /// <summary>
    /// 返回默认消息
    /// </summary>
    internal class Messages
    {
        private const string m_dirListingTail = "</PRE>\r\n <hr width=100% size=1 color=silver>\r\n\r\n {0}</body>\r\n</html>\r\n";
        private const string m_dirListingFormat1 = "<html>\r\n<head>\r\n<title>{0}</title>\r\n";
        private const string m_httpStyle = "<style>\r\n\tbody {font-family:\"Lucida Console\";font-weight:normal;font-size: 10px;color:black;} \r\n \tp {font-family:\"宋体\";font-weight:normal;color:black;margin-top: -5px}\r\n \tb {font-family:\"宋体\";font-weight:bold;color:black;margin-top: -5px}\r\n \th1 { font-family:\"宋体\";font-weight:normal;font-size:18pt;color:red }\r\n \th2 { font-family:\"宋体\";font-weight:normal;font-size:14pt;color:maroon }\r\n \tpre {font-family:\"Lucida Console\";font-size: 12pt}\r\n \t.marker {font-weight: bold; color: black;text-decoration: none;}\r\n \t.version {color: gray;}\r\n \t.error {margin-bottom: 10px;}\r\n \t.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }\r\n </style>\r\n";
        private const string m_dirListingFormat2 = "</head>\r\n <body bgcolor=\"white\">\r\n\r\n <h2> <i>{0}</i> </h2></span>\r\n\r\n <hr width=100% size=1 color=silver>\r\n\r\n<PRE>\r\n";
        private const string m_dirListingParentFormat = "<A href=\"{0}\">[To Parent Directory]</A>\r\n\r\n";
        private const string m_dirListingDirFormat = "{0,38:dddd, MMMM dd, yyyy hh:mm tt} &lt;dir&gt; <A href=\"{1}/\">{2}</A>\r\n";
        private const string m_dirListingFileFormat = "{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} <A href=\"{2}\">{3}</A>\r\n";
        private const string m_httpErrorFormat1 = "<html>\r\n <head>\r\n <title>{0}</title>\r\n";
        private const string m_httpErrorFormat2 = "</head>\r\n<body bgcolor=\"white\">\r\n\r\n<span><h1>{0}<hr width=100% size=1 color=silver></h1>\r\n\r\n<h2> <i>{1}</i> </h2></span>\r\n\r\n <hr width=100% size=1 color=silver>\r\n\r\n {2} \r\n\r\n  \r\n\r\n  </body>\r\n</html>\r\n";

        private static readonly string m_versionString = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static readonly string m_versionInfo = string.Format(CultureInfo.InvariantCulture,
            "Kernel Version：V{0} &nbsp; Development By <a href=\"{1}\" target=\"_black\">{2}</a>",
            m_versionString, "http://blog.iwenli.org", "iwenli");

        public static string VersionString
        {
            get { return m_versionString; }
        }

        public static string FormatDirectoryListing(string dirPath, string parentPath, FileSystemInfo[] elements)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string text = string.Format("目录清单 -- {0}", dirPath);
            string value = string.Format(CultureInfo.InvariantCulture, Messages.m_dirListingTail, m_versionInfo);

            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, m_dirListingFormat1, text));
            stringBuilder.Append(m_httpStyle);
            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, m_dirListingFormat2, text));

            if (parentPath != null)
            {
                if (!parentPath.EndsWith("/", StringComparison.Ordinal))
                {
                    parentPath += "/";
                }
                stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, m_dirListingParentFormat, parentPath));
            }
            if (elements != null)
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    if (elements[i] is FileInfo)
                    {
                        FileInfo fileInfo = (FileInfo)elements[i];
                        stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, m_dirListingFileFormat, fileInfo.LastWriteTime, fileInfo.Length, fileInfo.Name, fileInfo.Name));
                    }
                    else
                    {
                        if (elements[i] is DirectoryInfo)
                        {
                            DirectoryInfo directoryInfo = (DirectoryInfo)elements[i];
                            stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, m_dirListingDirFormat, directoryInfo.LastWriteTime, directoryInfo.Name, directoryInfo.Name));
                        }
                    }
                }
            }
            stringBuilder.Append(value);
            return stringBuilder.ToString();
        }
        public static string FormatErrorMessageBody(int statusCode, string appName)
        {
            string statusDescription = HttpWorkerRequest.GetStatusDescription(statusCode);
            string text = string.Format("服务器出错 发生在 '{0}' Web应用程序中.", appName);
            string text2 = string.Format("HTTP错误 {0} - {1}.", statusCode, statusDescription);
            return string.Format(CultureInfo.InvariantCulture, m_httpErrorFormat1, statusDescription) + m_httpStyle
                + string.Format(CultureInfo.InvariantCulture, Messages.m_httpErrorFormat2, text, text2, m_versionInfo);
        }
    }
}