/****************************************

* 作者：闪电黑客
* 日期：2026/10/22

* 描述：启动世界的规则。读配置 + 创建其他世界线。

*/

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

				// 挂资源管理器，并装上 YooAsset 来源
				YooAssetManager yooAsset = self.Line.GetAssetManager().AddAssetSource<YooAssetManager>();

				// 按平台选择配置源
				FileManagerAccessor files = self.GetFileManager();
				if (Define.IsEditor)
				{
					string bytesRoot = Path.GetFullPath(Path.Combine(
						Application.dataPath, "..", "..", "ExcelTool", "Output", "Bytes", "c"));
					files.AddSource(FileChannel.Pack, ConfigLoader.Group, string.Empty, new LocalFileSource(bytesRoot));
				}
				else
				{
					files.AddSource(FileChannel.Pack, ConfigLoader.Group, string.Empty, new YooAssetFileSource(yooAsset));
				}

				ConfigLoader.Load<Tables>(self, ConfigLoader.GameConfigPath);
				ConfigLoader.Load<StartTables>(self, ConfigLoader.StartConfigPath);
				ConfigLoader.CreateWorldLines(core);
			}
		}
	}
}
