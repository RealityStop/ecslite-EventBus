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
	internal interface IUniqueEventSubscription
	{
		void Invoke(IEventUnique evt);
	}

	internal class UniqueEventSubscription<T> : IUniqueEventSubscription
	{
		private readonly List<IEventBus_Uniques.UniqueEventActionRef<T>>
			_actions = new();


		public void Invoke(IEventUnique evt)
		{
			if (evt is T castedEvent)
				foreach (var action in _actions)
					action.Invoke(ref castedEvent);
			else
				throw new ArgumentException(
					$"Event type '{evt.GetType().Name}' does not match expected type '{typeof(T).Name}'");
		}

		
		public void ListenTo(IEventBus_Uniques.UniqueEventActionRef<T> whenEventFired)
		{
			_actions.Add(whenEventFired);
		}
		

		public IDisposable SubscribeTo(IEventBus_Uniques.UniqueEventActionRef<T> whenEventFired)
		{
			_actions.Add(whenEventFired);
			return UnsubscribeRecord.Fetch().Assign(this, whenEventFired);
		}


		public void RemoveListener(IEventBus_Uniques.UniqueEventActionRef<T> whenEventFired)
		{
			_actions.Remove(whenEventFired);
		}


		private class UnsubscribeRecord : Pooled<UnsubscribeRecord>, IDisposable
		{
			private IEventBus_Uniques.UniqueEventActionRef<T> _action;
			private UniqueEventSubscription<T> _sub;


			public void Dispose()
			{
				_sub.RemoveListener(_action);
				Free(this);
			}


			internal UnsubscribeRecord Assign(UniqueEventSubscription<T> sub,
				IEventBus_Uniques.UniqueEventActionRef<T> action)
			{
				_sub = sub;
				_action = action;
				return this;
			}
		}


	}
}