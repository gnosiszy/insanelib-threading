using InsaneLibrary.Threading;
using System;
using System.Threading;

namespace InsanePluginLibrary.Threading
{
	sealed public class AsyncWorker : Worker
	{
		readonly private EventWaitHandle _handle;
		readonly private Thread _thread;
		readonly WeakReference _dispatcher;

		public AsyncWorker()
		{
			_handle = new ManualResetEvent(false);
			_thread = new Thread(Listener)
			{
				IsBackground = true
			};

			_dispatcher = new WeakReference(_thread.GetDispatcher());
		}

		protected override EventWaitHandle Handle
		{
			get { return _handle; }
		}

		public override BaseDispatcher Dispatcher
		{
			get { return _dispatcher.Target as BaseDispatcher; }
		}

		private void Listener()
		{
			var dispatcher = Dispatcher;

			try
			{
				while (dispatcher.Wait() && Handle.WaitOne())
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

		override protected void OnDestroy()
		{
			_thread.Abort();

			base.OnDestroy();
		}
	}
}
