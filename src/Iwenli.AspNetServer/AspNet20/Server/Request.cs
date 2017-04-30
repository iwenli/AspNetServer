using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace AspNet20.Server
{
    internal sealed class Request : SimpleWorkerRequest
    {
        private string m_allRawHeaders;
        private Connection m_connection;
        private IStackWalk m_connectionPermission;
        private int m_contentLength;
        private int m_endHeadersOffset;
        private string m_filePath;
        private byte[] m_headerBytes;
        private ArrayList m_headerByteStrings;
        private bool m_headersSent;
        private Host m_host;
        private bool m_isClientScriptPath;
        private string[] m_knownRequestHeaders;
        private string m_path;
        private string m_pathInfo;
        private string m_pathTranslated;
        private byte[] m_preloadedContent;
        private int m_preloadedContentLength;
        private string m_prot;
        private string m_queryString;
        private byte[] m_queryStringBytes;
        private ArrayList m_responseBodyBytes;
        private StringBuilder m_responseHeadersBuilder;
        private int m_responseStatus;
        private bool m_specialCaseStaticFileHeaders;
        private int m_startHeadersOffset;
        private string[][] m_unknownRequestHeaders;
        private string m_url;
        private string m_verb;
        private static char[] m_badPathChars = new char[] { '%', '>', '<', ':', '\\' };
        private static string[] m_defaultFileNames = new string[] {
            "default.aspx",
            "default.htm",
            "default.html",
            "index.aspx",
            "index.htm",
            "index.html"
        };
        private static char[] m_intToHex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        private static string[] m_restrictedDirs = new string[] {
            "/bin",
            "/app_browsers",
            "/app_code",
            "/app_data",
            "/app_localresources",
            "/app_globalresources",
            "/app_webreferences"
        };

        private const int MaxChunkLength = 0x10000;
        private const int maxHeaderBytes = 0x8000;

        public Request(Host host, Connection connection) : base(string.Empty, string.Empty, null)
        {
            this.m_connectionPermission = new PermissionSet(PermissionState.Unrestricted);
            this.m_host = host;
            this.m_connection = connection;
        }

        public override void CloseConnection()
        {
            this.m_connectionPermission.Assert();
            this.m_connection.Close();
        }

        public override void EndOfRequest()
        {
        }

        public override void FlushResponse(bool finalFlush)
        {
            if ((((this.m_responseStatus != 0x194) || this.m_headersSent) || (!finalFlush || (this.m_verb != "GET"))) || !this.ProcessDirectoryListingRequest())
            {
                this.m_connectionPermission.Assert();
                if (!this.m_headersSent)
                {
                    this.m_connection.WriteHeaders(this.m_responseStatus, this.m_responseHeadersBuilder.ToString());
                    this.m_headersSent = true;
                }
                for (int i = 0; i < this.m_responseBodyBytes.Count; i++)
                {
                    byte[] data = (byte[])this.m_responseBodyBytes[i];
                    this.m_connection.WriteBody(data, 0, data.Length);
                }
                this.m_responseBodyBytes = new ArrayList();
                if (finalFlush)
                {
                    this.m_connection.Close();
                }
            }
        }

        public override string GetAppPath()
        {
            return this.m_host.VirtualPath;
        }

        public override string GetAppPathTranslated()
        {
            return this.m_host.PhysicalPath;
        }

        public override string GetFilePath()
        {
            return this.m_filePath;
        }

        public override string GetFilePathTranslated()
        {
            return this.m_pathTranslated;
        }

        public override string GetHttpVerbName()
        {
            return this.m_verb;
        }

        public override string GetHttpVersion()
        {
            return this.m_prot;
        }

        public override string GetKnownRequestHeader(int index)
        {
            return this.m_knownRequestHeaders[index];
        }

        public override string GetLocalAddress()
        {
            this.m_connectionPermission.Assert();
            return this.m_connection.LocalIP;
        }

        public override int GetLocalPort()
        {
            return this.m_host.Port;
        }

        public override string GetPathInfo()
        {
            return this.m_pathInfo;
        }

        public override byte[] GetPreloadedEntityBody()
        {
            return this.m_preloadedContent;
        }

        public override string GetQueryString()
        {
            return this.m_queryString;
        }

        public override byte[] GetQueryStringRawBytes()
        {
            return this.m_queryStringBytes;
        }

        public override string GetRawUrl()
        {
            return this.m_url;
        }

        public override string GetRemoteAddress()
        {
            this.m_connectionPermission.Assert();
            return this.m_connection.RemoteIP;
        }

        public override int GetRemotePort()
        {
            return 0;
        }

        public override string GetServerName()
        {
            string localAddress = this.GetLocalAddress();
            if ((!localAddress.Equals("127.0.0.1") && !localAddress.Equals("::1")) && !localAddress.Equals("::ffff:127.0.0.1"))
            {
                return localAddress;
            }
            return "localhost";
        }

        public override string GetServerVariable(string name)
        {
            string processUser = string.Empty;
            string str2 = name;
            if (str2 == null)
            {
                return processUser;
            }
            if (!(str2 == "ALL_RAW"))
            {
                if (str2 != "SERVER_PROTOCOL")
                {
                    if (str2 == "LOGON_USER")
                    {
                        if (this.GetUserToken() != IntPtr.Zero)
                        {
                            processUser = this.m_host.GetProcessUser();
                        }
                        return processUser;
                    }
                    if ((str2 == "AUTH_TYPE") && (this.GetUserToken() != IntPtr.Zero))
                    {
                        processUser = "NTLM";
                    }
                    return processUser;
                }
            }
            else
            {
                return this.m_allRawHeaders;
            }
            return this.m_prot;
        }

        public override string GetUnknownRequestHeader(string name)
        {
            int length = this.m_unknownRequestHeaders.Length;
            for (int i = 0; i < length; i++)
            {
                if (string.Compare(name, this.m_unknownRequestHeaders[i][0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return this.m_unknownRequestHeaders[i][1];
                }
            }
            return null;
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            return this.m_unknownRequestHeaders;
        }

        public override string GetUriPath()
        {
            return this.m_path;
        }

        public override IntPtr GetUserToken()
        {
            return this.m_host.GetProcessToken();
        }

        public override bool HeadersSent()
        {
            return this.m_headersSent;
        }

        private bool IsBadPath()
        {
            return ((this.m_path.IndexOfAny(m_badPathChars) >= 0) || ((CultureInfo.InvariantCulture.CompareInfo.IndexOf(this.m_path, "..", CompareOptions.Ordinal) >= 0) || (CultureInfo.InvariantCulture.CompareInfo.IndexOf(this.m_path, "//", CompareOptions.Ordinal) >= 0)));
        }

        public override bool IsClientConnected()
        {
            this.m_connectionPermission.Assert();
            return this.m_connection.Connected;
        }

        public override bool IsEntireEntityBodyIsPreloaded()
        {
            return (this.m_contentLength == this.m_preloadedContentLength);
        }

        private bool IsRequestForRestrictedDirectory()
        {
            string str = CultureInfo.InvariantCulture.TextInfo.ToLower(this.m_path);
            if (this.m_host.VirtualPath != "/")
            {
                str = str.Substring(this.m_host.VirtualPath.Length);
            }
            foreach (string str2 in m_restrictedDirs)
            {
                if (str.StartsWith(str2, StringComparison.Ordinal) && ((str.Length == str2.Length) || (str[str2.Length] == '/')))
                {
                    return true;
                }
            }
            return false;
        }

        public override string MapPath(string path)
        {
            string physicalPath = string.Empty;
            bool isClientScriptPath = false;
            if (((path == null) || (path.Length == 0)) || path.Equals("/"))
            {
                if (this.m_host.VirtualPath == "/")
                {
                    physicalPath = this.m_host.PhysicalPath;
                }
                else
                {
                    physicalPath = Environment.SystemDirectory;
                }
            }
            else if (this.m_host.IsVirtualPathAppPath(path))
            {
                physicalPath = this.m_host.PhysicalPath;
            }
            else if (this.m_host.IsVirtualPathInApp(path, out isClientScriptPath))
            {
                if (isClientScriptPath)
                {
                    physicalPath = this.m_host.PhysicalClientScriptPath + path.Substring(this.m_host.NormalizedClientScriptPath.Length);
                }
                else
                {
                    physicalPath = this.m_host.PhysicalPath + path.Substring(this.m_host.NormalizedVirtualPath.Length);
                }
            }
            else if (path.StartsWith("/", StringComparison.Ordinal))
            {
                physicalPath = this.m_host.PhysicalPath + path.Substring(1);
            }
            else
            {
                physicalPath = this.m_host.PhysicalPath + path;
            }
            physicalPath = physicalPath.Replace('/', '\\');
            if (physicalPath.EndsWith(@"\", StringComparison.Ordinal) && !physicalPath.EndsWith(@":\", StringComparison.Ordinal))
            {
                physicalPath = physicalPath.Substring(0, physicalPath.Length - 1);
            }
            return physicalPath;
        }

        private void ParseHeaders()
        {
            this.m_knownRequestHeaders = new string[40];
            ArrayList list = new ArrayList();
            for (int i = 1; i < this.m_headerByteStrings.Count; i++)
            {
                string str = ((ByteString)this.m_headerByteStrings[i]).GetString();
                int index = str.IndexOf(':');
                if (index >= 0)
                {
                    string header = str.Substring(0, index).Trim();
                    string str3 = str.Substring(index + 1).Trim();
                    int knownRequestHeaderIndex = HttpWorkerRequest.GetKnownRequestHeaderIndex(header);
                    if (knownRequestHeaderIndex >= 0)
                    {
                        this.m_knownRequestHeaders[knownRequestHeaderIndex] = str3;
                    }
                    else
                    {
                        list.Add(header);
                        list.Add(str3);
                    }
                }
            }
            int num4 = list.Count / 2;
            this.m_unknownRequestHeaders = new string[num4][];
            int num5 = 0;
            for (int j = 0; j < num4; j++)
            {
                this.m_unknownRequestHeaders[j] = new string[] { (string)list[num5++], (string)list[num5++] };
            }
            if (this.m_headerByteStrings.Count > 1)
            {
                this.m_allRawHeaders = Encoding.UTF8.GetString(this.m_headerBytes, this.m_startHeadersOffset, this.m_endHeadersOffset - this.m_startHeadersOffset);
            }
            else
            {
                this.m_allRawHeaders = string.Empty;
            }
        }

        private void ParsePostedContent()
        {
            this.m_contentLength = 0;
            this.m_preloadedContentLength = 0;
            string s = this.m_knownRequestHeaders[11];
            if (s != null)
            {
                try
                {
                    this.m_contentLength = int.Parse(s, CultureInfo.InvariantCulture);
                }
                catch
                {
                }
            }
            if (this.m_headerBytes.Length > this.m_endHeadersOffset)
            {
                this.m_preloadedContentLength = this.m_headerBytes.Length - this.m_endHeadersOffset;
                if (this.m_preloadedContentLength > this.m_contentLength)
                {
                    this.m_preloadedContentLength = this.m_contentLength;
                }
                if (this.m_preloadedContentLength > 0)
                {
                    this.m_preloadedContent = new byte[this.m_preloadedContentLength];
                    Buffer.BlockCopy(this.m_headerBytes, this.m_endHeadersOffset, this.m_preloadedContent, 0, this.m_preloadedContentLength);
                }
            }
        }

        private void ParseRequestLine()
        {
            ByteString[] strArray = ((ByteString)this.m_headerByteStrings[0]).Split(' ');
            if (((strArray == null) || (strArray.Length < 2)) || (strArray.Length > 3))
            {
                this.m_connection.WriteErrorAndClose(400);
            }
            else
            {
                this.m_verb = strArray[0].GetString();
                ByteString str2 = strArray[1];
                this.m_url = str2.GetString();
                if (this.m_url.IndexOf((char)0xfffd) >= 0)
                {
                    this.m_url = str2.GetString(Encoding.Default);
                }
                if (strArray.Length == 3)
                {
                    this.m_prot = strArray[2].GetString();
                }
                else
                {
                    this.m_prot = "HTTP/1.0";
                }
                int index = str2.IndexOf('?');
                if (index > 0)
                {
                    this.m_queryStringBytes = str2.Substring(index + 1).GetBytes();
                }
                else
                {
                    this.m_queryStringBytes = new byte[0];
                }
                index = this.m_url.IndexOf('?');
                if (index > 0)
                {
                    this.m_path = this.m_url.Substring(0, index);
                    this.m_queryString = this.m_url.Substring(index + 1);
                }
                else
                {
                    this.m_path = this.m_url;
                    this.m_queryString = string.Empty;
                }
                if (this.m_path.IndexOf('%') >= 0)
                {
                    this.m_path = HttpUtility.UrlDecode(this.m_path, Encoding.UTF8);
                    index = this.m_url.IndexOf('?');
                    if (index >= 0)
                    {
                        this.m_url = this.m_path + this.m_url.Substring(index);
                    }
                    else
                    {
                        this.m_url = this.m_path;
                    }
                }
                int startIndex = this.m_path.LastIndexOf('.');
                int num3 = this.m_path.LastIndexOf('/');
                if (((startIndex >= 0) && (num3 >= 0)) && (startIndex < num3))
                {
                    int length = this.m_path.IndexOf('/', startIndex);
                    this.m_filePath = this.m_path.Substring(0, length);
                    this.m_pathInfo = this.m_path.Substring(length);
                }
                else
                {
                    this.m_filePath = this.m_path;
                    this.m_pathInfo = string.Empty;
                }
                this.m_pathTranslated = this.MapPath(this.m_filePath);
            }
        }

        private void PrepareResponse()
        {
            this.m_headersSent = false;
            this.m_responseStatus = 200;
            this.m_responseHeadersBuilder = new StringBuilder();
            this.m_responseBodyBytes = new ArrayList();
        }

        [AspNetHostingPermission(SecurityAction.Assert, Level = AspNetHostingPermissionLevel.Medium)]
        public void Process()
        {
            if (this.TryParseRequest())
            {
                if (((this.m_verb == "POST") && (this.m_contentLength > 0)) && (this.m_preloadedContentLength < this.m_contentLength))
                {
                    this.m_connection.Write100Continue();
                }
                if (!this.m_host.RequireAuthentication || this.TryNtlmAuthenticate())
                {
                    if (this.m_isClientScriptPath)
                    {
                        this.m_connection.WriteEntireResponseFromFile(this.m_host.PhysicalClientScriptPath + this.m_path.Substring(this.m_host.NormalizedClientScriptPath.Length), false);
                    }
                    else if (this.IsRequestForRestrictedDirectory())
                    {
                        this.m_connection.WriteErrorAndClose(0x193);
                    }
                    else if (!this.ProcessDefaultDocumentRequest())
                    {
                        this.PrepareResponse();
                        HttpRuntime.ProcessRequest(this);
                    }
                }
            }
        }

        private bool ProcessDefaultDocumentRequest()
        {
            if (this.m_verb == "GET")
            {
                string path = this.m_pathTranslated;
                if (this.m_pathInfo.Length > 0)
                {
                    path = this.MapPath(this.m_path);
                }
                if (!Directory.Exists(path))
                {
                    return false;
                }
                if (!this.m_path.EndsWith("/", StringComparison.Ordinal))
                {
                    string str2 = this.m_path + "/";
                    string extraHeaders = "Location: " + UrlEncodeRedirect(str2) + "\r\n";
                    string body = "<html><head><title>Object moved</title></head><body>\r\n<h2>Object moved to <a href='" + str2 + "'>here</a>.</h2>\r\n</body></html>\r\n";
                    this.m_connection.WriteEntireResponseFromString(0x12e, extraHeaders, body, false);
                    return true;
                }
                foreach (string str5 in m_defaultFileNames)
                {
                    string str6 = path + @"\" + str5;
                    if (File.Exists(str6))
                    {
                        this.m_path = this.m_path + str5;
                        this.m_filePath = this.m_path;
                        this.m_url = (this.m_queryString != null) ? (this.m_path + "?" + this.m_queryString) : this.m_path;
                        this.m_pathTranslated = str6;
                        return false;
                    }
                }
            }
            return false;
        }

        private bool ProcessDirectoryListingRequest()
        {
            if (this.m_verb != "GET")
            {
                return false;
            }
            string path = this.m_pathTranslated;
            if (this.m_pathInfo.Length > 0)
            {
                path = this.MapPath(this.m_path);
            }
            if (!Directory.Exists(path))
            {
                return false;
            }
            if (this.m_host.DisableDirectoryListing)
            {
                return false;
            }
            FileSystemInfo[] elements = null;
            try
            {
                elements = new DirectoryInfo(path).GetFileSystemInfos();
            }
            catch
            {
            }
            string str2 = null;
            if (this.m_path.Length > 1)
            {
                int length = this.m_path.LastIndexOf('/', this.m_path.Length - 2);
                str2 = (length > 0) ? this.m_path.Substring(0, length) : "/";
                if (!this.m_host.IsVirtualPathInApp(str2))
                {
                    str2 = null;
                }
            }
            this.m_connection.WriteEntireResponseFromString(200, "Content-type: text/html; charset=utf-8\r\n", Messages.FormatDirectoryListing(this.m_path, str2, elements), false);
            return true;
        }

        private void ReadAllHeaders()
        {
            this.m_headerBytes = null;
            do
            {
                if (!this.TryReadAllHeaders())
                {
                    return;
                }
            }
            while (this.m_endHeadersOffset < 0);
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            int count = 0;
            this.m_connectionPermission.Assert();
            byte[] src = this.m_connection.ReadRequestBytes(size);
            if ((src != null) && (src.Length > 0))
            {
                count = src.Length;
                Buffer.BlockCopy(src, 0, buffer, 0, count);
            }
            return count;
        }

        private void Reset()
        {
            this.m_headerBytes = null;
            this.m_startHeadersOffset = 0;
            this.m_endHeadersOffset = 0;
            this.m_headerByteStrings = null;
            this.m_isClientScriptPath = false;
            this.m_verb = null;
            this.m_url = null;
            this.m_prot = null;
            this.m_path = null;
            this.m_filePath = null;
            this.m_pathInfo = null;
            this.m_pathTranslated = null;
            this.m_queryString = null;
            this.m_queryStringBytes = null;
            this.m_contentLength = 0;
            this.m_preloadedContentLength = 0;
            this.m_preloadedContent = null;
            this.m_allRawHeaders = null;
            this.m_unknownRequestHeaders = null;
            this.m_knownRequestHeaders = null;
            this.m_specialCaseStaticFileHeaders = false;
        }

        public override void SendCalculatedContentLength(int contentLength)
        {
            if (!this.m_headersSent)
            {
                this.m_responseHeadersBuilder.Append("Content-Length: ");
                this.m_responseHeadersBuilder.Append(contentLength.ToString(CultureInfo.InvariantCulture));
                this.m_responseHeadersBuilder.Append("\r\n");
            }
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            if (!this.m_headersSent)
            {
                switch (index)
                {
                    case 1:
                    case 2:
                    case 0x1a:
                        return;

                    case 0x12:
                    case 0x13:
                        if (!this.m_specialCaseStaticFileHeaders)
                        {
                            break;
                        }
                        return;

                    case 20:
                        if (!(value == "bytes"))
                        {
                            break;
                        }
                        this.m_specialCaseStaticFileHeaders = true;
                        return;
                }
                this.m_responseHeadersBuilder.Append(HttpWorkerRequest.GetKnownResponseHeaderName(index));
                this.m_responseHeadersBuilder.Append(": ");
                this.m_responseHeadersBuilder.Append(value);
                this.m_responseHeadersBuilder.Append("\r\n");
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            if (length != 0)
            {
                FileStream f = null;
                try
                {
                    SafeFileHandle handle2 = new SafeFileHandle(handle, false);
                    f = new FileStream(handle2, FileAccess.Read);
                    this.SendResponseFromFileStream(f, offset, length);
                }
                finally
                {
                    if (f != null)
                    {
                        f.Close();
                        f = null;
                    }
                }
            }
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            if (length != 0)
            {
                FileStream f = null;
                try
                {
                    f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    this.SendResponseFromFileStream(f, offset, length);
                }
                finally
                {
                    if (f != null)
                    {
                        f.Close();
                    }
                }
            }
        }

        private void SendResponseFromFileStream(FileStream f, long offset, long length)
        {
            long num = f.Length;
            if (length == -1)
            {
                length = num - offset;
            }
            if (((length != 0) && (offset >= 0)) && (length <= (num - offset)))
            {
                if (offset > 0)
                {
                    f.Seek(offset, SeekOrigin.Begin);
                }
                if (length <= 0x10000)
                {
                    byte[] buffer = new byte[(int)length];
                    int num2 = f.Read(buffer, 0, (int)length);
                    this.SendResponseFromMemory(buffer, num2);
                }
                else
                {
                    byte[] buffer2 = new byte[0x10000];
                    int num3 = (int)length;
                    while (num3 > 0)
                    {
                        int count = (num3 < 0x10000) ? num3 : 0x10000;
                        int num5 = f.Read(buffer2, 0, count);
                        this.SendResponseFromMemory(buffer2, num5);
                        num3 -= num5;
                        if ((num3 > 0) && (num5 > 0))
                        {
                            this.FlushResponse(false);
                        }
                    }
                }
            }
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            if (length > 0)
            {
                byte[] dst = new byte[length];
                Buffer.BlockCopy(data, 0, dst, 0, length);
                this.m_responseBodyBytes.Add(dst);
            }
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            this.m_responseStatus = statusCode;
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            if (!this.m_headersSent)
            {
                this.m_responseHeadersBuilder.Append(name);
                this.m_responseHeadersBuilder.Append(": ");
                this.m_responseHeadersBuilder.Append(value);
                this.m_responseHeadersBuilder.Append("\r\n");
            }
        }

        private void SkipAllPostedContent()
        {
            if ((this.m_contentLength > 0) && (this.m_preloadedContentLength < this.m_contentLength))
            {
                byte[] buffer;
                for (int i = this.m_contentLength - this.m_preloadedContentLength; i > 0; i -= buffer.Length)
                {
                    buffer = this.m_connection.ReadRequestBytes(i);
                    if ((buffer == null) || (buffer.Length == 0))
                    {
                        return;
                    }
                }
            }
        }

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode = true), SecurityPermission(SecurityAction.Assert, ControlPrincipal = true)]
        private bool TryNtlmAuthenticate()
        {
            try
            {
                using (NtlmAuth auth = new NtlmAuth())
                {
                    do
                    {
                        string blobString = null;
                        string extraHeaders = this.m_knownRequestHeaders[0x18];
                        if ((extraHeaders != null) && extraHeaders.StartsWith("NTLM ", StringComparison.Ordinal))
                        {
                            blobString = extraHeaders.Substring(5);
                        }
                        if (blobString != null)
                        {
                            if (!auth.Authenticate(blobString))
                            {
                                this.m_connection.WriteErrorAndClose(0x193);
                                return false;
                            }
                            if (auth.Completed)
                            {
                                goto Label_009A;
                            }
                            extraHeaders = "WWW-Authenticate: NTLM " + auth.Blob + "\r\n";
                        }
                        else
                        {
                            extraHeaders = "WWW-Authenticate: NTLM\r\n";
                        }
                        this.SkipAllPostedContent();
                        this.m_connection.WriteErrorWithExtraHeadersAndKeepAlive(0x191, extraHeaders);
                    }
                    while (this.TryParseRequest());
                    return false;
                    Label_009A:
                    if (this.m_host.GetProcessSID() != auth.SID)
                    {
                        this.m_connection.WriteErrorAndClose(0x193);
                        return false;
                    }
                }
            }
            catch
            {
                try
                {
                    this.m_connection.WriteErrorAndClose(500);
                }
                catch
                {
                }
                return false;
            }
            return true;
        }

        private bool TryParseRequest()
        {
            this.Reset();
            this.ReadAllHeaders();
            //if (!this._connection.IsLocal)
            //{
            //    this._connection.WriteErrorAndClose(0x193);
            //    return false;
            //}
            if (((this.m_headerBytes == null) || (this.m_endHeadersOffset < 0)) || ((this.m_headerByteStrings == null) || (this.m_headerByteStrings.Count == 0)))
            {
                this.m_connection.WriteErrorAndClose(400);
                return false;
            }
            this.ParseRequestLine();
            if (this.IsBadPath())
            {
                this.m_connection.WriteErrorAndClose(400);
                return false;
            }
            if (!this.m_host.IsVirtualPathInApp(this.m_path, out this.m_isClientScriptPath))
            {
                this.m_connection.WriteErrorAndClose(0x194);
                return false;
            }
            this.ParseHeaders();
            this.ParsePostedContent();
            return true;
        }

        private bool TryReadAllHeaders()
        {
            byte[] src = this.m_connection.ReadRequestBytes(0x8000);
            if ((src == null) || (src.Length == 0))
            {
                return false;
            }
            if (this.m_headerBytes != null)
            {
                int num = src.Length + this.m_headerBytes.Length;
                if (num > 0x8000)
                {
                    return false;
                }
                byte[] dst = new byte[num];
                Buffer.BlockCopy(this.m_headerBytes, 0, dst, 0, this.m_headerBytes.Length);
                Buffer.BlockCopy(src, 0, dst, this.m_headerBytes.Length, src.Length);
                this.m_headerBytes = dst;
            }
            else
            {
                this.m_headerBytes = src;
            }
            this.m_startHeadersOffset = -1;
            this.m_endHeadersOffset = -1;
            this.m_headerByteStrings = new ArrayList();
            ByteParser parser = new ByteParser(this.m_headerBytes);
            while (true)
            {
                ByteString str = parser.ReadLine();
                if (str == null)
                {
                    break;
                }
                if (this.m_startHeadersOffset < 0)
                {
                    this.m_startHeadersOffset = parser.CurrentOffset;
                }
                if (str.IsEmpty)
                {
                    this.m_endHeadersOffset = parser.CurrentOffset;
                    break;
                }
                this.m_headerByteStrings.Add(str);
            }
            return true;
        }

        private static string UrlEncodeRedirect(string path)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(path);
            int length = bytes.Length;
            int num2 = 0;
            for (int i = 0; i < length; i++)
            {
                if ((bytes[i] & 0x80) != 0)
                {
                    num2++;
                }
            }
            if (num2 > 0)
            {
                byte[] buffer2 = new byte[length + (num2 * 2)];
                int num4 = 0;
                for (int j = 0; j < length; j++)
                {
                    byte num6 = bytes[j];
                    if ((num6 & 0x80) == 0)
                    {
                        buffer2[num4++] = num6;
                    }
                    else
                    {
                        buffer2[num4++] = 0x25;
                        buffer2[num4++] = (byte)m_intToHex[(num6 >> 4) & 15];
                        buffer2[num4++] = (byte)m_intToHex[num6 & 15];
                    }
                }
                path = Encoding.ASCII.GetString(buffer2);
            }
            if (path.IndexOf(' ') >= 0)
            {
                path = path.Replace(" ", "%20");
            }
            return path;
        }
    }
}
