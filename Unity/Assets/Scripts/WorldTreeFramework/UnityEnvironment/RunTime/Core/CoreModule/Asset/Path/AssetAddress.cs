/****************************************

* 作者： 闪电黑客
* 日期： 2026/10/29

* 描述： 资源地址。格式为 Pack:Key；省略 Pack 时使用默认包。

*/

using System;

namespace WorldTree
{
	/// <summary>
	/// 资源地址
	/// </summary>
	public readonly struct AssetAddress : IEquatable<AssetAddress>
	{
		/// <summary>
		/// 资源包名；空字符串表示默认包
		/// </summary>
		public string Pack { get; }

		/// <summary>
		/// 包内地址
		/// </summary>
		public string Key { get; }

		public AssetAddress(string pack, string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentException("资源 Key 不能为空", nameof(key));

			Pack = pack ?? string.Empty;
			Key = key;
		}

		/// <summary>
		/// 解析 @Pack:Key；没有 @ 前缀时使用默认包
		/// </summary>
		public static AssetAddress Parse(string address)
		{
			if (!TryParse(address, out AssetAddress result))
				throw new ArgumentException("资源地址格式错误", nameof(address));

			return result;
		}

		/// <summary>
		/// 尝试解析 @Pack:Key；没有 @ 时作为默认包的 Key
		/// </summary>
		public static bool TryParse(string address, out AssetAddress result)
		{
			result = default;
			if (string.IsNullOrEmpty(address)) return false;

			if (address[0] != '@')
			{
				result = new AssetAddress(string.Empty, address);
				return true;
			}

			int colon = address.IndexOf(':', 1);
			if (colon <= 1 || colon == address.Length - 1) return false;

			string pack = address.Substring(1, colon - 1);
			string key = address.Substring(colon + 1);
			result = new AssetAddress(pack, key);
			return true;
		}

		public override string ToString()
		{
			return Pack.Length == 0 ? Key : "@" + Pack + ":" + Key;
		}

		public bool Equals(AssetAddress other)
		{
			return string.Equals(Pack, other.Pack, StringComparison.Ordinal)
				&& string.Equals(Key, other.Key, StringComparison.Ordinal);
		}

		public override bool Equals(object obj)
		{
			return obj is AssetAddress other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Pack.GetHashCode() * 397) ^ Key.GetHashCode();
			}
		}
	}
}
