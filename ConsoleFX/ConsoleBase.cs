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
    //Base class a typical console application.
    public abstract class ConsoleProgram
    {
        private bool _showHelp = false;

        [ExecutionPoint(ProgramMode.Normal)]
        public abstract int ExecuteNormal(string[] parameters);

        [ExecutionPoint(ProgramMode.Help)]
        public virtual int ExecuteHelp(string[] parameters)
        {
            this.DisplayUsage();
            return 0;
        }

        [Validator]
        public virtual bool Validate(string[] parameters)
        {
            return true;
        }

        [ModeSetter]
        public virtual int SetMode(string[] parameters)
        {
            return _showHelp ? ProgramMode.Help : ProgramMode.Normal;
        }

        [ErrorHandler(typeof(Exception), DisplayUsage = true)]
        public virtual void DefaultErrorHandler(Exception exception)
        {
            ConsoleEx.WriteLine(exception.Message);
        }

        [Usage]
        public virtual void DisplayUsage()
        {
        }

        protected bool ShowHelp
        {
            get
            {
                return _showHelp;
            }
            set
            {
                _showHelp = value;
            }
        }
	
    }
}
