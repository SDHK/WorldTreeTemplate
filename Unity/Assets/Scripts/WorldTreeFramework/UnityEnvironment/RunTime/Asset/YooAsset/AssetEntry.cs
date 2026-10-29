/****************************************

* 作者： 闪电黑客
* 日期： 2026/10/29

* 描述： 一个地址的 YooAsset 资源项。挂在 YooAssetManager 下。

*/

using UnityEngine;
using YooAsset;
using Object = UnityEngine.Object;

namespace WorldTree
{
	/// <summary>
	/// 一个地址的 YooAsset 资源项
	/// </summary>
	public class AssetEntry : Node
		, ChildOf<YooAssetManager>
		, AsRule<Awake>
	{
		/// <summary>
		/// YooAsset 句柄
		/// </summary>
		public AssetHandle Handle;

		/// <summary>
		/// 已加载资源
		/// </summary>
		public Object Asset;
	}

	/// <summary>
	/// AssetEntry 法则
	/// </summary>
	public static class AssetEntryRule
	{
		class RemoveRule : RemoveRule<AssetEntry>
		{
			protected override void Execute(AssetEntry self)
			{
				self.Handle?.Release();
				self.Handle = null;
				self.Asset = null;
			}
		}
	}
}
