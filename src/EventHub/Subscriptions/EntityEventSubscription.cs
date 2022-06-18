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
using UnityEditor.Experimental.GraphView;

namespace Leopotam.EcsLite
{
	internal interface IEntityEventSubscription
	{
		void Invoke(EcsPackedEntityWithWorld source, IEventEntity evt);
	}


	internal class EntityEventSubscription<T> : IEntityEventSubscription
	{
		private readonly Dictionary<EcsPackedEntityWithWorld, List<IEventBus_EntityEvents.EntityEventActionRef<T>>>
			_subscriptionsByEntity = new();


		public void Invoke(EcsPackedEntityWithWorld source, IEventEntity evt)
		{
			if (evt is T castedEvent)
			{
				if (_subscriptionsByEntity.TryGetValue(source, out var actions))
						foreach (var action in actions)
							action.Invoke(source, ref castedEvent);
			}
			else
			{
				throw new ArgumentException(
					$"Event type '{evt.GetType().Name}' does not match expected type '{typeof(T).Name}'");
			}
		}
		
		public void ListenTo(EcsPackedEntityWithWorld source, IEventBus_EntityEvents.EntityEventActionRef<T> whenEventFired)
		{
			if (!_subscriptionsByEntity.TryGetValue(source, out var actions))
			{
				actions = new List<IEventBus_EntityEvents.EntityEventActionRef<T>>();
				_subscriptionsByEntity.Add(source, actions);
			}

			actions.Add(whenEventFired);
		}


		public IDisposable SubscribeTo(EcsPackedEntityWithWorld source, IEventBus_EntityEvents.EntityEventActionRef<T> whenEventFired)
		{
			if (!_subscriptionsByEntity.TryGetValue(source, out var actions))
			{
				actions = new List<IEventBus_EntityEvents.EntityEventActionRef<T>>();
				_subscriptionsByEntity.Add(source, actions);
			}

			actions.Add(whenEventFired);

			return UnsubscribeRecord.Fetch().Assign(this, source, whenEventFired);
		}


		public void RemoveListener(EcsPackedEntityWithWorld source, IEventBus_EntityEvents.EntityEventActionRef<T> whenEventFired)
		{
			if (_subscriptionsByEntity.TryGetValue(source, out var actions))
				actions.Remove(whenEventFired);
		}


		private class UnsubscribeRecord : Pooled<UnsubscribeRecord>, IDisposable
		{
			private IEventBus_EntityEvents.EntityEventActionRef<T> _action;
			private EcsPackedEntityWithWorld _entity;
			private EntityEventSubscription<T> _sub;

			
			internal UnsubscribeRecord Assign(EntityEventSubscription<T> sub, EcsPackedEntityWithWorld entity, IEventBus_EntityEvents.EntityEventActionRef<T> action)
			{
				_sub = sub;
				_entity = entity;
				_action = action;
				return this;
			}


			public void Dispose()
			{
				_sub.RemoveListener(_entity, _action);
				Free(this);
			}
		}
	}
}