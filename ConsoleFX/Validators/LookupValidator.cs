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

using System.Globalization;

namespace ConsoleFx.Validators
{
    public sealed class LookupValidatorAttribute : BaseValidatorAttribute
    {
        private readonly string _lookups;
        private char _lookupSeparator = ',';
        private bool _ignoreCase = true;

        public LookupValidatorAttribute(int parameterIndex, string lookups)
            : base(parameterIndex)
        {
            _lookups = lookups;
        }

        public override void Validate(string parameterValue)
        {
            string[] lookups = _lookups.Split(_lookupSeparator);
            foreach (string lookup in lookups)
                if (string.Compare(parameterValue, lookup, _ignoreCase, CultureInfo.InvariantCulture) == 0)
                    return;
            throw new CommandLineException(CommandLineException.Codes.ValidationFailed,
                @"The parameter you specified ""{0}"" does not match any of the allowed values: ""{1}""",
                parameterValue, _lookups);
        }

        public bool IgnoreCase
        {
            get
            {
                return _ignoreCase;
            }
            set
            {
                _ignoreCase = value;
            }
        }
	
        public string Lookups
        {
            get
            {
                return _lookups;
            }
        }

        public char LookupSeparator
        {
            get
            {
                return _lookupSeparator;
            }
            set
            {
                _lookupSeparator = value;
            }
        }
    }
}
