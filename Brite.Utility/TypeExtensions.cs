/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;

namespace Brite.Utility
{
    public static class TypeExtensions
    {
        public static string GetFriendlyName(this Type type)
        {
            var name = type.Name;
            if (type.IsConstructedGenericType)
            {
                name = name.Substring(0, name.IndexOf('`')) + "<";
                for (var i = 0; i < type.GenericTypeArguments.Length; i++)
                {
                    var genericType = type.GenericTypeArguments[i];
                    var genericTypeName = genericType.GetFriendlyName();
                    name += i == 0 ? genericTypeName : ", " + genericTypeName;
                }
                name += ">";
            }
            else if (type.IsArray)
            {
                name = type.GetElementType().GetFriendlyName() + "[]";
            }

            return name;
        }
    }
}
