using System;
using System.IO;
using System.Linq;
using Luban;

namespace WorldTree.Server
{
    /// <summary>
    /// 把 Luban 的 bin 读进配置管理器。
    /// </summary>
    public static class ConfigLoader
    {
        /// <summary>
        /// 读一张表并登记到核心。
        /// </summary>
        public static void Load<T>(WorldTreeCore core, string path) where T : class
        {
            Func<string, ByteBuf> loader = name =>
            {
                string file = Path.Combine(path, name + ".bytes");
                if (!File.Exists(file)) throw new FileNotFoundException($"找不到配置: {file}");
                return new ByteBuf(File.ReadAllBytes(file));
            };
            core.ConfigManager.Add((T)Activator.CreateInstance(typeof(T), loader));
        }

        /// <summary>
        /// 按起服表创建本进程的世界线。
        /// </summary>
        public static void CreateWorldLines(WorldTreeCore core)
        {
            EnsureTypes(core);
            StartTables startTables = core.ConfigManager.Get<StartTables>();
            foreach (WorldLineConfigData row in startTables.WorldLineConfig.DataList)
            {
                if (row.Process != core.Options.Process) continue;
                CreateWorldLine(core, row);
            }
        }

        /// <summary>
        /// 把已加载程序集登记进 TypeInfo，供起服表按类型名创建世界线。
        /// </summary>
        private static void EnsureTypes(WorldTreeCore core)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly =>
                {
                    string name = assembly.GetName().Name;
                    return name != null && (name.StartsWith("WorldTree") || name.StartsWith("HotFix") || name.StartsWith("GameEntry"));
                })
                .ToArray();
            core.TypeInfo.LoadAssembly(core, assemblies);
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
    }
}
