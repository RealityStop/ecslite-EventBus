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
	internal class GlobalEventSystem<T> : IEcsRunSystem where T : struct, IEventGlobal
	{
		private readonly IEventBus_Globals _eventbus;
		private readonly bool _eventIsPersistent;
		private readonly EcsFilter _filter;
		private readonly EcsPool<T> _pool;
		private readonly IGlobalEventSubscription _subscriptionData;


		public GlobalEventSystem(IEventBus_Globals bus, IGlobalEventSubscription subscriptionData)
		{
			_eventbus = bus;
			_pool = _eventbus.GetEventsWorld().GetPool<T>();
			_filter = _eventbus.GetEventsWorld().Filter<T>().End();
			_subscriptionData = subscriptionData;

			_eventIsPersistent = typeof(IEventPersist).IsAssignableFrom(typeof(T));
		}


		public void Run(EcsSystems systems)
		{
			foreach (var eventEntityID in _filter)
			{
				ref var eventComponent = ref _pool.Get(eventEntityID);
				_subscriptionData.Invoke(eventComponent);
			}

			if (!_eventIsPersistent)
				_eventbus.Del<T>();
		}
	}
}