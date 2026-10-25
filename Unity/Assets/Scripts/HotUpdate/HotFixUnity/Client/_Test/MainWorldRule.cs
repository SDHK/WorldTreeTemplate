/****************************************

* 作者：闪电黑客
* 日期：2024/8/27 15:06

* 描述：

*/
namespace WorldTree
{
	public static partial class MainWorldRule
	{
		[NodeRule(nameof(Add))]
		private static void OnAddRule(this MainWorld self)
		{
			self.AddComponent(out CurveManager _);
			self.AddComponent(out TreeTweenManager _);
			self.AddComponent(out ViewManager _);
			self.AddComponent(out InitialDomain _);
		}

	}
}
