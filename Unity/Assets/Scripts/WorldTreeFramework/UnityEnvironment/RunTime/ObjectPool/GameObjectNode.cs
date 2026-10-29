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
	/// 游戏物体实体
	/// </summary>
	public class GameObjectNode : Node, ComponentOf<INode>, ChildOf<INode>
		, AsComponentBranch
		, AsRule<Awake>
	{
		/// <summary>
		/// 对象池
		/// </summary>
		internal GameObjectPool pool;

		/// <summary>
		/// 游戏物体
		/// </summary>
		public GameObject gameObject;
		/// <summary>
		/// 物体变换
		/// </summary>
		public Transform transform;

		public override string ToString()
		{
			return gameObject ? "GameObjectNode : " + gameObject.name : "GameObjectNode : ";
		}

		/// <summary>
		/// 从对象池绑定实例；只允许对象池管理器调用。
		/// </summary>
		internal void Bind(GameObjectPool pool, Transform parent = null)
		{
			if (pool == null)
			{
				throw new ArgumentNullException(nameof(pool));
			}

			this.pool = pool;
			gameObject = parent == null ? pool.Get() : pool.Get(parent);
			transform = gameObject.transform;
		}
	}

	class GameObjectEntityRemoveRule : RemoveRule<GameObjectNode>
	{
		protected override void Execute(GameObjectNode self)
		{
			if (self.gameObject != null)
			{
				if (self.pool != null && !self.pool.IsDisposed)
				{
					self.pool.Recycle(self.gameObject);
				}
				else
				{
					GameObject.Destroy(self.gameObject);
				}
			}

			self.gameObject = null;
			self.transform = null;
			self.pool = null;
		}
	}
}
