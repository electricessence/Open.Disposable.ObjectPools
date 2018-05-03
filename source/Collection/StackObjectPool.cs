﻿using System;
using System.Collections.Generic;

namespace Open.Disposable
{
	public sealed class StackObjectPool<T> : TrimmableObjectPoolBase<T>
		where T : class
	{

		public StackObjectPool(Func<T> factory, Action<T> recycler, int capacity = DEFAULT_CAPACITY, bool countTrackingEnabled = false)
			: base(factory, recycler, capacity, countTrackingEnabled)
		{
			Pool = new Stack<T>(capacity); // Very very slight speed improvment when capacity is set.
		}

		public StackObjectPool(Func<T> factory, int capacity = DEFAULT_CAPACITY, bool countTrackingEnabled = false)
			: this(factory, null, capacity, countTrackingEnabled)
		{

		}

		Stack<T> Pool;

		protected override bool Receive(T item)
		{
			var p = Pool;
			if (p != null)
			{
				// It's possible that the count could exceed MaxSize here, but the risk is negligble as a few over the limit won't hurt.
				// The lock operation should be quick enough to not pile up too many items.
				lock (p) p.Push(item);
				return true;
			}

			return false;
		}

		protected override T TryRelease()
		{
			var p = Pool;
			if (p != null && p.Count != 0)
			{
				lock (p)
				{
					if (p.Count != 0)
						return p.Pop();
				}

			}

			return null;
		}

		protected override void OnDispose(bool calledExplicitly)
		{
			Pool = null;
		}
	}

	public static class StackObjectPool
	{
		public static StackObjectPool<T> Create<T>(Func<T> factory, int capacity = Constants.DEFAULT_CAPACITY, bool countTrackingEnabled = false)
			where T : class
		{
			return new StackObjectPool<T>(factory, capacity, countTrackingEnabled);
		}

		public static StackObjectPool<T> Create<T>(int capacity = Constants.DEFAULT_CAPACITY, bool countTrackingEnabled = false)
			where T : class, new()
		{
			return Create(() => new T(), capacity, countTrackingEnabled);
		}

		public static StackObjectPool<T> Create<T>(Func<T> factory, bool autoRecycle, int capacity = Constants.DEFAULT_CAPACITY, bool countTrackingEnabled = false)
			 where T : class, IRecyclable
		{
			Action<T> recycler = null;
			if (autoRecycle) recycler = Recycler.Recycle;
			return new StackObjectPool<T>(factory, recycler, capacity, countTrackingEnabled);
		}

		public static StackObjectPool<T> Create<T>(bool autoRecycle, int capacity = Constants.DEFAULT_CAPACITY, bool countTrackingEnabled = false)
			where T : class, IRecyclable, new()
		{
			return Create(() => new T(), autoRecycle, capacity, countTrackingEnabled);
		}

	}
}