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
using System.Runtime.Serialization;

namespace ConsoleFx
{
    #region ConsoleFxException exception class

    [Serializable]
    public abstract class ConsoleFxException : Exception
    {
        private readonly int _errorCode;

        public ConsoleFxException(int errorCode)
        {
            _errorCode = errorCode;
        }

        public ConsoleFxException(int errorCode, string message, params object[] args)
            : base(string.Format(message, args))
        {
            _errorCode = errorCode;
        }

        public ConsoleFxException(int errorCode, Exception innerException, string message, params object[] args)
            : base(string.Format(message, args), innerException)
        {
            _errorCode = errorCode;
        }

        public int ErrorCode
        {
            get
            {
                return _errorCode;
            }
        }

        protected ConsoleFxException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    #endregion

    #region CommandLineException exception class

    [Serializable]
    public sealed class CommandLineException : ConsoleFxException
    {
        public CommandLineException(int errorCode)
            : base(errorCode)
        {
        }

        public CommandLineException(int errorCode, string message, params object[] args)
            : base(errorCode, message, args)
        {
        }

        public CommandLineException(int errorCode, Exception innerException,
            string message, params object[] args)
            : base(errorCode, innerException, message, args)
        {
        }

        internal CommandLineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #region Codes constants inner class

        public static class Codes
        {
            public const int InvalidCommandLineClass = ErrorCodeBase - 1;
            public const int InvalidSwitchSpecified = ErrorCodeBase - 2;
            public const int InvalidSwitchParametersSpecified = ErrorCodeBase - 3;
            public const int ValidationFailed = ErrorCodeBase - 4;
            public const int ExecutionMethodAbsent = ErrorCodeBase - 5;
            public const int MandatorySwitchAbsent = ErrorCodeBase - 6;
            public const int NonRequiredSwitchPresent = ErrorCodeBase - 7;
            public const int SwitchesBeforeParameters = ErrorCodeBase - 8;
            public const int SwitchesAfterParameters = ErrorCodeBase - 9;
            public const int TooFewSwitches = ErrorCodeBase - 10;
            public const int TooManySwitches = ErrorCodeBase - 11;
            public const int TooFewParameters = ErrorCodeBase - 12;
            public const int TooManyParameters = ErrorCodeBase - 13;

            private const int ErrorCodeBase = 0;
        }

        #endregion

        #region Messages constants inner class

        public static class Messages
        {
            public const string InvalidCommandLineClass = @"The class ""{0}"" should be decorated with a [CommandLine] attribute";
            public const string InvalidSwitchSpecified = @"An invalid option was specified: ""{0}""";
            public const string InvalidSwitchParametersSpecified = @"The parameters specified for the option ""{0}"" are invalid";
            public const string ValidationFailed = @"The command failed due to an internal validation error";
            public const string ExecutionMethodAbsent = @"There are no valid execution points in the application. Please specify one.";
            public const string MandatorySwitchAbsent = @"The option ""{0}"" should be specified in the current usage of this command";
            public const string NonRequiredSwitchPresent = @"The option ""{0}"" cannot be used in this context";
            public const string SwitchesBeforeParameters = @"The options should be specified before the parameters";
            public const string SwitchesAfterParameters = @"The options should be specified after the parameters";
            public const string TooFewSwitches = @"You have to specify at least {0} ""{1}"" options";
            public const string TooManySwitches = @"You cannot specify more than {0} ""{1}"" options";
            public const string TooFewParameters = @"You have to specify at least {0} parameters";
            public const string TooManyParameters = @"You cannot specify more than {0} parameters";
        }

        #endregion
    }

    #endregion

    #region ConsoleCaptureException exception class

    public sealed class ConsoleCaptureException : ConsoleFxException
    {
        public ConsoleCaptureException(int errorCode)
            : base(errorCode)
        {
        }

        public ConsoleCaptureException(int errorCode, string message, params object[] args)
            : base(errorCode, message, args)
        {
        }

        public ConsoleCaptureException(int errorCode, Exception innerException,
            string message, params object[] args)
            : base(errorCode, innerException, message, args)
        {
        }

        internal ConsoleCaptureException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #region Codes constants inner class

        public static class Codes
        {
            public const int ProcessStartFailed = ErrorCodeBase + 1;
            public const int ProcessAborted = ErrorCodeBase + 2;

            private const int ErrorCodeBase = 100;
        }

        #endregion

        #region Messages constants inner class

        public static class Messages
        {
            public const string ProcessStartFailed = "Could not start the process with filename '{0}'";
            public const string ProcessAborted = "The process was aborted";
        }

        #endregion
    }

    #endregion

    #region ConsoleProgramException exception class

    public sealed class ConsoleProgramException : ConsoleFxException
    {
        public ConsoleProgramException(int errorCode)
            : base(errorCode)
        {
        }

        public ConsoleProgramException(int errorCode, string message, params object[] args)
            : base(errorCode, message, args)
        {
        }

        public ConsoleProgramException(int errorCode, Exception innerException,
            string message, params object[] args)
            : base(errorCode, innerException, message, args)
        {
        }

        internal ConsoleProgramException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    #endregion
}
