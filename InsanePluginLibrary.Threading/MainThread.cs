using InsaneLibrary.Threading;
using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace InsanePluginLibrary.Threading
{
	sealed public class MainThread : MonoBehaviour
	{
		private const string DEFAULT_GAMEOBJECT_NAME = "_MAIN_THREAD_HELPER";

		readonly static private object _sync = new object();
		static private MainThread _current;

		readonly private EventWaitHandle _handle;

		readonly private SimpleDispatcher _dispatcher;
		readonly private Stopwatch _watcher;

		[SerializeField]
		private float _timeout = float.MaxValue;

		public MainThread()
		{
			if (!IsOnMainThread())
			{
				throw new InvalidOperationException();
			}

			_handle = new ManualResetEvent(false);
			_dispatcher = new MainDispatcher();
			_watcher = new Stopwatch();

			if (Current != null)
			{
				Destroy(Current);
			}

			Current = this;
		}

		static public MainThread Current
		{
			get { lock (_sync) return _current; }
			private set { lock (_sync) _current = value; }
		}

		static public SimpleDispatcher CurrentDispatcher
		{
			get { return Current.Dispatcher; }
		}

		public SimpleDispatcher Dispatcher
		{
			get { return _dispatcher; }
		}

		public TimeSpan Timeout
		{
			get { return TimeSpan.FromSeconds(_timeout); }
			set { _timeout = (float)value.TotalSeconds; }
		}

		static private bool Initialize(GameObject gameObject)
		{
			try
			{
				gameObject.AddComponent<MainThread>();
			}
			catch
			{
				return false;
			}

			return true;
		}

		static public bool Initialize()
		{
			if (!IsOnMainThread())
			{
				return false;
			}

			if (Current != null)
			{
				return true;
			}

			return Initialize(new GameObject(DEFAULT_GAMEOBJECT_NAME));
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

		private void Awake()
		{
			DontDestroyOnLoad(this);
			DontDestroyOnLoad(gameObject);
		}

		private void OnEnable()
		{
			_handle.Set();
		}

		private void OnDisable()
		{
			_handle.Reset();
		}

		private void Update()
		{
			var dispatcher = Dispatcher;

			_watcher.Reset();
			_watcher.Start();

			while (dispatcher.DispatchEntry() && _watcher.Elapsed < Timeout) { }
		}

		private void OnDestroy()
		{
			_handle.Reset();

			var disposable = _handle as IDisposable;

			if (disposable != null)
			{
				disposable.Dispose();
			}

			Current = null;
		}
	}
}
