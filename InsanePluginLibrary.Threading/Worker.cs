using InsaneLibrary.Threading;
using System;
using System.Threading;
using UnityEngine;

namespace InsanePluginLibrary.Threading
{
	abstract public class Worker : MonoBehaviour
	{
		abstract protected EventWaitHandle Handle { get; }

		abstract public BaseDispatcher Dispatcher { get; }

		virtual protected void OnEnable()
		{
			Handle.Set();
		}

		virtual protected void OnDisable()
		{
			Handle.Reset();
		}

		virtual protected void OnDestroy()
		{
			Handle.Reset();

			var disposable = Handle as IDisposable;

			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}
}
