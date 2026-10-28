using System;
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
        /// 配置使用的文件分组。
        /// </summary>
        public const string Group = "Config";

        /// <summary>
        /// 游戏配置目录。
        /// </summary>
        public const string GameConfigPath = "GameConfig";

        /// <summary>
        /// Luban 表文件扩展名。
        /// </summary>
        public const string Extension = ".bytes";

        /// <summary>
        /// 拼接表文件 key。
        /// </summary>
        private static string BuildKey(string prefix, string name)
        {
            string head = string.IsNullOrEmpty(prefix) ? string.Empty : prefix.Replace('\\', '/').Trim('/');
            return head.Length == 0 ? name + Extension : head + "/" + name + Extension;
        }

        /// <summary>
        /// 读一张表并登记到核心。
        /// </summary>
        public static void Load<T>(INode node, string prefix) where T : class
        {
            FileManagerAccessor files = node.GetFileManager();
            WorldTreeCore core = node.World.Line.Core;
            Func<string, ByteBuf> loader = name =>
            {
                string key = BuildKey(prefix, name);
                byte[] bytes = files.ReadAllBytes(new FileAddress(FileChannel.Pack, Group, string.Empty, key));
                return new ByteBuf(bytes);
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
                if (row.LineId == 0) throw new Exception("起服表不能写线 0（线 0 由启动流程创建）");
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
