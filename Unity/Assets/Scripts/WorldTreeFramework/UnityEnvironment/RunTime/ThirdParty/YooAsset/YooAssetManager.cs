/****************************************

* 作者：闪电黑客
* 日期：2026/10/22

* 描述：YooAsset 资源管理器。挂 WorldLine，封装默认包引用 + 资源加载 + 句柄缓存。

*/

using System.Collections.Generic;
using YooAsset;

namespace WorldTree
{
	/// <summary>
	/// YooAsset 资源管理器
	/// </summary>
	public class YooAssetManager : Node
		, ComponentOf<World>
		, AsRule<Awake>
	{
		/// <summary>
		/// 默认资源包
		/// </summary>
		public ResourcePackage package;

		/// <summary>
		/// 已加载资源句柄缓存（复用，避免重复加载）
		/// </summary>
		public Dictionary<string, AssetHandle> handleDict = new();
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
				foreach (AssetHandle handle in self.handleDict.Values)
				{
					handle.Release();
				}
				self.handleDict.Clear();
				self.package = null;
			}
		}

		/// <summary>
		/// 同步加载资源（句柄缓存复用）
		/// </summary>
		public static T LoadAsset<T>(this YooAssetManager self, string location) where T : UnityEngine.Object
		{
			if (self.handleDict.TryGetValue(location, out AssetHandle handle))
			{
				return handle.GetAssetObject<T>();
			}
			handle = self.package.LoadAssetSync<T>(location);
			self.handleDict[location] = handle;
			return handle.GetAssetObject<T>();
		}
	}
}
