using InsaneLibrary.Threading;
using System.Threading;

namespace InsanePluginLibrary.Threading
{
	sealed internal class MainDispatcher : BaseDispatcher
	{
		public override bool CheckAccess()
		{
			return IsOnMainThread();
		}

		static internal bool IsOnMainThread()
		{
			var thread = Thread.CurrentThread;

			return (
				thread.GetApartmentState() == ApartmentState.Unknown &&
				thread.ManagedThreadId == 1 &&
				!thread.IsBackground &&
				!thread.IsThreadPoolThread);
		}
	}
}
