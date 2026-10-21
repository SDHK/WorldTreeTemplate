using System;
using System.IO;
using System.Reflection;
using Luban;

namespace WorldTree.Server
{
    /// <summary>
    /// 把 Luban 的 bin 读进配置管理器。
    /// </summary>
    public static class ConfigLoader
    {
        /// <summary>
        /// 读服务器的游戏表和起服表，登记到核心。
        /// </summary>
        public static void Load(WorldTreeCore core)
        {
            string bytesRoot = GetServerBytesRoot();
            string gameDir = Path.Combine(bytesRoot, "GameConfig");
            string startDir = Path.Combine(bytesRoot, core.Options.ConfigPath);

            Tables tables = new Tables(name => Read(gameDir, name));
            StartTables startTables = new StartTables(name => Read(startDir, name));
            core.ConfigManager.Add(tables);
            core.ConfigManager.Add(startTables);
            Console.WriteLine($"配置已加载: WorldLineConfig {startTables.WorldLineConfig.DataList.Count} 行");
        }

        /// <summary>
        /// 服务器 bytes 根目录：ExcelTool/Output/Bytes/s。
        /// 程序在 Console/Server/Bin，向上三级是项目根。
        /// </summary>
        public static string GetServerBytesRoot()
        {
            string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "ExcelTool", "Output", "Bytes", "s"));
            if (!Directory.Exists(root)) throw new DirectoryNotFoundException($"找不到配置目录: {root}");
            return root;
        }

        /// <summary>
        /// 按起服表创建本进程的世界线。没有线 0 时补默认的 MainWorld。
        /// </summary>
        public static void CreateWorldLines(WorldTreeCore core)
        {
            EnsureTypes(core);
            StartTables startTables = core.ConfigManager.Get<StartTables>();
            WorldLineConfigData line0 = null;
            foreach (WorldLineConfigData row in startTables.WorldLineConfig.DataList)
            {
                if (row.Process != core.Options.Process || row.LineId != 0) continue;
                if (line0 != null) throw new Exception("起服表里线 0 重复");
                line0 = row;
            }

            if (line0 == null)
            {
                core.Create(0, typeof(MainWorld), typeof(WorldHeart), 1000);
                core.MainLine.Log("创建世界线 0 MainWorld WorldHeart 1000");
            }
            else
            {
                CreateWorldLine(core, line0);
            }

            foreach (WorldLineConfigData row in startTables.WorldLineConfig.DataList)
            {
                if (row.Process != core.Options.Process || row.LineId == 0) continue;
                CreateWorldLine(core, row);
            }
        }

        /// <summary>
        /// 把已加载程序集登记进 TypeInfo，供起服表按类型名创建世界线。
        /// </summary>
        private static void EnsureTypes(WorldTreeCore core)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }
                if (types == null) continue;
                foreach (Type type in types)
                {
                    if (type != null) core.TypeInfo.Add(type);
                }
            }
        }

        /// <summary>
        /// 按起服表的一行创建世界线。
        /// </summary>
        private static void CreateWorldLine(WorldTreeCore core, WorldLineConfigData row)
        {
            Type worldType = core.TypeInfo.NameToType(row.WorldType);
            Type heartType = core.TypeInfo.NameToType(row.HeartType);
            core.Create(row.LineId, worldType, heartType, row.FrameTime);
            core.MainLine.Log($"创建世界线 {row.LineId} {row.WorldType} {row.HeartType} {row.FrameTime}");
        }

        /// <summary>
        /// 读取一张表的 bytes。
        /// </summary>
        private static ByteBuf Read(string directory, string name)
        {
            string path = Path.Combine(directory, name + ".bytes");
            if (!File.Exists(path)) throw new FileNotFoundException($"找不到配置: {path}");
            return new ByteBuf(File.ReadAllBytes(path));
        }
    }
}
