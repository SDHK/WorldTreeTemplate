/****************************************

* 作者： 闪电黑客
* 日期： 2026/10/28

* 描述： YooAsset 配置内存句柄。只读，持有复制后的 bytes。

*/

using System;
using System.IO;

namespace WorldTree
{
	/// <summary>
	/// YooAsset 配置内存句柄
	/// </summary>
	public sealed class YooAssetFileHandle : FileHandle, TempOf<INode>, AsRule<Awake>
	{
		/// <summary>
		/// 配置字节
		/// </summary>
		private byte[] bytes;

		/// <summary>
		/// 当前读取位置
		/// </summary>
		private int position;

		/// <summary>
		/// 绑定配置字节
		/// </summary>
		public void Bind(byte[] bytes)
		{
			if (this.bytes != null) throw new InvalidOperationException("句柄已绑定");
			this.bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
		}

		/// <summary>
		/// 清空句柄内容
		/// </summary>
		internal void Clear()
		{
			bytes = null;
			position = 0;
		}

		/// <summary>
		/// 配置字节
		/// </summary>
		private byte[] Bytes => bytes ?? throw new InvalidOperationException("句柄未绑定");

		/// <summary>
		/// 总长度
		/// </summary>
		public override long Length => Bytes.Length;

		/// <summary>
		/// 当前读写位置
		/// </summary>
		public override long Position
		{
			get => position;
			set
			{
				if (value < 0 || value > Length) throw new ArgumentOutOfRangeException(nameof(value));
				position = (int)value;
			}
		}

		/// <summary>
		/// 可读
		/// </summary>
		public override bool CanRead => bytes != null;

		/// <summary>
		/// 可写
		/// </summary>
		public override bool CanWrite => false;

		/// <summary>
		/// 可定位
		/// </summary>
		public override bool CanSeek => bytes != null;

		/// <summary>
		/// 读取
		/// </summary>
		public override int Read(byte[] buffer, int offset, int count)
		{
			byte[] sources = Bytes;
			int available = sources.Length - position;
			int read = Math.Min(count, available);
			if (read <= 0) return 0;

			Array.Copy(sources, position, buffer, offset, read);
			position += read;
			return read;
		}

		/// <summary>
		/// 写入（只读）
		/// </summary>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("YooAsset 配置句柄只读");
		}

		/// <summary>
		/// 定位
		/// </summary>
		public override long Seek(long offset, SeekOrigin origin)
		{
			long next;
			switch (origin)
			{
				case SeekOrigin.Begin:
					next = offset;
					break;
				case SeekOrigin.Current:
					next = position + offset;
					break;
				case SeekOrigin.End:
					next = Length + offset;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(origin));
			}

			if (next < 0 || next > Length) throw new ArgumentOutOfRangeException(nameof(offset));
			position = (int)next;
			return position;
		}

		/// <summary>
		/// 刷新缓冲（内存句柄无操作）
		/// </summary>
		public override void Flush() { }
	}

	/// <summary>
	/// YooAsset 配置句柄法则
	/// </summary>
	public static class YooAssetFileHandleRule
	{
		class RemoveRule : RemoveRule<YooAssetFileHandle>
		{
			protected override void Execute(YooAssetFileHandle self)
			{
				self.Clear();
			}
		}
	}
}
