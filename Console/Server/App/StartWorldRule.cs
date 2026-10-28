using System;
using System.IO;

namespace WorldTree.Server
{
    /// <summary>
    /// 启动世界的规则。读配置 + 创建其他世界线。
    /// </summary>
    public static class StartWorldRule
    {
        class Add : AddRule<StartWorld>
        {
            protected override void Execute(StartWorld self)
            {
                self.Log("启动世界：读配置 + 创建世界线");
                WorldTreeCore core = self.Line.Core;

                // 挂载服务器配置根
                string bytesRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ExcelTool", "Output", "Bytes", "s"));
                self.GetFileManager().AddSource(FileChannel.Pack, ConfigLoader.Group, string.Empty, new LocalFileSource(bytesRoot));

                // 读服务器配置
                string startPrefix = core.Options.ConfigPath.Replace('\\', '/').TrimEnd('/') + "/";
                ConfigLoader.Load<Tables>(self, ConfigLoader.GameConfigPath);
                ConfigLoader.Load<StartTables>(self, startPrefix);
                ConfigLoader.CreateWorldLines(core);
            }
        }
    }
}
