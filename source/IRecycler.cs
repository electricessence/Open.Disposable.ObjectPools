﻿using System;
using System.Threading.Tasks;

namespace Open.Disposable
{
	public interface IRecycler<in T> : IDisposable
		where T : class
	{
		bool Recycle(T item);
		Task Close();
	}
}
