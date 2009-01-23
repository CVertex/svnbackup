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
using ConsoleFx.Validators;

namespace ConsoleFx.Internal
{
    // mode -> parameter index -> ParameterPropertyInfo
    internal sealed class ParameterPropertyCollection : Dictionary<int, Dictionary<int, ParameterPropertyInfo>>
    {
    }

    internal sealed class ParameterPropertyInfo
    {
        private readonly PropertyInfo _propertyInfo;
        private List<BaseValidatorAttribute> _validators = new List<BaseValidatorAttribute>();

        internal ParameterPropertyInfo(PropertyInfo propertyInfo)
        {
            _propertyInfo = propertyInfo;
        }

        internal PropertyInfo PropertyInfo
        {
            get
            {
                return _propertyInfo;
            }
        }

        internal List<BaseValidatorAttribute> Validators
        {
            get
            {
                return _validators;
            }
        }
    }
}
