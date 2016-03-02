using System;
using System.Collections.Generic;
using System.Threading;

namespace InsaneLibrary.Threading
{
	internal class WeakThreadComparer : EqualityComparer<WeakReference>
	{
		override public bool Equals(WeakReference x, WeakReference y)
		{
			var tx = x.Target as Thread;
			var ty = y.Target as Thread;

			if (tx == null)
			{
				return ty == null;
			}

			if (ty == null)
			{
				return false;
			}

			return tx == ty;
		}

		override public int GetHashCode(WeakReference obj)
		{
			var thread = obj.Target as Thread;

			if (thread == null)
			{
				return 0;
			}

			return thread.GetHashCode();
		}
	}
}
