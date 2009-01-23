#region --- License & Copyright Notice ---

/*

ConsoleFx CommandLine Processing Library

Copyright (c) 2006 Jeevan James
All rights reserved.

The contents of this file are made available under the terms of the
Eclipse Public License v1.0 (the "License") which accompanies this
distribution, and is available at the following URL:
http://opensource.org/licenses/eclipse-1.0.txt

Software distributed under the License is distributed on an "AS IS" basis,
WITHOUT WARRANTY OF ANY KIND, either expressed or implied. See the License for
the specific language governing rights and limitations under the License.

By using this software in any fashion, you are agreeing to be bound by the
terms of the License.

*/

#endregion

using System.Collections.Generic;
using System.Reflection;

namespace ConsoleFx.Internal
{
    #region ErrorHandlerMethodCollection class

    //Contains the list of error handlers and their corresponding method details. This
    //collection is sorted in the specific-ness of the exception types, from most specific
    //to least specific.
    internal sealed class ErrorHandlerMethodCollection : SortedList<ErrorHandlerAttribute, MethodInfo>
    {
        internal ErrorHandlerMethodCollection()
            : base(new ErrorHandlerAttributeComparer())
        {
        }

        #region ErrorHandlerAttributeComparer internal class

        //Allows ordering of the exceptions from the more specific exceptions to the less
        //specific exceptions.
        internal sealed class ErrorHandlerAttributeComparer : IComparer<ErrorHandlerAttribute>
        {
            int IComparer<ErrorHandlerAttribute>.Compare(ErrorHandlerAttribute x, ErrorHandlerAttribute y)
            {
                if (x.ExceptionType.IsSubclassOf(y.ExceptionType))
                    return -1;
                else if (y.ExceptionType.IsSubclassOf(x.ExceptionType))
                    return 1;
                else
                    return string.Compare(x.ExceptionType.FullName, y.ExceptionType.FullName);
            }
        }

        #endregion
    }

    #endregion
}
