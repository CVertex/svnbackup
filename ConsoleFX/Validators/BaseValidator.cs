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

using System;

namespace ConsoleFx.Validators
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
    public abstract class BaseValidatorAttribute : Attribute
    {
        private int _parameterIndex = 0;

        public BaseValidatorAttribute(int parameterIndex)
        {
            _parameterIndex = parameterIndex;
        }

        public abstract void Validate(string parameterValue);

        public int ParameterIndex
        {
            get
            {
                return _parameterIndex;
            }
            protected set
            {
                _parameterIndex = value;
            }
        }
    }

    //Few constants for the parameter index that needs to be specified for the validator
    //attributes. A special value of -1 is meant to be applied on all parameters.
    public static class ParameterIndex
    {
        public const int AllParameters = -1;
        public const int FirstParameter = 0;
        public const int SecondParameter = 1;
        public const int ThirdParameter = 2;
        public const int FourthParameter = 3;
    }
}
