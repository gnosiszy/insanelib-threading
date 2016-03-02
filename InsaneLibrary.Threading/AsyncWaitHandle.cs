using System;
using System.Threading;

namespace InsaneLibrary.Threading
{
	sealed public class AsyncWaitHandle
	{
		readonly private EventWaitHandle _handle = new ManualResetEvent(false);
		readonly private Action _method;

		internal AsyncWaitHandle(Action method)
		{
			_method = method;
		}

		public Action Method
		{
			get { return _method; }
		}

		public string MethodName
		{
			get { return _method.Method.Name; }
		}

		internal bool Invoke()
		{
			_method.Invoke();

			return _handle.Set();
		}

		public bool Wait()
		{
			return _handle.WaitOne();
		}

		public bool Wait(TimeSpan timeout)
		{
			return _handle.WaitOne(timeout);
		}
	}
}
