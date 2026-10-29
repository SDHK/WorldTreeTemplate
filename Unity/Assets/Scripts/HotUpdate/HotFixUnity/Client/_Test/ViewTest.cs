using System;

namespace WorldTree
{
	/// <summary>
	/// 视图测试节点
	/// </summary>
	public partial class ViewTest : Node
		, ComponentOf<InitialDomain>
		, AsComponentBranch
		, AsRule<Awake>
	{
		[NodeRule(nameof(Add))]
		private static void OnAddRule(ViewTest self)
		{
			ViewTestRunner.Run(self).Coroutine();
		}
	}

	/// <summary>
	/// 视图测试执行器
	/// </summary>
	internal static class ViewTestRunner
	{
		/// <summary>
		/// 执行视图测试
		/// </summary>
		public static async TreeTask Run(ViewTest self)
		{
			self.Log($"视图测试开始！！!");

			if (!self.World.TryGetComponent(out ViewManager windowManager))
			{
				self.World.AddComponent(out windowManager);
			}

			ViewLayer viewLayer = await windowManager.GetLayer(0);
			viewLayer.TryAddView(out ViewTestWindow view);
			view.Name = "测试窗口ViewTestWindow";

			// 1. 打开
			await viewLayer.OpenView(view.Type);
			Check(view.IsOpen, "打开后 IsOpen");
			Check(view.Bind.Value != null, "打开后 Bind 存在");
			Check(ReferenceEquals(view.Bind.Value.Parent, view), "Bind.Parent == View");
			Check(ReferenceEquals(view.ParentLayer, viewLayer), "ParentLayer == 层");
			Check(viewLayer.LayerCount == 1, "层内窗口数 = 1");

			// 2. 显隐
			view.IsShow = false;
			Check(!view.IsShow, "隐藏后 IsShow = false");
			view.IsShow = true;
			Check(view.IsShow, "显示后 IsShow = true");

			// 3. 关闭
			await viewLayer.CloseView(view.Type);
			Check(!view.IsOpen, "关闭后 IsOpen = false");
			Check(viewLayer.LayerCount == 0, "关闭后层内窗口数 = 0");

			// 4. 重开 + 释放
			await viewLayer.OpenView(view.Type);
			Check(view.IsOpen, "重开后 IsOpen");
			await viewLayer.CloseView(view.Type);
			Check(!view.IsOpen, "再次关闭后 IsOpen = false");

			view.Dispose();
			Check(!view.IsOpen, "Dispose 后 IsOpen = false");

			self.Log($"视图测试通过！！!");
		}

		/// <summary>
		/// 检查条件 
		/// </summary>
		/// <exception cref="Exception"></exception>
		private static void Check(bool ok, string what)
		{
			if (!ok) throw new Exception($"视图测试失败：{what}");
		}
	}
}
