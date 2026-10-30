using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldTree
{
	/// <summary>
	/// 视图平台元素收集器：保存 View 字段名到 prefab 平台对象的映射。
	/// </summary>
	[DisallowMultipleComponent]
	public class ViewElementCollector : MonoBehaviour
	{
		/// <summary>
		/// 收集项
		/// </summary>
		[Serializable]
		public class Entry
		{
			/// <summary>
			/// View 字段名
			/// </summary>
			public string Key;

			/// <summary>
			/// 平台对象
			/// </summary>
			public UnityEngine.Object Target;
		}

		/// <summary>
		/// 目标 View 类型名
		/// </summary>
		[SerializeField]
		private string viewTypeName;

		/// <summary>
		/// 收集项
		/// </summary>
		[SerializeField]
		private Entry[] entries = Array.Empty<Entry>();

		/// <summary>
		/// 运行时查找表
		/// </summary>
		private Dictionary<string, UnityEngine.Object> mapDict;

		/// <summary>
		/// 目标 View 类型名
		/// </summary>
		public string ViewTypeName
		{
			get => viewTypeName;
			set
			{
				viewTypeName = value;
				mapDict = null;
			}
		}

		/// <summary>
		/// 收集项
		/// </summary>
		public IReadOnlyList<Entry> Entries => entries;

		/// <summary>
		/// 尝试获取平台对象
		/// </summary>
		public bool TryGet(string key, out UnityEngine.Object target)
		{
			BuildMap();
			return mapDict.TryGetValue(key, out target);
		}

		/// <summary>
		/// 尝试获取平台组件
		/// </summary>
		public bool TryGet<T>(string key, out T target)
			where T : UnityEngine.Object
		{
			if (TryGet(key, out UnityEngine.Object value) && value is T result)
			{
				target = result;
				return true;
			}

			target = null;
			return false;
		}

		/// <summary>
		/// Awake
		/// </summary>
		private void Awake() => BuildMap();

		/// <summary>
		/// 序列化变化后重建
		/// </summary>
		private void OnValidate() => mapDict = null;

		/// <summary>
		/// 构建查找表
		/// </summary>
		private void BuildMap()
		{
			if (mapDict != null) return;

			mapDict = new Dictionary<string, UnityEngine.Object>(entries.Length);
			for (int i = 0; i < entries.Length; i++)
			{
				Entry entry = entries[i];
				if (entry == null || string.IsNullOrEmpty(entry.Key)) continue;
				mapDict[entry.Key] = entry.Target;
			}
		}
	}
}
