/****************************************

* 作者：闪电黑客
* 日期：2026/10/22

* 描述：启动世界的规则。读配置 + 创建其他世界线。

*/

using System;
using System.IO;
using UnityEngine;

namespace WorldTree
{
	public static class StartWorldRule
	{
		class Add : AddRule<StartWorld>
		{
			protected override void Execute(StartWorld self)
			{
				self.Log("启动世界：读配置 + 创建世界线");
				WorldTreeCore core = self.Line.Core;

				// 运行时挂 YooAssetManager（编辑器直读文件，不需要）
				if (!Define.IsEditor) self.Line.AddComponent(out YooAssetManager _);

				// 读配置：编辑器直读工程外
				string bytesRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "ExcelTool", "Output", "Bytes", "c"));
				ConfigLoader.Load<Tables>(core, Path.Combine(bytesRoot, "GameConfig"));
				ConfigLoader.Load<StartTables>(core, Path.Combine(bytesRoot, "StartConfig", "Client"));
				ConfigLoader.CreateWorldLines(core);
			}
		}
	}
}
