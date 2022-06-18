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
	public interface IEventBus_FlagComponents : IEventBus_Subsystem
	{
		/// <summary>
		///     Adds a flag component to the target entity.  This is the same as simply adding the component to the entity the
		///     normal ecslite pool-based way, but is included to unify the API.  Once you are comfortable with the API, you should
		///     probably simply add it yourself, or ensure you pass the cached Pool to minimize performance impact.
		///
		///		Note: A higher performance variant exists.
		/// </summary>
		/// <param name="targetEntity">The target entity that should be flagged</param>
		/// <param name="optionalCachedPool">
		///     The pool to use to flag with.  Optional for convenience, but for frequent operations
		///     this should be provided to minimize performance impact.
		/// </param>
		/// <typeparam name="T">The flag component type.  Must derive from IFlagComponent.</typeparam>
		void Add<T>(EcsPackedEntityWithWorld targetEntity, EcsPool<T> optionalCachedPool = default)
			where T : struct, IFlagComponent;


		/// <summary>
		///     Adds a flag component to the target entity.  This is the same as simply adding the component to the entity the
		///     normal ecslite pool-based way, but is included to unify the API.  Once you are comfortable with the API, you should
		///     probably simply add it yourself, or ensure you pass the cached Pool to minimize performance impact.
		///
		///		Note: This is a higher performance variant.  A more casual variant exists
		/// </summary>
		/// <param name="targetEntity">The target entity that should be flagged</param>
		/// <param name="cachedPool">
		///     The pool to use to flag with.  Required when directly passing the entity.
		/// </param>
		/// <typeparam name="T">The flag component type.  Must derive from IFlagComponent.</typeparam>
		void Add<T>(int targetEntity, EcsPool<T> cachedPool) where T : struct, IFlagComponent;

		/// <summary>
		/// Observe an event on the target entity.  Requirea a call to RemovesListener to terminate the observation.
		/// </summary>
		/// <param name="targetEntity"></param>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		void ListenTo<T>(EcsPackedEntityWithWorld targetEntity,
			Action<EcsPackedEntityWithWorld> whenEventFired) where T : struct, IFlagComponent;
		

		/// <summary>
		/// Observes an event on the target entity.  Dispose the returned IDisposable to terminate the observation.
		/// </summary>
		/// <param name="targetEntity"></param>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IDisposable SubscribeTo<T>(EcsPackedEntityWithWorld targetEntity,
			Action<EcsPackedEntityWithWorld> whenEventFired) where T : struct, IFlagComponent;


		/// <summary>
		/// When using the non-tracking subscription call, you have to manually call Unsubscribe to end a subscription
		/// </summary>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		void RemoveListener<T>(EcsPackedEntityWithWorld targetEntity, Action<EcsPackedEntityWithWorld> whenEventFired) where T : struct, IFlagComponent;
		
		
		IEcsRunSystem ProcessorFor<T>(EcsWorld world) where T : struct, IFlagComponent;

		IEcsRunSystem ProcessorAll();
	}
}