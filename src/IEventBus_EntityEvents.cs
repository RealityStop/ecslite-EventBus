/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */
using System;

namespace Leopotam.EcsLite
{
	/// <summary>
	/// **Entity Events** are data-carrying events that are attached to the Entity and multiple can be raised at a time.
	/// This can be useful for things like collisions, where the data about the collision is important, and we can't use
	/// components on the entities, since multiple might occur at once.  While it would be possible to track the
	/// collision data in a collection in the component, we'd need an *additional* component to signal when the
	/// collection is non-empty, and the observers would each have to unpack and collect the appropriate data.  Using
	/// Entity Events abstracts that away by storing the event data in a separate world (to prevent memory being
	/// reserved for each event type in your main worlds) and delivering the data directly to the observers.
	/// 
	///> Note: due to the optimizations required to store multiple entity events in a performant manner, it is not
	/// possible to manually check for or fetch Entity Events.  The Event Bus will notify listeners at the correct time,
	/// but it does not off the ability to query for the presence of them.
	/// </summary>
	public interface IEventBus_EntityEvents : IEventBus_Subsystem
	{
		/// <summary>
		/// Entity Event listeners should take the packed reference and the actual event data.  In theory, listeners can
		/// save the unpacked world and entity and use them, but when processing in bulk that isn't practical, so it is
		/// passed in the callback.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		delegate void EntityEventActionRef<T>(EcsPackedEntityWithWorld packed, ref T item);

		/// <summary>
		/// Raises an event on the target entity.  For performance, the pool can be passed.
		/// </summary>
		/// <param name="targetEntity">The target entity that should be flagged</param>
		/// <param name="optionalCachedPool">
		///     The pool to use to flag with.  Optional for convenience, but for frequent operations
		///     this should be provided to minimize performance impact.</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		ref T Add<T>(EcsPackedEntityWithWorld targetEntity, EcsPool<T> optionalCachedPool = default)
			where T : struct, IEventEntity;


		/// <summary>
		/// Observes an event on the target entity.  Requires a call to RemoveListener to terminate the observation.
		/// </summary>
		/// <param name="targetEntity"></param>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		void ListenTo<T>(EcsPackedEntityWithWorld targetEntity,
			EntityEventActionRef<T> whenEventFired) where T : struct, IEventEntity;

		/// <summary>
		/// Observes an event on the target entity.  Dispose the returned IDisposable to terminate the observation.
		/// </summary>
		/// <param name="targetEntity"></param>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IDisposable SubscribeTo<T>(EcsPackedEntityWithWorld targetEntity,
			EntityEventActionRef<T> whenEventFired) where T : struct, IEventEntity;
		
		/// <summary>
		/// Terminates the observation of an entity event.  Auto-called by the returned disposable of SubscribeTo, or
		/// manually called for ListenTo calls.
		/// </summary>
		/// <param name="targetEntity"></param>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		void RemoveListener<T>(EcsPackedEntityWithWorld targetEntity,
			EntityEventActionRef<T> whenEventFired) where T : struct, IEventEntity;

		/// <summary>
		/// Fetches a processor for a specific event type, causing it to be processed early.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IEcsRunSystem ProcessorFor<T>() where T : struct, IEventEntity;

		/// <summary>
		/// Processes all remaining events.
		/// </summary>
		/// <returns></returns>
		IEcsRunSystem ProcessorAll();
	}
}