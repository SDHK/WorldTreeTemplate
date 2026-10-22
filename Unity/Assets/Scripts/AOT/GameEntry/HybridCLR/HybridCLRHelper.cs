/****************************************

* 作者： 闪电黑客
* 日期： 2024/3/19 17:43

* 描述：

*/

using HybridCLR;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using YooAsset;

namespace WorldTree.AOT
{
	/// <summary>
	/// HybridCLR辅助类
	/// </summary>
	public static class HybridCLRHelper
	{
		/// <summary>
		/// 加载AOT
		/// </summary>
		public static async Task LoadAOT()
		{
			if (!Define.IsEditor)
			{
				ResourcePackage package = YooAssets.GetPackage("DefaultPackage");
				foreach (AssetInfo assetInfo in YooAssets.GetAssetInfos("aotDlls"))
				{
					AssetHandle handle = package.LoadAssetAsync<TextAsset>(assetInfo.Address);
					await handle.Task;
					TextAsset textAsset = (handle.AssetObject as TextAsset);
					Debug.Log($"AOT加载:{assetInfo.Address} : {textAsset.bytes.Length}");
					RuntimeApi.LoadMetadataForAOTAssembly(textAsset.bytes, HomologousImageMode.SuperSet);
				}
			}

			await Task.CompletedTask;
		}

		/// <summary>
		/// 热更程序集加载顺序。必须按依赖：被引用的先加载。
		/// YooAsset 按标签取资源的顺序不保证这个关系，App 若先于 HotFix 加载，会在解析 WorldLineConfigData 时抛 TypeLoadException。
		/// </summary>
		private static readonly string[] HotUpdateOrder =
		{
			"WorldTree.CoreUnity.dll",
			"WorldTree.HotFix.dll",
			"WorldTree.App.dll",
			"WorldTree.HotFixUnity.dll",
		};

		/// <summary>
		/// 加载热更
		/// </summary>
		public static async Task LoadHotUpdate()
		{
			if (!Define.IsEditor)
			{
				ResourcePackage package = YooAssets.GetPackage("DefaultPackage");
				Dictionary<string, TextAsset> dlls = new();
				foreach (AssetInfo assetInfo in YooAssets.GetAssetInfos("hotUpdateDlls"))
				{
					if (!assetInfo.Address.EndsWith(".dll")) continue;
					AssetHandle handle = package.LoadAssetAsync<TextAsset>(assetInfo.Address);
					await handle.Task;
					dlls.Add(assetInfo.Address, handle.AssetObject as TextAsset);
				}

				foreach (string address in HotUpdateOrder)
				{
					if (!dlls.TryGetValue(address, out TextAsset textAsset))
					{
						throw new Exception($"热更包里没有 {address}");
					}
					Debug.Log($"HotUpdate加载 {address}:{textAsset.bytes.Length}");
					Assembly.Load(textAsset.bytes);
				}
			}

			await Task.CompletedTask;
		}
	}
}