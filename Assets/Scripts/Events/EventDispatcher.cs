using System;
using System.Collections.Generic;
using UnityEngine;

namespace Events
{
	public class EventDispatcher : MonoBehaviour
	{
		private readonly Dictionary<EventID, Action<object>> _listeners = new();

		static EventDispatcher _instance;
		public static EventDispatcher Instance
		{
			get
			{
				// instance not exist, then create new one
				if (_instance == null)
				{
					GameObject singletonObject = new GameObject();
					_instance = singletonObject.AddComponent<EventDispatcher>();
					singletonObject.name = "EventDispatcher";
				}
				return _instance;
			}
			private set { }
		}

		void Awake ()
		{
			// check if there's another instance already exist in scene
			if (_instance != null && _instance.GetInstanceID() != this.GetInstanceID())
			{
				Destroy(gameObject);
			}
			else
			{
				_instance = this;
			}
		}


		void OnDestroy ()
		{
			if (_instance == this)
			{
				ClearAllListener();
				_instance = null;
			}
		}

		public void RegisterListener (EventID eventID, Action<object> callback)
		{
			if (_listeners.ContainsKey(eventID))
			{
				_listeners[eventID] += callback;
			}
			else
			{
				_listeners.Add(eventID, null);
				_listeners[eventID] += callback;
			}
		}

		public void PublishEvent (EventID eventID, object param = null)
		{
			if (!_listeners.ContainsKey(eventID))
			{
				return;
			}

			var callbacks = _listeners[eventID];
			if (callbacks != null)
			{
				callbacks(param);
			}
			else
			{
				_listeners.Remove(eventID);
			}
		}

		public void ClearAllListener ()
		{
			_listeners.Clear();
		}
	}
}