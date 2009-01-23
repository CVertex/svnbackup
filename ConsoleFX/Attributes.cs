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

namespace ConsoleFx
{
    #region CommandLine attribute class

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandLineAttribute : Attribute
    {
        //Specifies the preferred ordering of the switch and non-switch arguments. By
        //default, this does not matter, but it can be changed to enforce that all
        //switches come before non-switches or vice versa.
        private CommandLineGrouping _grouping = CommandLineGrouping.DoesNotMatter;

        //Default behavior for any exceptions that do not have corresponding error
        //handlers. If this property is true, the usage details are displayed after the
        //error message.
        //For exceptions that do have an error handler, this behavior is decided by the
        //corresponding ErrorHandler attribute's DisplayUsage property.
        private bool _displayUsageOnError = true;

        #region Public properties

        public CommandLineGrouping Grouping
        {
            get
            {
                return _grouping;
            }
            set
            {
                _grouping = value;
            }
        }

        public bool DisplayUsageOnError
        {
            get
            {
                return _displayUsageOnError;
            }
            set
            {
                _displayUsageOnError = value;
            }
        }

        #endregion
    }

    public enum CommandLineGrouping
    {
        DoesNotMatter,
        SwitchesBeforeArguments,
        SwitchesAfterArguments
    }

    #endregion

    #region ParameterUsage attribute class

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ParameterUsageAttribute : Attribute
    {
        private readonly int _mode;
        private int _minOccurences = int.MinValue;
        private int _maxOccurences = int.MaxValue;

        public ParameterUsageAttribute(int mode)
        {
            _mode = mode;
        }

        public int MaxOccurences
        {
            get
            {
                return _maxOccurences;
            }
            set
            {
                _maxOccurences = value;
            }
        }

        public int Mode
        {
            get
            {
                return _mode;
            }
        }

        public int MinOccurences
        {
            get
            {
                return _minOccurences;
            }
            set
            {
                _minOccurences = value;
            }
        }
    }

    #endregion

    #region ParameterProperty attribute class

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class ParameterPropertyAttribute : Attribute
    {
        private readonly int _mode;
        private readonly int _index;

        public ParameterPropertyAttribute(int mode, int index)
        {
            _mode = mode;
            _index = index;
        }

        public int Index
        {
            get
            {
                return _index;
            }
        }

        public int Mode
        {
            get
            {
                return _mode;
            }
        }
    }

    #endregion

    #region Switch attribute class

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class SwitchAttribute : Attribute
    {
        //The name of the switch. This is the only property that is mandatory
        private readonly string _name;

        //An optional short name for the switch. For example, if the name if "copy", then
        //you can have an optional short name of "c".
        private string _shortName;

        //The maximum number of times this switch can be repeated on the command line.
        private int _maxOccurences = 1;

        //The minimum number of times this switch should be repeated on the command line.
        //If the switch is optional, then this property has no effect.
        private int _minOccurences = 1;

        //The maximum number of parameters that this switch can have. A value of -1
        //indicates that there is no limit.
        private int _maxParameters = -1;

        //The minimum number of parameters that this switch should have, if specified. A
        //value of -1 indicates that this property is not to be enforced.
        private int _minParameters = -1;

        //Indicates whether the name and short name of the switch are case sensitive.
        private bool _caseSensitive = false;

        //Indicates the priority of the switch during processing. A lower value indicates
        //that the switch will have higher priority, i.e. it will be processed before
        //switches with a higher value.
        private int _order = int.MaxValue;

        public SwitchAttribute(string name)
        {
            _name = name;
        }

        #region Public properties

        public bool CaseSensitive
        {
            get
            {
                return _caseSensitive;
            }
            set
            {
                _caseSensitive = value;
            }
        }

        public int MaxOccurences
        {
            get
            {
                return _maxOccurences;
            }
            set
            {
                _maxOccurences = value;
            }
        }
	
        public int MaxParameters
        {
            get
            {
                return _maxParameters;
            }
            set
            {
                _maxParameters = value;
            }
        }

        public int MinOccurences
        {
            get
            {
                return _minOccurences;
            }
            set
            {
                _minOccurences = value;
            }
        }
	
        public int MinParameters
        {
            get
            {
                return _minParameters;
            }
            set
            {
                _minParameters = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public int Order
        {
            get
            {
                return _order;
            }
            set
            {
                _order = value;
            }
        }

        public string ShortName
        {
            get
            {
                return _shortName;
            }
            set
            {
                _shortName = value;
            }
        }

        #endregion
    }

    #endregion

    #region SwitchUsage attribute class

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class SwitchUsageAttribute : Attribute
    {
        private readonly int _mode;
        private readonly SwitchUsage _switchUsage;

        public SwitchUsageAttribute(int mode, SwitchUsage switchUsage)
        {
            _mode = mode;
            _switchUsage = switchUsage;
        }

        public int Mode
        {
            get
            {
                return _mode;
            }
        }

        public SwitchUsage SwitchUsage
        {
            get
            {
                return _switchUsage;
            }
        }
    }

    public enum SwitchUsage
    {
        Mandatory,
        Optional,
        NotAllowed
    }

    #endregion

    #region ExecutionPoint attribute class

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class ExecutionPointAttribute : Attribute
    {
        private readonly int _mode = -1;

        public ExecutionPointAttribute(int mode)
        {
            _mode = mode;
        }

        public int Mode
        {
            get
            {
                return _mode;
            }
        }
    }

    #endregion

    #region ErrorHandler attribute class

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class ErrorHandlerAttribute : Attribute
    {
        private readonly Type _exceptionType;
        private bool _displayUsage = false;

        public ErrorHandlerAttribute()
        {
            _exceptionType = typeof(Exception);
        }

        public ErrorHandlerAttribute(Type exceptionType)
        {
            _exceptionType = exceptionType;
        }

        public bool DisplayUsage
        {
            get
            {
                return _displayUsage;
            }
            set
            {
                _displayUsage = value;
            }
        }

        public Type ExceptionType
        {
            get
            {
                return _exceptionType;
            }
        }
    }

    #endregion

    #region Validator attribute class

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class ValidatorAttribute : Attribute
    {
    }

    #endregion

    #region ModeSetter attribute class

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class ModeSetterAttribute : Attribute
    {
    }

    #endregion

    #region Usage attribute class

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public sealed class UsageAttribute : Attribute
    {
    }

    #endregion
}
