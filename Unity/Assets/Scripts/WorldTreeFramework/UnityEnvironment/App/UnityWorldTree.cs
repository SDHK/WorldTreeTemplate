/****************************************

* 作者：闪电黑客
* 日期：2024/5/14 17:47

* 描述：

*/
using System;
using UnityEngine;

namespace WorldTree
{
	/// <summary>
	/// 世界树框架启动器，一切从这里开始
	/// </summary>
	public class UnityWorldTree : MonoBehaviour
	{
		/// <summary>
		/// 世界树核心
		/// </summary>
		public WorldTreeCore WorldTreeCore;

		/// <summary>
		/// 启动项
		/// </summary>
		public Options Options = new();

		/// <summary>
		/// 启动
		/// </summary>
		public void Start()
		{

			AppDomain.CurrentDomain.UnhandledException += (sender, e) => Debug.LogError(e.ExceptionObject.ToString());
			// 命令行参数接收
			//Parser.Default.ParseArguments<Options>(System.Environment.GetCommandLineArgs())
			//		   .WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
			//		   .WithParsed((o) => Options = o);

			WorldTreeCore = new();
			WorldTreeCore.SetLog<UnityWorldLog>();
			WorldTreeCore.SetOptions(Options);

			if (Define.IsEditor) WorldTreeCore.SetView(typeof(UnityViewBuilderWorld), typeof(WorldHeart), typeof(UnityWorldTreeNodeViewBuilder));

			// 线 0 启动世界线（StartWorld 的 Add 规则读配置 + 建线）
			WorldTreeCore.Create(0, typeof(StartWorld), typeof(WorldHeart), 0);
		}


		/// <summary>
		/// 退出
		/// </summary>
		private void OnApplicationQuit()
		{
			WorldTreeCore?.Dispose();
			WorldTreeCore = null;
		}

		/// <summary>
		/// 销毁
		/// </summary>
		private void OnDestroy()
		{
			WorldTreeCore?.Dispose();
			WorldTreeCore = null;
		}
	}
}