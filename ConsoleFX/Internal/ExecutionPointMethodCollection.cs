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
    #region ExecutionPointMethodCollection class

    //Contains the list of execution points and their corresponding method details. This
    //collection can be indexed by the program mode value.
    internal sealed class ExecutionPointMethodCollection : Dictionary<ExecutionPointAttribute, MethodInfo>
    {
        internal MethodInfo this[int mode]
        {
            get
            {
                foreach (KeyValuePair<ExecutionPointAttribute, MethodInfo> kvp in this)
                    if (kvp.Key.Mode == mode)
                        return kvp.Value;
                return null;
            }
        }
    }

    #endregion
}
