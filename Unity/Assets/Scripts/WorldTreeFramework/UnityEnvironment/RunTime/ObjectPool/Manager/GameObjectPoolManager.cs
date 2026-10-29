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
	/// 游戏对象池管理器
	/// </summary>
	public class GameObjectPoolManager : Node, ComponentOf<World>
		, AsChildBranch
		, AsRule<Awake>
	{
		UnitDictionary<GameObject, GameObjectPool> poolDict = new UnitDictionary<GameObject, GameObjectPool>();

		/// <summary>
		/// 获取实例
		/// </summary>
		public GameObject Get(GameObject prefab)
		{
			return GetPool(prefab).Get();
		}

		/// <summary>
		/// 回收实例
		/// </summary>
		public void Recycle(GameObject prefab, GameObject gameObject)
		{
			GetPool(prefab).Recycle(gameObject);
		}

		/// <summary>
		/// 加载预制体并从池获取实例；无冒号走链式，Pack:Key 指定包
		/// </summary>
		public GameObjectNode PoolGet(string address, Transform parent = null)
		{
			GameObject prefab = World.GetAssetManager().LoadAsset<GameObject>(address);
			return CreateNode(prefab, parent);
		}

		/// <summary>
		/// 异步加载预制体并从池获取实例；无冒号走链式，Pack:Key 指定包
		/// </summary>
		public async TreeTask<GameObjectNode> PoolGetAsync(string address, Transform parent = null)
		{
			GameObject prefab = await World.GetAssetManager().LoadAssetAsync<GameObject>(address);
			return CreateNode(prefab, parent);
		}

		/// <summary>
		/// 加载预制体并从池获取实例，节点挂到指定节点下。
		/// </summary>
		public GameObjectNode PoolGet<N>(string address, N parent, Transform transformParent = null)
			where N : class, INode, AsChildBranch
		{
			GameObject prefab = World.GetAssetManager().LoadAsset<GameObject>(address);
			if (prefab == null) throw new ArgumentNullException(nameof(prefab));

			GameObjectNode node = parent.AddChild(out GameObjectNode _);
			node.Bind(GetPool(prefab), transformParent);
			return node;
		}

		/// <summary>
		/// 异步加载预制体并从池获取实例，节点挂到指定节点下。
		/// </summary>
		public async TreeTask<GameObjectNode> PoolGetAsync<N>(string address, N parent, Transform transformParent = null)
			where N : class, INode, AsChildBranch
		{
			GameObject prefab = await World.GetAssetManager().LoadAssetAsync<GameObject>(address);
			if (prefab == null) throw new ArgumentNullException(nameof(prefab));

			GameObjectNode node = parent.AddChild(out GameObjectNode _);
			node.Bind(GetPool(prefab), transformParent);
			return node;
		}

		/// <summary>
		/// 创建池化节点
		/// </summary>
		private GameObjectNode CreateNode(GameObject prefab, Transform parent)
		{
			if (prefab == null)
			{
				throw new System.ArgumentNullException(nameof(prefab));
			}

			GameObjectNode node = World.AddChild(out GameObjectNode _);
			node.Bind(GetPool(prefab), parent);
			return node;
		}

		/// <summary>
		/// 获取池
		/// </summary>
		public GameObjectPool GetPool(GameObject prefab)
		{
			if (!poolDict.TryGetValue(prefab, out GameObjectPool pool))
			{
				this.AddChild(out pool);
				pool.SetPrefab(prefab);
				poolDict.Add(prefab, pool);
			}
			return pool;
		}

		/// <summary>
		/// 释放池
		/// </summary>
		public void DisposePool(GameObject prefab)
		{
			if (poolDict.TryGetValue(prefab, out GameObjectPool pool))
			{
				pool.Dispose();
				poolDict.Remove(prefab);
			}
		}
	}
}
