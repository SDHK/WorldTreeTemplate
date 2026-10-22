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

                // 读服务器配置
                string bytesRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ExcelTool", "Output", "Bytes", "s"));
                ConfigLoader.Load<Tables>(core, Path.Combine(bytesRoot, "GameConfig"));
                ConfigLoader.Load<StartTables>(core, Path.Combine(bytesRoot, core.Options.ConfigPath));
                ConfigLoader.CreateWorldLines(core);
            }
        }
    }
}
