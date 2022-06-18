/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */
namespace Leopotam.EcsLite
{
	public class FlagComponentSystem<T> : IEcsRunSystem where T : struct, IFlagComponent
	{
		private readonly IEventBus_FlagComponents _eventBus;
		private readonly bool _eventIsPersistent;

		private readonly EcsFilter _filter;
		private readonly EcsPool<T> _pool;
		private readonly EcsWorld _world;
		private IFlagComponentSubscription _subscriptionData;


		internal FlagComponentSystem(IEventBus_FlagComponents eventBus, EcsWorld world)
		{
			_eventBus = eventBus;
			_world = world;
			_pool = _world.GetPool<T>();
			_filter = _world.Filter<T>().End();
			_eventIsPersistent = typeof(IEventPersist).IsAssignableFrom(typeof(T));
		}


		public void Run(EcsSystems systems)
		{
			if (GetSubscriptionData(out var subData))
				foreach (var eventEntityID in _filter)
				{
					ref var eventComponent = ref _pool.Get(eventEntityID);
					_subscriptionData.Invoke(_world.PackEntityWithWorld(eventEntityID));

					if (!_eventIsPersistent)
						_pool.Del(eventEntityID);
				}
		}


		private bool GetSubscriptionData(out IFlagComponentSubscription subscriptionData)
		{
			if (_subscriptionData != null)
				subscriptionData = _subscriptionData;
			else if (((EventBus_FlagComponents)_eventBus).GetSubscriptionData<T>(out subscriptionData))
				_subscriptionData = subscriptionData;

			return subscriptionData != null;
		}
	}
}