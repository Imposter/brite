/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;

namespace Brite.API
{
    public class BriteException : Exception
    {
        public BriteException()
        {
        }

        public BriteException(string message) 
            : base(message)
        {
        }

        public BriteException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
