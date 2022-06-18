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
	public static class EventBusExt
	{
		public static EcsSystems AddWorld(this EcsSystems systems, IEventBus bus)
		{
			systems.AddWorld(bus.GetEventsWorld(), "__EVENTS");
			return systems;
		}
		
		public static EcsSystems AddAllEvents(this EcsSystems systems, IEventBus bus)
		{
			systems.Add(bus.AllEventsProcessor());
			return systems;
		}
	}
}