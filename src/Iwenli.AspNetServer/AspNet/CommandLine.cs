using System;
using System.Collections;
using System.Collections.Specialized;

namespace AspNet
{
    /// <summary>
    /// 分解命令行参数
    /// </summary>
    public sealed class CommandLine
    {
        private string[] _arguments;
        private IDictionary _options;
        private bool _showHelp;
        public string[] Arguments
        {
            get
            {
                return this._arguments;
            }
        }
        public IDictionary Options
        {
            get
            {
                if (this._options == null)
                {
                    this._options = new HybridDictionary(true);
                }
                return this._options;
            }
        }
        /// <summary>
        /// 是否显示帮助信息
        /// </summary>
        public bool ShowHelp
        {
            get
            {
                return this._showHelp;
            }
        }
        public CommandLine(string[] args)
        {
            ArrayList arrayList = new ArrayList();
            for (int i = 0; i < args.Length; i++)
            {
                char c = args[i][0];
                if (c != '/' && c != '-')
                {
                    arrayList.Add(args[i]);
                }
                else
                {
                    int num = args[i].IndexOf(':');
                    if (num == -1)
                    {
                        string text = args[i].Substring(1);
                        if (string.Compare(text, "help", StringComparison.OrdinalIgnoreCase) == 0 || text.Equals("?"))
                        {
                            this._showHelp = true;
                        }
                        else
                        {
                            this.Options[text] = string.Empty;
                        }
                    }
                    else
                    {
                        this.Options[args[i].Substring(1, num - 1)] = args[i].Substring(num + 1);
                    }
                }
            }
            this._arguments = (string[])arrayList.ToArray(typeof(string));
        }
    }
}
