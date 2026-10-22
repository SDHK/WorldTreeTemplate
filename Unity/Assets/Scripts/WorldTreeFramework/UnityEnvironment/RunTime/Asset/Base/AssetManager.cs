/****************************************

* 作者：闪电黑客
* 日期：2026/10/22

* 描述：资源管理器。挂在 World 上，持有一个资源来源，业务代码只认这个入口。

*/

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
		/// 资源来源
		/// </summary>
		public IAssetSource Source;
	}
}
