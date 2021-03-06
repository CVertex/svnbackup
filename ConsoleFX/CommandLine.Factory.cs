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

namespace ConsoleFx
{
    public sealed partial class CommandLine
    {
        public static int Run(object program, string[] args)
        {
            return new CommandLine(program, args).Execute(true);
        }

        public static int Run<T>(string[] args)
            where T: new()
        {
            return new CommandLine(typeof(T), args).Execute(true);
        }

        public static int Parse(object program, params string[] args)
        {
            return new CommandLine(program, args).Execute(false);
        }

        public static int Parse<T>(string[] args)
            where T: new()
        {
            return new CommandLine(typeof(T), args).Execute(false);
        }
    }
}
