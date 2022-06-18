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
	public class AllEventsSystem : IEcsRunSystem
	{
		private readonly EventsBus _eventBus;


		public AllEventsSystem(EventsBus eventsBus)
		{
			_eventBus = eventsBus;
		}


		public void Run(EcsSystems systems)
		{
			((EventBus_FlagComponents)_eventBus.FlagComponents).InvokeAll(systems);
			((EventBus_EntityEvents)_eventBus.EntityEvents).InvokeAll(systems);
			((EventBus_Globals)_eventBus.GlobalEvents).InvokeAll(systems);
			((EventsBus_Uniques)_eventBus.UniqueEvents).InvokeAll(systems);
		}
	}
}