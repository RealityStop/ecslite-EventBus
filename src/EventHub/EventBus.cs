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
	public partial class EventsBus : IEventBus
	{
		private readonly EcsWorld _eventsWorld;


		
		public IEventBus_Uniques UniqueEvents { get; }
		public IEventBus_Globals GlobalEvents { get; }
		public IEventBus_EntityEvents EntityEvents { get; }
		public IEventBus_FlagComponents FlagComponents { get; }



		public EventsBus(int capacityEvents = 8, int capacityEventsSingleton = 8)
		{
			UniqueEvents = new EventsBus_Uniques(this, capacityEventsSingleton);
			GlobalEvents = new EventBus_Globals(this, capacityEvents);
			EntityEvents = new EventBus_EntityEvents(this);
			FlagComponents = new EventBus_FlagComponents(this);
			
			_eventsWorld = new EcsWorld();
			
		}


		/// <summary>
		///     External modification of events world can lead to Unforeseen Consequences
		/// </summary>
		/// <returns></returns>
		public EcsWorld GetEventsWorld()
		{
			return _eventsWorld;
		}


		public void Destroy()
		{
			((EventsBus_Uniques)UniqueEvents).Destroy();
			((EventBus_Globals)GlobalEvents).Destroy();
			((EventBus_EntityEvents)EntityEvents).Destroy();
			((EventBus_FlagComponents)FlagComponents).Destroy();
			_eventsWorld.Destroy();
		}


		public IEcsSystem AllEventsProcessor()
		{
			return new AllEventsProcessor(this);
		}
	}
}