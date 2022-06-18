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
	public class AllFlagComponentSystem : IEcsRunSystem
	{
		private readonly IEventBus_FlagComponents _eventsBus;


		public AllFlagComponentSystem(IEventBus_FlagComponents eventsBus)
		{
			_eventsBus = eventsBus;
		}


		public void Run(EcsSystems systems)
		{
			((EventBus_FlagComponents)_eventsBus).InvokeAll(systems);
		}
	}
}