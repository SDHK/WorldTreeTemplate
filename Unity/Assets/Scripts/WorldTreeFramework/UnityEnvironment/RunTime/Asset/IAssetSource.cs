/****************************************

* 作者：闪电黑客
* 日期：2026/10/22

* 描述：资源来源接口。具体实现（YooAsset、编辑器直读等）放在各自的目录。

*/

namespace WorldTree
{
	/// <summary>
	/// 资源来源：按地址加载并释放资源
	/// </summary>
	public interface IAssetSource : INode
	{
		/// <summary>
		/// 是否存在地址
		/// </summary>
		public bool Exists(string address);

		/// <summary>
		/// 同步加载
		/// </summary>
		public UnityEngine.Object Load(string address);

		/// <summary>
		/// 异步加载
		/// </summary>
		public TreeTask<UnityEngine.Object> LoadAsync(string address);

		/// <summary>
		/// 释放一个地址的句柄
		/// </summary>
		public void Release(string address);

		/// <summary>
		/// 释放全部句柄
		/// </summary>
		public void ReleaseAll();
	}
}
