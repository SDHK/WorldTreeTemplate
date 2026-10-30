/****************************************

* 作者：闪电黑客
* 日期：2026/10/22

* 描述：资源管理器的扩展方法。加载、卸载都从这里走。

*/

using UnityEngine;

namespace WorldTree
{
	public static class AssetManagerRule
	{
		/// <summary>
		/// 获取世界上的资源管理器，没有就建一个。来源由启动流程装配。
		/// </summary>
		public static AssetManager GetAssetManager(this World self)
		{
			if (self.TryGetComponent(out AssetManager manager)) return manager;

			self.AddComponent(out manager);
			return manager;
		}

		/// <summary>
		/// 给默认组装上资源来源
		/// </summary>
		public static TSource AddAssetSource<TSource>(this AssetManager self)
			where TSource : class, INode, IAssetSource, AsRule<Awake>, FieldOf<AssetManager>
		{
			if (self.TryGetSource(AssetManager.DefaultGroup, out _))
				throw new System.Exception($"资源分组重复: {AssetManager.DefaultGroup}");

			self.AddField(out TSource source);
			self.AddSource(AssetManager.DefaultGroup, source, 100);
			return source;
		}

		/// <summary>
		/// 给指定组装上资源来源；group 同时作为初始化参数
		/// </summary>
		public static TSource AddAssetSource<TSource>(this AssetManager self, string group, int priority = 100)
			where TSource : class, INode, IAssetSource, AsRule<Awake<string>>, FieldOf<AssetManager>
		{
			if (self.TryGetSource(group, out _))
				throw new System.Exception($"资源分组重复: {group ?? AssetManager.DefaultGroup}");

			self.AddField(out TSource source, group);
			self.AddSource(group, source, priority);
			return source;
		}

		/// <summary>
		/// 同步加载；无 @ 前缀走链式，@Pack:Key 指定包
		/// </summary>
		public static T LoadAsset<T>(this AssetManager self, string address) where T : Object
		{
			return self.LoadAsset<T>(AssetAddress.Parse(address));
		}

		/// <summary>
		/// 同步加载指定包和地址
		/// </summary>
		public static T LoadAsset<T>(this AssetManager self, string pack, string key) where T : Object
		{
			IAssetSource source = ResolveSource(self, pack, key);
			return source.Load(key) as T;
		}

		/// <summary>
		/// 同步加载指定资源地址
		/// </summary>
		public static T LoadAsset<T>(this AssetManager self, AssetAddress address) where T : Object
		{
			IAssetSource source = ResolveSource(self, address.Pack, address.Key);
			return source.Load(address.Key) as T;
		}

		/// <summary>
		/// 异步加载；无 @ 前缀走链式，@Pack:Key 指定包
		/// </summary>
		public static TreeTask<T> LoadAssetAsync<T>(this AssetManager self, string address) where T : Object
		{
			return self.LoadAssetAsync<T>(AssetAddress.Parse(address));
		}

		/// <summary>
		/// 异步加载指定包和地址
		/// </summary>
		public static async TreeTask<T> LoadAssetAsync<T>(this AssetManager self, string pack, string key) where T : Object
		{
			IAssetSource source = ResolveSource(self, pack, key);
			Object asset = await source.LoadAsync(key);
			return asset as T;
		}

		/// <summary>
		/// 异步加载指定资源地址
		/// </summary>
		public static async TreeTask<T> LoadAssetAsync<T>(this AssetManager self, AssetAddress address) where T : Object
		{
			IAssetSource source = ResolveSource(self, address.Pack, address.Key);
			Object asset = await source.LoadAsync(address.Key);
			return asset as T;
		}

		/// <summary>
		/// 卸载；无 @ 前缀走链式，@Pack:Key 指定包
		/// </summary>
		public static void UnloadAsset(this AssetManager self, string address)
		{
			self.UnloadAsset(AssetAddress.Parse(address));
		}

		/// <summary>
		/// 卸载指定包和地址
		/// </summary>
		public static void UnloadAsset(this AssetManager self, string pack, string key)
		{
			ResolveSource(self, pack, key).Release(key);
		}

		/// <summary>
		/// 卸载指定资源地址
		/// </summary>
		public static void UnloadAsset(this AssetManager self, AssetAddress address)
		{
			ResolveSource(self, address.Pack, address.Key).Release(address.Key);
		}

		/// <summary>
		/// 解析资源来源
		/// </summary>
		private static IAssetSource ResolveSource(AssetManager self, string pack, string key)
		{
			return string.IsNullOrEmpty(pack)
				? self.ResolveSource(key)
				: self.GetSource(pack);
		}

		/// <summary>
		/// 卸载全部
		/// </summary>
		public static void UnloadAllAssets(this AssetManager self)
		{
			self.ReleaseAll();
		}

		/// <summary>
		/// 卸载指定包
		/// </summary>
		public static void UnloadAllAssets(this AssetManager self, string pack)
		{
			self.GetSource(pack).ReleaseAll();
		}

	}
}
