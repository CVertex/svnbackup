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

namespace ConsoleFx.Validators
{
    public sealed class StringValidatorAttribute : BaseValidatorAttribute
    {
        private int _minLength;
        private int _maxLength = int.MaxValue;

        public StringValidatorAttribute(int parameterIndex)
            : base(parameterIndex)
        {
        }

        public override void Validate(string parameterValue)
        {
            if (parameterValue.Length < _minLength)
                throw new CommandLineException(CommandLineException.Codes.ValidationFailed,
                    @"The parameter you specified ""{0}"" should be at least {1} characters long", parameterValue, _minLength);
            if (parameterValue.Length > _maxLength)
                throw new CommandLineException(CommandLineException.Codes.ValidationFailed,
                    @"The parameter you specified ""{0}"" should not be more than {1} characters long", parameterValue, _maxLength);
        }

        public int MaxLength
        {
            get
            {
                return _maxLength;
            }
            set
            {
                _maxLength = value;
            }
        }

        public int MinLength
        {
            get
            {
                return _minLength;
            }
            set
            {
                _minLength = value;
            }
        }
	
    }
}
