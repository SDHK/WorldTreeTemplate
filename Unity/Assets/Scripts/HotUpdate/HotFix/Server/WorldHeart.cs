/****************************************

* 作者：闪电黑客
* 日期：2026/10/20

* 描述：服务器心跳。线程在 WorldHeartThread，这里只占住 WorldHeart 这个名字。

*/

namespace WorldTree
{
	/// <summary>
	/// 服务器心跳。驱动是 <see cref="WorldHeartThread"/>。
	/// </summary>
	public class WorldHeart : WorldHeartThread
	{
	}
}
