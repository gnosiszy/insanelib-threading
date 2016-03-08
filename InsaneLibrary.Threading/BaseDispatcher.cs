using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InsaneLibrary.Threading
{
	public abstract class BaseDispatcher
	{
		readonly private object _sync = new object();

		readonly private LinkedList<Task> _queue;
		readonly private EventWaitHandle _monitor;

		public event Action<BaseDispatcher, Exception> Error;

		protected BaseDispatcher()
		{
			_queue = new LinkedList<Task>();
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

		public virtual Task<T> Invoke<T>(Func<T> method)
		{
			var task = new Task<T>(method);

			InvokeAsync(task);

			task.Wait();

			return task;
		}

		public virtual object Invoke(Delegate method, params object[] args)
		{
			var task = new Task<object>(() => method.DynamicInvoke(args));

			InvokeAsync(task);

			task.Wait();

			return task.Result;
		}

		public virtual void Invoke(Action method)
		{
			InvokeAsync(method).Wait();
		}

		public virtual void Invoke(Task task)
		{
			InvokeAsync(task);

			task.Wait();
		}

		public virtual Task<object> InvokeAsync(Delegate method, params object[] args)
		{
			var task = new Task<object>(() => method.DynamicInvoke(args));

			lock (_sync)
			{
				Enqueue(task);
			}

			return task;
		}

		public virtual Task<T> InvokeAsync<T>(Func<T> method)
		{
			var task = new Task<T>(method);

			lock (_sync)
			{
				Enqueue(task);
			}

			return task;
		}

		public virtual Task InvokeAsync(Action method)
		{
			var task = new Task(method);

			lock (_sync)
			{
				Enqueue(task);
			}

			return task;
		}

		public virtual void InvokeAsync(Task task)
		{
			lock (_sync)
			{
				Enqueue(task);
			}
		}

		public virtual bool Wait()
		{
			return _monitor.WaitOne();
		}

		public virtual bool Wait(TimeSpan timeout)
		{
			return _monitor.WaitOne(timeout);
		}

		public virtual bool Cancel(Task task)
		{
			lock (_sync)
			{
				_queue.Remove(task);
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

		private void Enqueue(Task task)
		{
			_queue.AddLast(task);
			_monitor.Set();
		}

		private bool TryDequeue(out Task task)
		{
			bool result;

			lock (_sync)
			{
				result = _queue.Count != 0;

				if (result)
				{
					task = _queue.First.Value;
					_queue.RemoveFirst();
				}
				else
				{
					task = null;
					_monitor.Reset();
				}
			}

			return result;
		}

		public virtual bool DispatchEntry()
		{
			Task task;

			if (!TryDequeue(out task))
			{
				return false;
			}

			try
			{
				task.RunSynchronously();
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
