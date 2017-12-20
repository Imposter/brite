/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Collections.Generic;

namespace Brite.UWP.App.Core
{
    public static class NavigationStore
    {
        private static Dictionary<int, object> Data = new Dictionary<int, object>();
        private static Random Random = new Random();

        public static int? Store(object data)
        {
            if (data == null)
                return null;

            lock (Data)
            {
                int num;
                do
                {
                    num = Random.Next(int.MaxValue);
                } while (Data.ContainsKey(num));
                Data.Add(num, data);
                return num;
            }
        }

        public static T Get<T>(int id)
        {
            lock (Data)
            {
                if (Data.ContainsKey(id))
                {
                    var data = (T)Data[id];
                    Data.Remove(id);
                    return data;
                }
                throw new KeyNotFoundException("Invalid id");
            }
        }
    }
}
