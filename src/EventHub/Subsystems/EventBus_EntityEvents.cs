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
	public class EventBus_EntityEvents : IEventBus_EntityEvents
	{
		private readonly List<IEcsRunSystem> _entityEventProcessors = new();
		private readonly Dictionary<Type, IEntityEventSubscription> _entitySubscriptions = new();
		private readonly IEventBus _eventBusRoot;

		public EcsWorld GetEventsWorld() => _eventBusRoot.GetEventsWorld();


		public EventBus_EntityEvents(IEventBus eventBusRoot)
		{
			_eventBusRoot = eventBusRoot;
		}


		public void Destroy()
		{
			ReleaseAll();
		}


		public void ReleaseAll()
		{
			_entitySubscriptions.Clear();
			_entityEventProcessors.Clear();
		}


		public ref T Add<T>(EcsPackedEntityWithWorld targetEntity, EcsPool<T> optionalCachedPool = default)
			where T : struct, IEventEntity
		{
			var newEntity = GetEventsWorld().NewEntity();

			optionalCachedPool ??= GetEventsWorld().GetPool<T>();

			ref var component = ref optionalCachedPool.Add(newEntity);
			component.Source = targetEntity;
			return ref component;
		}


		private EntityEventSubscription<T> Fetch<T>() where T : struct, IEventEntity
		{
			IEntityEventSubscription eventSubscriptions = default;
			if (!_entitySubscriptions.TryGetValue(typeof(T), out eventSubscriptions))
			{
				eventSubscriptions = new EntityEventSubscription<T>();
				_entitySubscriptions.Add(typeof(T), eventSubscriptions);
				_entityEventProcessors.Add(ProcessorFor<T>());
			}

			if (eventSubscriptions is EntityEventSubscription<T> castedSubscription)
				return castedSubscription;

			throw new InvalidOperationException();
		}


		public void ListenTo<T>(EcsPackedEntityWithWorld targetEntity, IEventBus_EntityEvents.EntityEventActionRef<T> whenEventFired) where T : struct, IEventEntity
		{
			Fetch<T>().ListenTo(targetEntity, whenEventFired);
		}


		public IDisposable SubscribeTo<T>(EcsPackedEntityWithWorld targetEntity,
			IEventBus_EntityEvents.EntityEventActionRef<T> whenEventFired) where T : struct, IEventEntity
		{
			return Fetch<T>().SubscribeTo(targetEntity, whenEventFired);
		}


		public void RemoveListener<T>(EcsPackedEntityWithWorld targetEntity, IEventBus_EntityEvents.EntityEventActionRef<T> whenEventFired) where T : struct, IEventEntity
		{
			Fetch<T>().RemoveListener(targetEntity, whenEventFired);
		}


		public IEcsRunSystem ProcessorFor<T>() where T : struct, IEventEntity
		{
			return new EntityEventSystem<T>(this);
		}


		public IEcsRunSystem ProcessorAll()
		{
			return new AllEntityEventSystem(this);
		}


		internal bool GetSubscriptionData<T>(out IEntityEventSubscription subscription)
		{
			return _entitySubscriptions.TryGetValue(typeof(T), out subscription);
		}


		internal void InvokeAll(EcsSystems systems)
		{
			foreach (var eventProcessor in _entityEventProcessors) eventProcessor.Run(systems);
		}
	}
}