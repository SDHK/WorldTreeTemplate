/****************************************

* 作者：闪电黑客
* 日期：2024/5/14 17:47

* 描述：

*/
using System.IO;

namespace WorldTree
{
	public static class DataBaseTestRule
	{
		private class AwakeRule : AwakeRule<DataBaseTest>
		{
			protected override void Execute(DataBaseTest self)
			{
				string path = Path.Combine(UnityEngine.Application.persistentDataPath, "LiteDBTest.db");
				self.AddDataSource<LiteDBManager>(path);
			}
		}
	}

	public static class LiteDBTestRule
	{

		private class AwakeRule : AwakeRule<LiteDBTest>
		{
			protected override void Execute(LiteDBTest self)
			{
				//获取数据库代理
				self.World.AddComponent(out DataBaseTest dataBaseProxy);

				long id;

				self.AddChild(out TestClass testClass);
				testClass.Name = "A123测试Test!!!!!ABCD=123456789";

				id = testClass.Id;

				//插入数据
				dataBaseProxy.Insert(testClass.Id, testClass);

				//销毁类型
				testClass.Dispose();

				//查询数据
				TestClass result = dataBaseProxy.Find<TestClass>(id);

				result.SetParent(self);//设置父节点


				self.Log($"读取：{result.Name}");

				self.Log(NodeRule.ToStringDrawTree(self));
			}
		}
	}
}