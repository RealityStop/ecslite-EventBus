/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */
using System;
using System.Collections.Generic;

namespace Leopotam.EcsLite
{
	internal interface IFlagComponentSubscription
	{
		void Invoke(EcsPackedEntityWithWorld packed);
	}


	internal class FlagComponentSubscription<T> : IFlagComponentSubscription
	{
		private readonly Dictionary<EcsPackedEntityWithWorld, List<Action<EcsPackedEntityWithWorld>>>
			_subscriptionsByEntity = new();


		public void Invoke(EcsPackedEntityWithWorld packed)
		{
			if (_subscriptionsByEntity.TryGetValue(packed, out var actions))
				foreach (var action in actions)
					action.Invoke(packed);
		}


		public void ListenTo(EcsPackedEntityWithWorld source,
			Action<EcsPackedEntityWithWorld> whenEventFired)
		{
			if (!_subscriptionsByEntity.TryGetValue(source, out var actions))
			{
				actions = new List<Action<EcsPackedEntityWithWorld>>();
				_subscriptionsByEntity.Add(source, actions);
			}

			actions.Add(whenEventFired);
		}


		public IDisposable SubscribeTo(EcsPackedEntityWithWorld source,
			Action<EcsPackedEntityWithWorld> whenEventFired)
		{
			ListenTo(source, whenEventFired);
			return UnsubscribeRecord.Fetch().Assign(this, source, whenEventFired);
		}


		public void RemoveListener(EcsPackedEntityWithWorld source, Action<EcsPackedEntityWithWorld> whenEventFired)
		{
			if (_subscriptionsByEntity.TryGetValue(source, out var actions))
				actions.Remove(whenEventFired);
		}


		private class UnsubscribeRecord : Pooled<UnsubscribeRecord>, IDisposable
		{
			private Action<EcsPackedEntityWithWorld> _action;
			private EcsPackedEntityWithWorld _entity;
			private FlagComponentSubscription<T> _sub;


			public void Dispose()
			{
				_sub.RemoveListener(_entity, _action);
				Free(this);
			}


			internal UnsubscribeRecord Assign(FlagComponentSubscription<T> sub, EcsPackedEntityWithWorld entity,
				Action<EcsPackedEntityWithWorld> action)
			{
				_sub = sub;
				_entity = entity;
				_action = action;
				return this;
			}
		}
	}
}