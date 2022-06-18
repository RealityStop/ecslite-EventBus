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
	public class EventBus_Globals : IEventBus_Globals
	{
		private readonly List<IEcsRunSystem> _globalEventProcessors = new();
		private readonly Dictionary<Type, IGlobalEventSubscription> _globalSubscriptions = new();
		private readonly IEventBus _eventBusRoot;

		private readonly Dictionary<Type, EcsFilter> _cachedFilters;

		public EcsWorld GetEventsWorld() => _eventBusRoot.GetEventsWorld();


		public EventBus_Globals(IEventBus eventBusRoot, int capacityEvents)
		{
			_eventBusRoot = eventBusRoot;
			_cachedFilters = new Dictionary<Type, EcsFilter>(capacityEvents);
		}


		public ref T Add<T>() where T : struct, IEventGlobal
		{
			var newEntity = GetEventsWorld().NewEntity();
			return ref GetEventsWorld().GetPool<T>().Add(newEntity);
		}


		public bool Has<T>() where T : struct, IEventGlobal
		{
			var filter = GetFilter<T>();
			return filter.GetEntitiesCount() != 0;
		}


		public void Del<T>() where T : struct, IEventGlobal
		{
			foreach (var eventEntity in GetFilter<T>()) GetEventsWorld().DelEntity(eventEntity);
		}


		public void ListenTo<T>(IEventBus_Globals.GlobalEventActionRef<T> whenEventFired) where T : struct, IEventGlobal
		{
			Fetch<T>().ListenTo(whenEventFired);
		}


		private GlobalEventSubscription<T> Fetch<T>() where T : struct, IEventGlobal
		{
			if (!_globalSubscriptions.TryGetValue(typeof(T), out var eventSubscriptions))
			{
				eventSubscriptions = new GlobalEventSubscription<T>();
				_globalSubscriptions.Add(typeof(T), eventSubscriptions);
				_globalEventProcessors.Add(ProcessorFor<T>());
			}

			if (eventSubscriptions is GlobalEventSubscription<T> castedSubscription)
				return castedSubscription;
			else
				throw new InvalidOperationException();
		}


		public void RemoveListener<T>(IEventBus_Globals.GlobalEventActionRef<T> whenEventFired) where T : struct, IEventGlobal
		{
			Fetch<T>().RemoveListener(whenEventFired);
		}


		public IDisposable SubscribeTo<T>(IEventBus_Globals.GlobalEventActionRef<T> whenEventFired)
			where T : struct, IEventGlobal
		{
			return Fetch<T>().SubscribeTo(whenEventFired);
		}


		public IEcsRunSystem ProcessorFor<T>() where T : struct, IEventGlobal
		{
			return new GlobalEventSystem<T>(this, Fetch<T>());
		}


		public IEcsRunSystem ProcessorAll()
		{
			return new AllGlobalEventSystem(this);
		}


		internal void InvokeAll(EcsSystems systems)
		{
			foreach (var eventProcessor in _globalEventProcessors) eventProcessor.Run(systems);
		}


		private EcsFilter GetFilter<T>() where T : struct, IEventGlobal
		{
			var type = typeof(T);
			if (!_cachedFilters.TryGetValue(type, out var filter))
			{
				filter = GetEventsWorld().Filter<T>().End();
				_cachedFilters.Add(type, filter);
			}

			return filter;
		}


		public void Destroy()
		{
			ReleaseAll();
		}


		public void ReleaseAll()
		{
			_globalSubscriptions.Clear();
			_globalEventProcessors.Clear();
			_cachedFilters.Clear();
		}
	}
}