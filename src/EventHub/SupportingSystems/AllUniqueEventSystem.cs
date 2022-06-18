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
	public class AllUniqueEventSystem : IEcsRunSystem
	{
		private readonly EventsBus_Uniques _eventBus;


		public AllUniqueEventSystem(EventsBus_Uniques eventsBus)
		{
			_eventBus = eventsBus;
		}


		public void Run(EcsSystems systems)
		{
			_eventBus.InvokeAll(systems);
		}
	}
}