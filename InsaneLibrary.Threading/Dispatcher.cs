using System;
using System.Collections.Generic;
using System.Threading;

namespace InsaneLibrary.Threading
{
	public sealed class Dispatcher : BaseDispatcher
	{
		readonly static private object _sync = new object();
		readonly static private IDictionary<WeakReference, Dispatcher> _dispatcherList = new Dictionary<WeakReference, Dispatcher>(new WeakThreadComparer());

		static private WeakReference _mainThread;

		readonly private WeakReference _thread;

		internal Dispatcher(Thread thread) : base()
		{
			_thread = new WeakReference(thread);
		}

		public static Dispatcher CurrentDispatcher
		{
			get { return GetDispatcher(Thread.CurrentThread); }
		}

		public static Dispatcher MainDispatcher
		{
			get
			{
				var thread = _mainThread != null ? _mainThread.Target as Thread : null;

				return GetDispatcher(thread);
			}
		}

		public Thread Thread
		{
			get { return _thread.Target as Thread; }
		}

		static internal void SetAsMain(Thread thread)
		{
			lock (_sync)
			{
				_mainThread = new WeakReference(thread);
			}
		}

		static internal Dispatcher GetDispatcher(Thread thread)
		{
			lock (_sync)
			{
				Dispatcher dispatcher;

				var @ref = new WeakReference(thread);

				if (_dispatcherList.TryGetValue(@ref, out dispatcher))
				{
					return dispatcher;
				}

				return _dispatcherList[@ref] = new Dispatcher(thread);
			}
		}

		override public bool CheckAccess()
		{
			return Thread == Thread.CurrentThread;
		}
	}
}
