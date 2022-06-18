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
	public class EventBus_FlagComponents : IEventBus_FlagComponents
	{
		private readonly List<IEcsRunSystem> _flagComponentProcessors = new();
		private readonly Dictionary<Type, IFlagComponentSubscription> _flagComponentSubscriptions = new();

		private readonly IEventBus _eventBusRoot;

		public EcsWorld GetEventsWorld() => _eventBusRoot.GetEventsWorld();



		public EventBus_FlagComponents(IEventBus eventBusRoot)
		{
			_eventBusRoot = eventBusRoot;
		}
		
		
		public void Destroy()
		{
			ReleaseAll();
		}


		public void ReleaseAll()
		{
			_flagComponentSubscriptions.Clear();
			_flagComponentProcessors.Clear();
		}


		/// <summary>
		/// Handles adding a flag component to an entity.  Note:  A higher performance variant exists.
		/// </summary>
		/// <param name="targetEntity"></param>
		/// <param name="optionalCachedPool"></param>
		/// <typeparam name="T"></typeparam>
		public void Add<T>(EcsPackedEntityWithWorld targetEntity, EcsPool<T> optionalCachedPool = default)
			where T : struct, IFlagComponent
		{
			if (!targetEntity.Unpack(out var world, out var entity)) return;
			
			optionalCachedPool ??= world.GetPool<T>();
			if (!optionalCachedPool.Has(entity))
				optionalCachedPool.Add(entity);
		}


		/// <summary>
		/// A higher performance variant of AddFlagComponent.  Adds the flag, but takes the entity and pool directly.
		/// </summary>
		/// <param name="targetEntity"></param>
		/// <param name="cachedPool"></param>
		/// <typeparam name="T"></typeparam>
		public void Add<T>(int targetEntity, EcsPool<T> cachedPool) where T : struct, IFlagComponent
		{
			if (!cachedPool.Has(targetEntity))
				cachedPool.Add(targetEntity);
		}


		private FlagComponentSubscription<T> Fetch<T>(EcsPackedEntityWithWorld targetEntity) where T : struct, IFlagComponent
		{
			if (!targetEntity.Unpack(out var targetWorld, out var targetEntityID))
			{
				return null;
			}
			
			
			if (!_flagComponentSubscriptions.TryGetValue(typeof(T), out var flagComponentSubscription))
			{
				flagComponentSubscription = new FlagComponentSubscription<T>();
				_flagComponentSubscriptions.Add(typeof(T), flagComponentSubscription);
				_flagComponentProcessors.Add(ProcessorFor<T>(targetWorld));
			}

			if (flagComponentSubscription is FlagComponentSubscription<T> castedSubscription)
				return castedSubscription;
			throw new InvalidOperationException();
		}


		public void ListenTo<T>(EcsPackedEntityWithWorld targetEntity, Action<EcsPackedEntityWithWorld> whenEventFired) where T : struct, IFlagComponent
		{
			Fetch<T>(targetEntity).ListenTo(targetEntity, whenEventFired);
		}


		public IDisposable SubscribeTo<T>(EcsPackedEntityWithWorld targetEntity, Action<EcsPackedEntityWithWorld> whenEventFired) where T : struct, IFlagComponent
		{
			return Fetch<T>(targetEntity).SubscribeTo(targetEntity, whenEventFired);
		}


		public void RemoveListener<T>(EcsPackedEntityWithWorld targetEntity, Action<EcsPackedEntityWithWorld> whenEventFired) where T : struct, IFlagComponent
		{
			Fetch<T>(targetEntity)?.RemoveListener(targetEntity, whenEventFired);
		}


		public IEcsRunSystem ProcessorFor<T>(EcsWorld world) where T : struct, IFlagComponent
		{
			return new FlagComponentSystem<T>(this, world);
		}


		public IEcsRunSystem ProcessorAll()
		{
			return new AllFlagComponentSystem(this);
		}
		

		internal bool GetSubscriptionData<T>(out IFlagComponentSubscription subscription)
		{
			subscription = default;

			return _flagComponentSubscriptions.TryGetValue(typeof(T), out subscription);
		}
		

		internal void InvokeAll(EcsSystems systems)
		{
			foreach (var flagComponentProcessor in _flagComponentProcessors)
			{
				flagComponentProcessor.Run(systems);
			}			
		}
	}
}