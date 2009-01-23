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
using System.Diagnostics;
using System.Threading;

namespace ConsoleFx
{
    #region ConsoleCapture class

    public sealed class ConsoleCapture
    {
        #region Private fields

        private readonly string _filename;
        private readonly string _arguments;

        #endregion

        #region Constructors

        public ConsoleCapture()
        {
        }

        public ConsoleCapture(string filename)
            : this(filename, null)
        {
        }

        public ConsoleCapture(string filename, string arguments)
        {
            _filename = filename;
            _arguments = arguments;
        }

        #endregion

        #region Capture methods

        public ConsoleCaptureResult Start(bool captureError)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = _filename;
                process.StartInfo.Arguments = _arguments;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                if (captureError)
                    process.StartInfo.RedirectStandardError = true;

                if (!process.Start())
                    throw new ConsoleCaptureException(ConsoleCaptureException.Codes.ProcessStartFailed,
                        ConsoleCaptureException.Messages.ProcessStartFailed, _filename);

                string outputMessage, errorMessage = string.Empty;
                int exitCode = captureError ?
                    CaptureOutputAndError(process, out outputMessage, out errorMessage) :
                    CaptureOutput(process, out outputMessage);
                return new ConsoleCaptureResult(exitCode, outputMessage, errorMessage);
            }
        }

        private static int CaptureOutput(Process process, out string outputMessage)
        {
            outputMessage = process.StandardOutput.ReadToEnd();

            if (!process.HasExited)
                process.WaitForExit();

            return process.ExitCode;
        }

        private static int CaptureOutputAndError(Process process, out string outputMessage, out string errorMessage)
        {
            ConsoleReaderDelegate outputReader = new ConsoleReaderDelegate(process.StandardOutput.ReadToEnd);
            ConsoleReaderDelegate errorReader = new ConsoleReaderDelegate(process.StandardError.ReadToEnd);

            IAsyncResult outputResult = outputReader.BeginInvoke(null, null);
            IAsyncResult errorResult = errorReader.BeginInvoke(null, null);

            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                while (!(outputResult.IsCompleted && errorResult.IsCompleted))
                    Thread.Sleep(100);
            }
            else
            {
                WaitHandle[] waitHandles = new WaitHandle[] { outputResult.AsyncWaitHandle, errorResult.AsyncWaitHandle };
                if (!WaitHandle.WaitAll(waitHandles))
                    throw new ConsoleCaptureException(ConsoleCaptureException.Codes.ProcessAborted,
                        ConsoleCaptureException.Messages.ProcessAborted);
            }

            outputMessage = outputReader.EndInvoke(outputResult);
            errorMessage = errorReader.EndInvoke(errorResult);

            if (!process.HasExited)
                process.WaitForExit();

            return process.ExitCode;
        }

        #endregion

        #region Public properties

        public string Arguments
        {
            get
            {
                return _arguments;
            }
        }

        public string Filename
        {
            get
            {
                return _filename;
            }
        }

        #endregion

        private delegate string ConsoleReaderDelegate();

        #region Public static helpers

        public static ConsoleCaptureResult Start(string filename, string arguments, bool captureError)
        {
            return new ConsoleCapture(filename, arguments).Start(captureError);
        }

        #endregion
    }

    #endregion

    #region ConsoleCaptureResult class

    [DebuggerDisplay("{ToString()}")]
    [Serializable]
    public sealed class ConsoleCaptureResult
    {
        #region Private fields

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly int _exitCode;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _outputMessage;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string _errorMessage;

        #endregion

        public ConsoleCaptureResult(int exitCode, string outputMessage, string errorMessage)
        {
            _exitCode = exitCode;
            _outputMessage = outputMessage;
            _errorMessage = errorMessage;
        }

        #region Public properties

        public string ErrorMessage
        {
            [DebuggerStepThrough]
            get
            {
                return _errorMessage;
            }
        }

        public int ExitCode
        {
            [DebuggerStepThrough]
            get
            {
                return _exitCode;
            }
        }

        public string OutputMessage
        {
            [DebuggerStepThrough]
            get
            {
                return _outputMessage;
            }
        }

        #endregion
    }

    #endregion
}
