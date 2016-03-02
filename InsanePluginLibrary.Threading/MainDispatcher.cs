using InsaneLibrary.Threading;

namespace InsanePluginLibrary.Threading
{
	sealed internal class MainDispatcher : SimpleDispatcher
	{
		public override bool CheckAccess()
		{
			return true;
		}
	}
}
