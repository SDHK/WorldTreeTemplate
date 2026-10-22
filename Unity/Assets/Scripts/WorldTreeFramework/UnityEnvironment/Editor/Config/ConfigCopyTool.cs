/****************************************

* 作者： 闪电黑客
* 日期： 2026/10/21

* 描述： 客户端 Luban 配置拷贝工具。把 ExcelTool 导出的产物同步进 Unity。

*/

using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTool
{
	/// <summary>
	/// 客户端 Luban 配置拷贝工具
	/// </summary>
	public static class ConfigCopyTool
	{
		// 源目录（相对 ExcelTool 根）
		private const string RuntimeSrc = "Luban/Runtime";
		private const string CSharpSrc = "Output/CSharp/c";
		private const string GameBytesSrc = "Output/Bytes/c/GameConfig";
		private const string StartBytesSrc = "Output/Bytes/c/StartConfig/Client";

		// 目标目录（相对 Assets 根）
		private const string RuntimeDst = "Scripts/ThirdParty/Luban";
		private const string CSharpDst = "Scripts/HotUpdate/HotFix/Generated";
		private const string GameBytesDst = "AssetBundles/Config/GameConfig";
		private const string StartBytesDst = "AssetBundles/Config/StartConfig/Client";

		[MenuItem("WorldTree/拷贝客户端配置")]
		public static void Copy()
		{
			string assetsRoot = Application.dataPath;
			string excelRoot = Path.GetFullPath(Path.Combine(assetsRoot, "..", "..", "ExcelTool"));

			if (!Directory.Exists(excelRoot))
			{
				Debug.LogError($"找不到 ExcelTool 目录：{excelRoot}（Unity 工程和 ExcelTool 应在同一父目录下）");
				return;
			}

			string csharpSrc = Path.Combine(excelRoot, CSharpSrc);
			if (!Directory.Exists(csharpSrc))
			{
				Debug.LogError($"找不到客户端表代码：{csharpSrc}，请先运行 ExcelTool/Excel/配置导出.bat");
				return;
			}

			CopyDir(Path.Combine(excelRoot, RuntimeSrc), Path.Combine(assetsRoot, RuntimeDst));
			CopyDir(csharpSrc, Path.Combine(assetsRoot, CSharpDst));
			CopyDir(Path.Combine(excelRoot, GameBytesSrc), Path.Combine(assetsRoot, GameBytesDst));
			CopyDir(Path.Combine(excelRoot, StartBytesSrc), Path.Combine(assetsRoot, StartBytesDst));

			AssetDatabase.Refresh();
			Debug.Log("客户端配置拷贝完成");
		}

		/// <summary>
		/// 清空目标目录后递归拷贝整个源目录。
		/// </summary>
		private static void CopyDir(string src, string dst)
		{
			if (!Directory.Exists(src))
			{
				Debug.LogWarning($"源目录不存在（跳过）：{src}");
				return;
			}

			if (Directory.Exists(dst))
			{
				Directory.Delete(dst, true);
			}
			Directory.CreateDirectory(dst);

			foreach (string file in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
			{
				string relative = file.Substring(src.Length).TrimStart('\\', '/');
				string target = Path.Combine(dst, relative);
				Directory.CreateDirectory(Path.GetDirectoryName(target));
				File.Copy(file, target, true);
			}
		}
	}
}
