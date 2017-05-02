using System;

namespace AspNet40.Server
{
    /// <summary>
    /// 分隔请求内容类
    /// </summary>
	internal sealed class ByteParser
	{
		private byte[] m_bytes;
		private int m_pos;
        /// <summary>
        /// 当前偏移量
        /// </summary>
		internal int CurrentOffset
		{
			get
			{
				return this.m_pos;
			}
		}
		internal ByteParser(byte[] bytes)
		{
			this.m_bytes = bytes;
			this.m_pos = 0;
		}
        /// <summary>
        /// 读取一行内容
        /// </summary>
        /// <returns></returns>
		internal ByteString ReadLine()
		{
			ByteString byteString = null;
			ByteString result;
			for (int i = this.m_pos; i < this.m_bytes.Length; i++)
			{
				if (this.m_bytes[i] == 10)
				{
					int num = i - this.m_pos;
					if (num > 0 && this.m_bytes[i - 1] == 13)
					{
						num--;
					}
					byteString = new ByteString(this.m_bytes, this.m_pos, num);
					this.m_pos = i + 1;
					result = byteString;
					return result;
				}
			}
			if (this.m_pos < this.m_bytes.Length)
			{
				byteString = new ByteString(this.m_bytes, this.m_pos, this.m_bytes.Length - this.m_pos);
			}
			this.m_pos = this.m_bytes.Length;
			result = byteString;
			return result;
		}
	}
}
