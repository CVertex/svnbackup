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
    public sealed class IntegerValidatorAttribute : BaseValidatorAttribute
    {
        private int _minimumValue = int.MinValue;
        private int _maximumValue = int.MaxValue;

        public IntegerValidatorAttribute(int parameterIndex)
            : base(parameterIndex)
        {
        }

        public override void Validate(string parameterValue)
        {
            int value;
            if (!int.TryParse(parameterValue, out value))
                throw new CommandLineException(CommandLineException.Codes.ValidationFailed,
                    "{0} is not a valid integer", parameterValue);
            if (value < _minimumValue || value > _maximumValue)
                throw new CommandLineException(CommandLineException.Codes.ValidationFailed,
                    "{0} does not fall into the valid range of {1} to {2}", value, _minimumValue, _maximumValue);
        }

        public int MaximumValue
        {
            get
            {
                return _maximumValue;
            }
            set
            {
                _maximumValue = value;
            }
        }

        public int MinimumValue
        {
            get
            {
                return _minimumValue;
            }
            set
            {
                _minimumValue = value;
            }
        }
	
    }
}
