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
	/// GameObject对象池
	/// </summary>
	public class GameObjectPool : GenericPool<GameObject>, ChildOf<GameObjectPoolManager>
		, AsChildBranch
		, AsRule<Awake>
		, AsRule<ClockTimerTimeout>
	{
		/// <summary>
		/// 空闲多久开始清理，秒
		/// </summary>
		private const int IdleSeconds = 60;

		/// <summary>
		/// 清理间隔，秒
		/// </summary>
		private const int TrimIntervalSeconds = 1;

		/// <summary>
		/// 空闲计时器
		/// </summary>
		internal ClockTimer idleTimer;

		/// <summary>
		/// 预制体
		/// </summary>
		public GameObject prefab { get; set; }

		/// <summary>
		/// 游戏对象名称
		/// </summary>
		private string objName;

		public GameObjectPool()
		{
			ObjectType = typeof(GameObject);

			// 不限制池内空闲对象数量，只靠空闲计时的 DisposeOne 逐步清理。
			MaxLimit = -1;

			objName = "GameObject";

			NewObject = ObjectNew;
			DestroyObject = ObjectDestroy;

			objectOnNew += ObjectOnNew;
			objectOnGet += ObjectOnGet;
			objectOnRecycle += ObjectOnRecycle;
		}

		/// <summary>
		/// 设置预制体
		/// </summary>
		public void SetPrefab(GameObject prefab)
		{
			if (prefab != null)
			{
				this.prefab = prefab;
				objName = prefab.name;
			}
		}
		public override string ToString()
		{
			return "[GameObjectPool] : " + objName;
		}

		public override void OnDispose()
		{
			base.OnDispose();
		}

		/// <summary>
		/// 获取对象
		/// </summary>
		public override GameObject Get()
		{
			RefreshTime();
			return base.Get();
		}

		/// <summary>
		/// 获取对象（设置父节点）
		/// </summary>
		public GameObject Get(Transform parent)
		{
			RefreshTime();
			GameObject gameObject = DequeueOrNewObject();
			gameObject.transform.parent = parent;
			objectOnGet?.Invoke(gameObject);
			Preload();

			return gameObject;
		}

		/// <summary>
		/// 回收对象
		/// </summary>
		public override void Recycle(object obj)
		{
			RefreshTime();
			base.Recycle(obj);
		}

		/// <summary>
		/// 刷新空闲计时
		/// </summary>
		internal void RefreshTime()
		{
			idleTimer?.Restart(TimeSpan.FromSeconds(IdleSeconds));
		}

		/// <summary>
		/// 空闲到点
		/// </summary>
		internal void OnIdleTimeout()
		{
			if (IsDisposed) return;

			if (Count > 0) DisposeOne();

			idleTimer.Restart(TimeSpan.FromSeconds(Count > 0 ? TrimIntervalSeconds : IdleSeconds));
		}

		/// <summary>
		/// 新建对象
		/// </summary>
		private GameObject ObjectNew(ViewPool pool)
		{
			return (prefab == null) ? new GameObject(objName) : GameObject.Instantiate(prefab);
		}
		/// <summary>
		/// 销毁对象
		/// </summary>
		private void ObjectDestroy(GameObject gameObject)
		{
			GameObject.Destroy(gameObject);
		}
		/// <summary>
		/// 对象新建处理
		/// </summary>
		/// <param name="gameObject"></param>
		private void ObjectOnNew(GameObject gameObject)
		{

		}
		/// <summary>
		/// 对象获取处理
		/// </summary>
		private void ObjectOnGet(GameObject gameObject)
		{
			gameObject.SetActive(true);
		}
		/// <summary>
		/// 对象回收处理
		/// </summary>
		private void ObjectOnRecycle(GameObject gameObject)
		{
			gameObject.SetActive(false);
		}

	}

	/// <summary>
	/// GameObjectPool 法则
	/// </summary>
	public static class GameObjectPoolRule
	{
		class Awake : AwakeRule<GameObjectPool>
		{
			protected override void Execute(GameObjectPool self)
			{
				self.idleTimer = self.AddChild(out ClockTimer _);
				self.RefreshTime();
			}
		}

		class Timeout : ClockTimerTimeoutRule<GameObjectPool>
		{
			protected override void Execute(GameObjectPool self)
			{
				self.OnIdleTimeout();
			}
		}
	}
}