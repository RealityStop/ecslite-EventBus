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
	public interface IEventBus_Uniques : IEventBus_Subsystem
	{
		delegate void UniqueEventActionRef<T>(ref T item);

		ref T Add<T>() where T : struct, IEventUnique;
		bool Has<T>() where T : struct, IEventUnique;
		void Del<T>() where T : struct, IEventUnique;


		/// <summary>
		/// Subscribes to a unique event without tracking, which means it is up to you to handle unsubscribing
		/// appropriately (don't use lambdas)
		/// </summary>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		void ListenTo<T>(UniqueEventActionRef<T> whenEventFired) where T : struct, IEventUnique;


		/// <summary>
		/// Subscribes to a unique event with tracking, which means simply dispose the return value to
		/// end the subscription.  A small amount of memory is allocated to contain the tracking information (2 ints).
		/// </summary>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		IDisposable SubscribeTo<T>(UniqueEventActionRef<T> whenEventFired)
			where T : struct, IEventUnique;


		/// <summary>
		/// When using the non-tracking subscription call, you have to manually call Unsubscribe to end a subscription
		/// </summary>
		/// <param name="whenEventFired"></param>
		/// <typeparam name="T"></typeparam>
		void RemoveListener<T>(UniqueEventActionRef<T> whenEventFired) where T : struct, IEventUnique;


		IEcsRunSystem ProcessorFor<T>() where T : struct, IEventUnique;

		IEcsRunSystem ProcessorAll();
	}
}