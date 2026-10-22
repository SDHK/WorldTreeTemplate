/****************************************

* 作者：闪电黑客
* 日期：2026/10/22

* 描述：YooAsset 资源来源。句柄缓存 + 同步/异步加载。

*/

using System.Collections.Generic;
using UnityEngine;
using YooAsset;

namespace WorldTree
{
	/// <summary>
	/// YooAsset 资源来源
	/// </summary>
	public class YooAssetManager : Node
		, IAssetSource
		, FieldOf<AssetManager>
		, AsRule<Awake>
	{
		/// <summary>
		/// 默认资源包
		/// </summary>
		public ResourcePackage package;

		/// <summary>
		/// 已加载资源句柄缓存（按地址复用）
		/// </summary>
		public Dictionary<string, AssetHandle> handleDict = new();

		public Object Load(string address)
		{
			if (handleDict.TryGetValue(address, out AssetHandle handle))
			{
				return handle.GetAssetObject<Object>();
			}
			handle = package.LoadAssetSync<Object>(address);
			handleDict[address] = handle;
			return handle.GetAssetObject<Object>();
		}

		public async TreeTask<Object> LoadAsync(string address)
		{
			if (handleDict.TryGetValue(address, out AssetHandle handle))
			{
				return handle.GetAssetObject<Object>();
			}
			handle = package.LoadAssetAsync<Object>(address);
			await this.GetAwaiter(handle.Task);
			handleDict[address] = handle;
			return handle.GetAssetObject<Object>();
		}

		public void Release(string address)
		{
			if (!handleDict.Remove(address, out AssetHandle handle)) return;
			handle?.Release();
		}

		public void ReleaseAll()
		{
			foreach (AssetHandle handle in handleDict.Values)
			{
				handle.Release();
			}
			handleDict.Clear();
		}
	}

	public static class YooAssetManagerRule
	{
		class Awake : AwakeRule<YooAssetManager>
		{
			protected override void Execute(YooAssetManager self)
			{
				self.package = YooAssets.GetPackage("DefaultPackage");
			}
		}

		class Remove : RemoveRule<YooAssetManager>
		{
			protected override void Execute(YooAssetManager self)
			{
				self.ReleaseAll();
				self.package = null;
			}
		}
	}
}
