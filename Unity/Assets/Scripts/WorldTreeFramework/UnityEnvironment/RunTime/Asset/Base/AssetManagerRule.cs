/****************************************

* 作者：闪电黑客
* 日期：2026/10/22

* 描述：资源管理器的扩展方法。加载、释放都从这里走。

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
		/// 给管理器装上资源来源。只有一个来源，重复调用直接返回已有的。
		/// </summary>
		public static TSource AddAssetSource<TSource>(this AssetManager self)
			where TSource : class, INode, IAssetSource, AsRule<Awake>, FieldOf<AssetManager>
		{
			if (self.Source is TSource exists) return exists;

			self.AddField(out TSource source);
			self.Source = source;
			return source;
		}

		/// <summary>
		/// 同步加载
		/// </summary>
		public static T LoadAsset<T>(this AssetManager self, string address) where T : Object
		{
			return self.Source.Load(address) as T;
		}

		/// <summary>
		/// 异步加载
		/// </summary>
		public static async TreeTask<T> LoadAssetAsync<T>(this AssetManager self, string address) where T : Object
		{
			Object asset = await self.Source.LoadAsync(address);
			return asset as T;
		}

		/// <summary>
		/// 释放一个地址
		/// </summary>
		public static void ReleaseAsset(this AssetManager self, string address)
		{
			self.Source.Release(address);
		}

		/// <summary>
		/// 释放全部
		/// </summary>
		public static void ReleaseAllAsset(this AssetManager self)
		{
			self.Source.ReleaseAll();
		}
	}
}
