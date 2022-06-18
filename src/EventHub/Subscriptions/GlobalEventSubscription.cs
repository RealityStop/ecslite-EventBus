/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */
using System;
using System.Collections.Generic;

namespace Leopotam.EcsLite
{
	internal interface IGlobalEventSubscription
	{
		void Invoke(IEventGlobal evt);
	}

	internal class GlobalEventSubscription<T> : IGlobalEventSubscription
	{
		private readonly List<IEventBus_Globals.GlobalEventActionRef<T>>
			_actions = new();


		public void Invoke(IEventGlobal evt)
		{
			if (evt is T castedEvent)
				foreach (var action in _actions)
					action.Invoke(ref castedEvent);
			else
				throw new ArgumentException(
					$"Event type '{evt.GetType().Name}' does not match expected type '{typeof(T).Name}'");
		}

		
		public void ListenTo(IEventBus_Globals.GlobalEventActionRef<T> whenEventFired)
		{
			_actions.Add(whenEventFired);
		}

		public IDisposable SubscribeTo(IEventBus_Globals.GlobalEventActionRef<T> whenEventFired)
		{
			_actions.Add(whenEventFired);

			return UnsubscribeRecord.Fetch().Assign(this, whenEventFired);
		}


		public void RemoveListener(IEventBus_Globals.GlobalEventActionRef<T> whenEventFired)
		{
			_actions.Remove(whenEventFired);
		}


		private class UnsubscribeRecord : Pooled<UnsubscribeRecord>, IDisposable
		{
			private IEventBus_Globals.GlobalEventActionRef<T> _action;
			private GlobalEventSubscription<T> _sub;


			public void Dispose()
			{
				_sub.RemoveListener(_action);
				Free(this);
			}


			internal UnsubscribeRecord Assign(GlobalEventSubscription<T> sub,
				IEventBus_Globals.GlobalEventActionRef<T> action)
			{
				_sub = sub;
				_action = action;
				return this;
			}
		}
		
		
		/*private class UnsubscribeDisposable : IDisposable
		{
			private readonly IEventBus_Globals.GlobalEventActionRef<T> _action;
			private GlobalEventSubscription<T> _sub;


			public UnsubscribeDisposable(GlobalEventSubscription<T> sub, IEventBus_Globals.GlobalEventActionRef<T> action)
			{
				_sub = sub;
				_action = action;
			}


			public void Dispose()
			{
				_sub.RemoveListener(_action);
			}
		}*/
	}
}