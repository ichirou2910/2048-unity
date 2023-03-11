using System;
using UnityEngine;

namespace Events
{
	public static class EventDispatcherExtension
	{
		/// Use for registering with EventsManager
		public static void RegisterListener (this MonoBehaviour listener, EventID eventID, Action<object> callback)
		{
			EventDispatcher.Instance.RegisterListener(eventID, callback);
		}

		/// Post event with param
		public static void PublishEvent (this MonoBehaviour listener, EventID eventID, object param = null)
		{
			EventDispatcher.Instance.PublishEvent(eventID, param);
		}
	}
}
