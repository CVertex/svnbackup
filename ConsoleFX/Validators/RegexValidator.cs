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

using System.Text.RegularExpressions;

namespace ConsoleFx.Validators
{
    public sealed class RegexValidatorAttribute : BaseValidatorAttribute
    {
        private readonly string _pattern;

        public RegexValidatorAttribute(int parameterIndex, string pattern)
            : base(parameterIndex)
        {
            _pattern = pattern;
        }

        public override void Validate(string parameterValue)
        {
            if (!Regex.IsMatch(parameterValue, _pattern))
                throw new CommandLineException(CommandLineException.Codes.ValidationFailed,
                    @"The parameters you specified ""{0}"" does not match a valid value", parameterValue);
        }

        public string Pattern
        {
            get
            {
                return _pattern;
            }
        }
	
    }
}
