using System.Threading;

namespace InsaneLibrary.Threading
{
	static public class DispatcherExtension
	{
		static public void SetAsMain(this Thread @this)
		{
			Dispatcher.SetAsMain(@this);
		}

		static public Dispatcher GetDispatcher(this Thread @this)
		{
			if (@this == null)
			{
				@this.ToString();
			}

			return Dispatcher.GetDispatcher(@this);
		}
	}
}
