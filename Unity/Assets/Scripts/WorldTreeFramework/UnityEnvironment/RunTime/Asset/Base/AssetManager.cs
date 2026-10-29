/****************************************

* 作者：闪电黑客
* 日期：2026/10/22

* 描述：资源管理器。挂在 World 上，按组注册资源来源，按优先级解析地址。

*/

using System;
using System.Collections.Generic;

namespace WorldTree
{
	/// <summary>
	/// 资源管理器
	/// </summary>
	public class AssetManager : Node
		, ComponentOf<World>
		, AsFieldBranch
		, AsRule<Awake>
	{
		/// <summary>
		/// 默认资源组
		/// </summary>
		public const string DefaultGroup = "DefaultPackage";

		/// <summary>
		/// 分组到资源来源
		/// </summary>
		private readonly Dictionary<string, IAssetSource> sourceDict = new();

		/// <summary>
		/// 按优先级排序的资源来源
		/// </summary>
		private readonly List<AssetSourceEntry> sourceList = new();

		/// <summary>
		/// 按分组获取资源来源
		/// </summary>
		public IAssetSource GetSource(string group = DefaultGroup)
		{
			if (TryGetSource(group, out IAssetSource source)) return source;

			throw new Exception($"资源来源不存在: {group ?? DefaultGroup}");
		}

		/// <summary>
		/// 尝试按分组获取资源来源
		/// </summary>
		public bool TryGetSource(string group, out IAssetSource source)
		{
			return sourceDict.TryGetValue(group ?? DefaultGroup, out source);
		}

		/// <summary>
		/// 添加资源来源；分组重复会报错
		/// </summary>
		public void AddSource(string group, IAssetSource source, int priority = 100)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));

			string key = group ?? DefaultGroup;
			if (sourceDict.ContainsKey(key))
				throw new Exception($"资源分组重复: {key}");

			sourceDict.Add(key, source);

			int index = 0;
			while (index < sourceList.Count && sourceList[index].Priority <= priority) index++;
			sourceList.Insert(index, new AssetSourceEntry(source, priority));
		}

		/// <summary>
		/// 按优先级解析地址
		/// </summary>
		public IAssetSource ResolveSource(string address)
		{
			for (int i = 0; i < sourceList.Count; i++)
			{
				IAssetSource source = sourceList[i].Source;
				if (source.Exists(address)) return source;
			}

			throw new Exception($"资源不存在: {address}");
		}

		/// <summary>
		/// 释放所有来源的缓存
		/// </summary>
		public void ReleaseAll()
		{
			for (int i = 0; i < sourceList.Count; i++)
			{
				sourceList[i].Source.ReleaseAll();
			}
		}

		public override void OnDispose()
		{
			sourceDict.Clear();
			sourceList.Clear();
			base.OnDispose();
		}

		/// <summary>
		/// 资源来源条目
		/// </summary>
		private readonly struct AssetSourceEntry
		{
			/// <summary>
			/// 资源来源
			/// </summary>
			public IAssetSource Source { get; }

			/// <summary>
			/// 优先级，越小越优先
			/// </summary>
			public int Priority { get; }

			public AssetSourceEntry(IAssetSource source, int priority)
			{
				Source = source;
				Priority = priority;
			}
		}
	}
}
