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
	/// <summary>
	/// The root access for observing and raising ECS Events 
	/// </summary>
	public interface IEventBus
	{
		/// <summary>
		/// For internal use, only.  External modification of events world can lead to Unforeseen Consequences and Eldritch Code Horrors.
		/// </summary>
		/// <returns></returns>
		EcsWorld GetEventsWorld();

		/// <summary>
		/// Clears the Event Bus, breaking all subscriptions, and freeing memory. 
		/// </summary>
		void Destroy();

		IEventBus_Uniques UniqueEvents { get; }
		IEventBus_Globals GlobalEvents { get; }
		IEventBus_EntityEvents EntityEvents { get; }
		IEventBus_FlagComponents FlagComponents { get; }

		/// <summary>
		/// Returns a processor for all remaining events.
		/// </summary>
		/// <returns></returns>
		IEcsSystem AllEventsProcessor();
	}
}