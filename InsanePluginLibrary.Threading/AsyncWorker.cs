using InsaneLibrary.Threading;
using System;
using System.Threading;
using UnityEngine;

namespace InsanePluginLibrary.Threading
{
	sealed public class AsyncWorker : MonoBehaviour
	{
		readonly private EventWaitHandle _handle;

		readonly private WeakReference _dispatcher;
		readonly private Thread _thread;

		public AsyncWorker()
		{
			_handle = new ManualResetEvent(false);
			_thread = new Thread(Listener)
			{
				IsBackground = true
			};

			_dispatcher = new WeakReference(_thread.GetDispatcher());
		}

		public SimpleDispatcher Dispatcher
		{
			get { return _dispatcher.Target as SimpleDispatcher; }
		}

		private void Listener()
		{
			var dispatcher = Dispatcher;

			try
			{
				while (dispatcher.Wait() && _handle.WaitOne())
				{
					dispatcher.DispatchEntry();
				}
			}
			catch (ThreadAbortException)
			{
				if (dispatcher != null)
				{
					dispatcher.Clear();
				}
			}
		}

		private void Start()
		{
			_thread.Start();
		}

		private void OnEnable()
		{
			_handle.Set();
		}

		private void OnDisable()
		{
			_handle.Reset();
		}

		private void OnDestroy()
		{
			_thread.Abort();

			_handle.Reset();

			var disposable = _handle as IDisposable;

			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}
}
