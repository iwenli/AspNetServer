using System;
using System.Collections;
using System.Text;

namespace AspNet20.Server
{
    /// <summary>
    /// 把字节转为字符
    /// </summary>
	internal sealed class ByteString
	{
		private byte[] m_bytes;
		private int m_length;
		private int m_offset;
		public byte[] Bytes
		{
			get
			{
				return this.m_bytes;
			}
		}
		public bool IsEmpty
		{
			get
			{
				return this.m_bytes == null || this.m_length == 0;
			}
		}
		public byte this[int index]
		{
			get
			{
				return this.m_bytes[this.m_offset + index];
			}
		}
		public int Length
		{
			get
			{
				return this.m_length;
			}
		}
		public int Offset
		{
			get
			{
				return this.m_offset;
			}
		}
		public ByteString(byte[] bytes, int offset, int length)
		{
			this.m_bytes = bytes;
			if (this.m_bytes != null && offset >= 0 && length >= 0 && offset + length <= this.m_bytes.Length)
			{
				this.m_offset = offset;
				this.m_length = length;
			}
		}
		public byte[] GetBytes()
		{
			byte[] array = new byte[this.m_length];
			if (this.m_length > 0)
			{
				Buffer.BlockCopy(this.m_bytes, this.m_offset, array, 0, this.m_length);
			}
			return array;
		}
		public string GetString()
		{
			return this.GetString(Encoding.UTF8);
		}
		public string GetString(Encoding enc)
		{
			string result;
			if (this.IsEmpty)
			{
				result = string.Empty;
			}
			else
			{
				result = enc.GetString(this.m_bytes, this.m_offset, this.m_length);
			}
			return result;
		}
		public int IndexOf(char ch)
		{
			return this.IndexOf(ch, 0);
		}
		public int IndexOf(char ch, int offset)
		{
			int result;
			for (int i = offset; i < this.m_length; i++)
			{
				if (this[i] == (byte)ch)
				{
					result = i;
					return result;
				}
			}
			result = -1;
			return result;
		}
		public ByteString[] Split(char sep)
		{
			ArrayList arrayList = new ArrayList();
			int i = 0;
			while (i < this.m_length)
			{
				int num = this.IndexOf(sep, i);
				if (num < 0)
				{
					arrayList.Add(this.Substring(i));
					break;
				}
				arrayList.Add(this.Substring(i, num - i));
				i = num + 1;
				while (i < this.m_length && this[i] == (byte)sep)
				{
					i++;
				}
			}
			int count = arrayList.Count;
			ByteString[] array = new ByteString[count];
			for (int j = 0; j < count; j++)
			{
				array[j] = (ByteString)arrayList[j];
			}
			return array;
		}
		public ByteString Substring(int offset)
		{
			return this.Substring(offset, this.m_length - offset);
		}
		public ByteString Substring(int offset, int len)
		{
			return new ByteString(this.m_bytes, this.m_offset + offset, len);
		}
	}
}
