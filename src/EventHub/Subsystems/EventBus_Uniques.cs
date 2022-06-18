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
	public class EventsBus_Uniques : IEventBus_Uniques
	{
		private readonly IEventBus _eventBusRoot;
		private readonly Dictionary<Type, int> _uniqueEntities;
		private readonly List<IEcsRunSystem> _uniqueEventProcessors = new();
		private readonly Dictionary<Type, IUniqueEventSubscription> _uniqueSubscriptions = new();


		public EventsBus_Uniques(IEventBus eventBusRoot, int capacityEventsSingleton)
		{
			_eventBusRoot = eventBusRoot;
			_uniqueEntities = new Dictionary<Type, int>(capacityEventsSingleton);
		}


		public EcsWorld GetEventsWorld()
		{
			return _eventBusRoot.GetEventsWorld();
		}


		public ref T Add<T>() where T : struct, IEventUnique
		{
			var type = typeof(T);
			var eventsPool = GetEventsWorld().GetPool<T>();
			if (!_uniqueEntities.TryGetValue(type, out var eventEntity))
			{
				eventEntity = GetEventsWorld().NewEntity();
				_uniqueEntities.Add(type, eventEntity);
				return ref eventsPool.Add(eventEntity);
			}

			return ref eventsPool.Get(eventEntity);
		}


		public bool Has<T>() where T : struct, IEventUnique
		{
			return _uniqueEntities.ContainsKey(typeof(T));
		}


		public void Del<T>() where T : struct, IEventUnique
		{
			var type = typeof(T);
			if (_uniqueEntities.TryGetValue(type, out var eventEntity))
			{
				GetEventsWorld().DelEntity(eventEntity);
				_uniqueEntities.Remove(type);
			}
		}


		/// <summary>
		///     Subscribes to a unique event without tracking, which means it is up to you to handle unsubscribing
		///     appropriately (don't use lambdas)
		/// </summary>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		public void ListenTo<T>(IEventBus_Uniques.UniqueEventActionRef<T> whenEventFired) where T : struct, IEventUnique
		{
			Fetch<T>().ListenTo(whenEventFired);
		}


		/// <summary>
		///     Subscribes to a unique event with tracking, which means simply dispose the return value to
		///     end the subscription.  A small amount of memory is allocated to contain the tracking information (2 ints).
		/// </summary>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public IDisposable SubscribeTo<T>(IEventBus_Uniques.UniqueEventActionRef<T> whenEventFired)
			where T : struct, IEventUnique
		{
			return Fetch<T>().SubscribeTo(whenEventFired);
		}


		/// <summary>
		///     When using the non-tracking subscription call, you have to manually call Unsubscribe to end a subscription
		/// </summary>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		public void RemoveListener<T>(IEventBus_Uniques.UniqueEventActionRef<T> whenEventFired)
			where T : struct, IEventUnique
		{
			Fetch<T>().RemoveListener(whenEventFired);
		}


		public IEcsRunSystem ProcessorFor<T>() where T : struct, IEventUnique
		{
			return new UniqueEventSystem<T>(this, Fetch<T>());
		}


		public IEcsRunSystem ProcessorAll()
		{
			return new AllUniqueEventSystem(this);
		}


		public void Destroy()
		{
			ReleaseAll();
		}


		public void ReleaseAll()
		{
			_uniqueSubscriptions.Clear();
			_uniqueEventProcessors.Clear();
			_uniqueEntities.Clear();
		}


		private UniqueEventSubscription<T> Fetch<T>() where T : struct, IEventUnique
		{
			IUniqueEventSubscription eventSubscriptions = default;
			if (!_uniqueSubscriptions.TryGetValue(typeof(T), out eventSubscriptions))
			{
				eventSubscriptions = new UniqueEventSubscription<T>();
				_uniqueSubscriptions.Add(typeof(T), eventSubscriptions);
				_uniqueEventProcessors.Add(ProcessorFor<T>());
			}

			if (eventSubscriptions is UniqueEventSubscription<T> castedSubscription)
				return castedSubscription;
			throw new InvalidOperationException();
		}


		internal void InvokeAll(EcsSystems systems)
		{
			foreach (var eventProcessor in _uniqueEventProcessors) eventProcessor.Run(systems);
		}
	}
}