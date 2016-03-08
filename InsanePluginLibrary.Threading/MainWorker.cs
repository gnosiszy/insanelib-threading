using InsaneLibrary.Threading;
using System;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace InsanePluginLibrary.Threading
{
	sealed public class MainWorker : Worker
	{
		private const string DEFAULT_GAMEOBJECT_NAME = "_MAIN_THREAD_HELPER";

		readonly static private object _sync = new object();
		static private MainWorker _current;

		readonly private EventWaitHandle _handle;

		readonly private BaseDispatcher _dispatcher;
		readonly private Stopwatch _watcher;

		[SerializeField]
		private float _timeout = 0xFF;

		public MainWorker()
		{
			if (!IsOnMainThread())
			{
				throw new InvalidOperationException();
			}

			_handle = new ManualResetEvent(false);
			_dispatcher = new MainDispatcher();
			_watcher = new Stopwatch();

			MainWorker current;

			lock (_sync)
			{
				current = _current;
				_current = this;
			}

			if (current != null)
			{
				Destroy(current);
			}
		}

		static public MainWorker Current
		{
			get { lock (_sync) return _current; }
		}

		static public BaseDispatcher CurrentDispatcher
		{
			get { lock (_sync) return _current != null ? _current.Dispatcher : null; }
		}

		protected override EventWaitHandle Handle
		{
			get { return _handle; }
		}

		override public BaseDispatcher Dispatcher
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
				gameObject.AddComponent<MainWorker>();
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
			return MainDispatcher.IsOnMainThread();
		}

		private void Awake()
		{
			DontDestroyOnLoad(this);
			DontDestroyOnLoad(gameObject);
		}

		private void Update()
		{
			var dispatcher = Dispatcher;

			_watcher.Reset();
			_watcher.Start();

			while (dispatcher.DispatchEntry() && _watcher.Elapsed < Timeout) { }
		}

		override protected void OnDestroy()
		{
			base.OnDestroy();

			lock (_sync)
			{
				if (_current == this)
				{
					_current = null;
				}
			}
		}
	}
}
