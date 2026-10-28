/****************************************

* 作者： 闪电黑客
* 日期： 2026/10/28

* 描述： YooAsset 配置文件源。按文件名定位 TextAsset，复制 bytes 后释放资源句柄。

*/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;

namespace WorldTree
{
	/// <summary>
	/// YooAsset 文件源（只读）
	/// </summary>
	public sealed class YooAssetFileSource : FileSource
	{
		/// <summary>
		/// 资源包
		/// </summary>
		private readonly ResourcePackage package;

		/// <summary>
		/// 创建 YooAsset 文件源
		/// </summary>
		public YooAssetFileSource(YooAssetManager manager)
		{
			if (manager == null) throw new ArgumentNullException(nameof(manager));
			package = manager.Package ?? throw new InvalidOperationException("YooAsset package 未初始化");
		}

		/// <summary>
		/// 源名称
		/// </summary>
		public override string Name => "YooAsset";

		/// <summary>
		/// 能力
		/// </summary>
		public override FileCapabilities Capabilities => FileCapabilities.Read;

		/// <summary>
		/// 逻辑路径转 YooAsset 地址（当前收集器使用 AddressByFileName）
		/// </summary>
		private static string ToLocation(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new FileException(FileError.InvalidPath, path, "文件路径不能为空");

			return Path.GetFileNameWithoutExtension(path);
		}

		/// <summary>
		/// 是否存在
		/// </summary>
		public override bool Exists(string path)
		{
			return package.CheckLocationValid(ToLocation(path));
		}

		/// <summary>
		/// 文件信息
		/// </summary>
		public override FileItemInfo Stat(string path)
		{
			if (!package.CheckLocationValid(ToLocation(path))) return default;
			return new FileItemInfo(path);
		}

		/// <summary>
		/// 打开文件
		/// </summary>
		public override FileHandle Open(INode parent, string path, FileOpenMode mode, FileOpenAccess access)
		{
			if (parent == null) throw new ArgumentNullException(nameof(parent));
			if (mode != FileOpenMode.Open || access != FileOpenAccess.Read)
				throw new FileException(FileError.NotSupported, path, "YooAsset 文件源只读");

			string location = ToLocation(path);
			AssetHandle assetHandle = package.LoadAssetSync<TextAsset>(location);
			try
			{
				TextAsset textAsset = assetHandle.GetAssetObject<TextAsset>();
				parent.AddTemp(out YooAssetFileHandle handle);
				handle.Bind(textAsset.bytes);
				return handle;
			}
			finally
			{
				assetHandle.Release();
			}
		}

		/// <summary>
		/// 异步打开文件
		/// </summary>
		public override async TreeTask<FileHandle> OpenAsync(INode parent, string path, FileOpenMode mode, FileOpenAccess access)
		{
			if (parent == null) throw new ArgumentNullException(nameof(parent));
			if (mode != FileOpenMode.Open || access != FileOpenAccess.Read)
				throw new FileException(FileError.NotSupported, path, "YooAsset 文件源只读");

			string location = ToLocation(path);
			AssetHandle assetHandle = package.LoadAssetAsync<TextAsset>(location);
			try
			{
				await parent.GetAwaiter(assetHandle.Task);
				TextAsset textAsset = assetHandle.GetAssetObject<TextAsset>();
				parent.AddTemp(out YooAssetFileHandle handle);
				handle.Bind(textAsset.bytes);
				return handle;
			}
			finally
			{
				assetHandle.Release();
			}
		}

		/// <summary>
		/// 不支持目录枚举
		/// </summary>
		public override IEnumerable<FileItemInfo> List(string path)
		{
			throw new FileException(FileError.NotSupported, path, "YooAsset 文件源不支持目录枚举");
		}

		/// <summary>
		/// 不支持删除
		/// </summary>
		public override void Delete(string path)
		{
			throw new FileException(FileError.NotSupported, path, "YooAsset 文件源只读");
		}

		/// <summary>
		/// 不支持移动
		/// </summary>
		public override void Move(string from, string to)
		{
			throw new FileException(FileError.NotSupported, from, "YooAsset 文件源只读");
		}

		/// <summary>
		/// 不支持创建目录
		/// </summary>
		public override void CreateDirectory(string path)
		{
			throw new FileException(FileError.NotSupported, path, "YooAsset 文件源只读");
		}
	}

}
