/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */
using System.Collections.Generic;

namespace Leopotam.EcsLite
{
	internal abstract class Pooled<T> where T: Pooled<T>, new()
	{
		private static readonly List<T> _free = new();

		public static T Fetch()
		{
			if (_free.Count == 0)
				return new T();


			var index = _free[^1];
			_free.RemoveAt(_free.Count - 1);

			return index;
		}


		protected static void Free(T pooled)
		{
			_free.Add(pooled);
		}
	}
}