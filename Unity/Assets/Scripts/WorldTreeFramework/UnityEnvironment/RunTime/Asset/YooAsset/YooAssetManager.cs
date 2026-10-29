/****************************************

* 作者： 闪电黑客
* 日期： 2026/10/22

* 描述： YooAsset 资源来源。按地址缓存 AssetEntry，异步加载合并。

*/

using System;
using System.Collections.Generic;
using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace WorldTree
{
	/// <summary>
	/// YooAsset 资源来源
	/// </summary>
	public class YooAssetManager : Node
		, IAssetSource
		, FieldOf<AssetManager>
		, AsChildBranch
		, AsRule<Awake>
		, AsRule<Awake<string>>
	{
		/// <summary>
		/// 默认资源包
		/// </summary>

		/// <summary>
		/// 资源包名
		/// </summary>
		public string PackageName { get; set; } = "DefaultPackage";

		/// <summary>
		/// 默认资源包
		/// </summary>
		public ResourcePackage Package { get; set; }

		/// <summary>
		/// 是否存在地址
		/// </summary>
		public bool Exists(string address)
		{
			return Package != null && Package.CheckLocationValid(address);
		}

		/// <summary>
		/// 地址到资源项
		/// </summary>
		private readonly Dictionary<string, AssetEntry> entryDict = new();

		public Object Load(string address)
		{
			AssetEntry entry = GetOrAddEntry(address);
			if (entry.Asset != null) return entry.Asset;
			if (entry.Handle != null)
				throw new InvalidOperationException($"资源正在异步加载: {address}");

			try
			{
				entry.Handle = Package.LoadAssetSync<Object>(address);
				entry.Asset = entry.Handle.GetAssetObject<Object>();
				return entry.Asset;
			}
			catch
			{
				RemoveEntry(address);
				throw;
			}
		}

		public async TreeTask<Object> LoadAsync(string address)
		{
			AssetEntry entry = GetOrAddEntry(address);
			if (entry.Asset != null) return entry.Asset;

			entry.Handle ??= Package.LoadAssetAsync<Object>(address);
			try
			{
				await this.GetAwaiter(entry.Handle.Task);
				entry.Asset = entry.Handle.GetAssetObject<Object>();
				return entry.Asset;
			}
			catch
			{
				RemoveEntry(address);
				throw;
			}
		}

		public void Release(string address)
		{
			if (!entryDict.TryGetValue(address, out AssetEntry entry)) return;
			if (entry.Handle != null && !entry.Handle.Task.IsCompleted)
				throw new InvalidOperationException($"资源正在异步加载，不能卸载: {address}");

			entryDict.Remove(address);
			entry.Dispose();
		}

		public void ReleaseAll()
		{
			foreach (AssetEntry entry in entryDict.Values)
			{
				entry.Dispose();
			}
			entryDict.Clear();
		}

		/// <summary>
		/// 获取或创建资源项
		/// </summary>
		private AssetEntry GetOrAddEntry(string address)
		{
			if (entryDict.TryGetValue(address, out AssetEntry entry)) return entry;

			entry = this.AddChild(out AssetEntry _);
			entryDict.Add(address, entry);
			return entry;
		}

		/// <summary>
		/// 移除并释放资源项
		/// </summary>
		private void RemoveEntry(string address)
		{
			if (!entryDict.Remove(address, out AssetEntry entry)) return;

			entry.Dispose();
		}
	}

	public static partial class YooAssetManagerRule
	{
		class Awake : AwakeRule<YooAssetManager>
		{
			protected override void Execute(YooAssetManager self)
			{
				self.PackageName = "DefaultPackage";
				self.Package = YooAssets.GetPackage(self.PackageName);
			}
		}

		[NodeRule(nameof(Awake<string>))]
		private static void OnAwakeString(YooAssetManager self, string packageName)
		{
			self.PackageName = packageName;
			self.Package = YooAssets.GetPackage(packageName);
		}

		class Remove : RemoveRule<YooAssetManager>
		{
			protected override void Execute(YooAssetManager self)
			{
				self.ReleaseAll();
				self.Package = null;
			}
		}
	}
}
