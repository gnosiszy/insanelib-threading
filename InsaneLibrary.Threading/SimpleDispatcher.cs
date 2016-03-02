using System;
using System.Collections.Generic;
using System.Threading;

namespace InsaneLibrary.Threading
{
	public abstract class SimpleDispatcher
	{
		readonly private object _sync = new object();

		readonly private LinkedList<AsyncWaitHandle> _queue;
		readonly private EventWaitHandle _monitor;

		public event Action<SimpleDispatcher, Exception> Error;

		protected SimpleDispatcher()
		{
			_queue = new LinkedList<AsyncWaitHandle>();
			_monitor = new ManualResetEvent(false);
		}

		protected virtual void OnError(Exception ex)
		{
			if (Error != null)
			{
				Error(this, ex);
			}
		}

		abstract public bool CheckAccess();

		public virtual bool Invoke(Action method)
		{
			return InvokeAsync(method).Wait();
		}

		public virtual AsyncWaitHandle InvokeAsync(Action method)
		{
			var handle = new AsyncWaitHandle(method);

			lock (_sync)
			{
				_queue.AddLast(handle);
				_monitor.Set();
			}

			return handle;
		}

		public virtual bool Wait()
		{
			return _monitor.WaitOne();
		}

		public virtual bool Wait(TimeSpan timeout)
		{
			return _monitor.WaitOne(timeout);
		}

		public virtual bool Cancel(AsyncWaitHandle handle)
		{
			lock (_sync)
			{
				_queue.Remove(handle);
			}

			return true;
		}

		public virtual void Clear()
		{
			lock (_sync)
			{
				_queue.Clear();
			}
		}

		private bool TryDequeue(out AsyncWaitHandle handle)
		{
			bool result;

			lock (_sync)
			{
				result = _queue.Count != 0;

				if (result)
				{
					handle = _queue.First.Value;
					_queue.RemoveFirst();
				}
				else
				{
					handle = null;
					_monitor.Reset();
				}
			}

			return result;
		}

		public virtual bool DispatchEntry()
		{
			AsyncWaitHandle handle;

			if (!TryDequeue(out handle))
			{
				return false;
			}

			try
			{
				handle.Invoke();
			}
			catch (Exception ex)
			{
				OnError(ex);
			}

			return true;
		}

		public virtual bool DispatchAll()
		{
			if (!CheckAccess())
			{
				return false;
			}

			while (DispatchEntry()) { }

			return true;
		}
	}
}
