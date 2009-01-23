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
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleFx
{
    public sealed partial class ConsoleEx
    {
        #region Properties

        private static char _secretMask = '*';

        public static char SecretMask
        {
            get
            {
                return _secretMask;
            }
            set
            {
                _secretMask = value;
            }
        }

        #endregion

        #region Prompt methods

        public static string Prompt(string prompt, params object[] args)
        {
            ConsoleEx.Write(prompt);
            return Console.ReadLine();
        }

        public static string Prompt(ConsoleColor? foreColor, ConsoleColor? backColor, string prompt,
            params object[] args)
        {
            ConsoleEx.Write(foreColor, backColor, prompt, args);
            return Console.ReadLine();
        }

        #endregion

        #region ReadSecret methods

        public static string ReadSecret(string prompt, params object[] args)
        {
            ConsoleEx.Write(prompt, args);

            StringBuilder result = new StringBuilder();

            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                result.Append(keyInfo.KeyChar);
                Console.Write(_secretMask);
                keyInfo = Console.ReadKey(true);
            }
            Console.WriteLine();

            return result.ToString();
        }

        public static string ReadSecret()
        {
            return ConsoleEx.ReadSecret(string.Empty);
        }

        //TODO: public static SecureString();

        #endregion

        #region WaitForXXXX methods

        public static void WaitForEnter()
        {
            ConsoleEx.WaitForKeys(ConsoleKey.Enter);
        }

        public static ConsoleKeyInfo WaitForAnyKey()
        {
            return Console.ReadKey(true);
        }

        public static char WaitForKeys(params char[] keys)
        {
            char key;
            do
            {
                key = Console.ReadKey(true).KeyChar;
            } while (Array.IndexOf(keys, key) < 0);
            return key;
        }

        public static ConsoleKey WaitForKeys(params ConsoleKey[] keys)
        {
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            } while (Array.IndexOf(keys, key) < 0);
            return key;
        }

        #endregion

        #region Write & WriteLine methods

        public static void Write(string text, params object[] args)
        {
            const string colorMarkupRE = @"<(\w*):(\w*)>";

            string resolvedText = string.Format(text, args);
            MatchCollection matches = Regex.Matches(resolvedText, colorMarkupRE);

            int currentPosition = 0;
            foreach (Match match in matches)
            {
                string textToWrite = resolvedText.Substring(currentPosition, match.Index - currentPosition);
                Console.Write(textToWrite);

                Console.ResetColor();
                string foreColor = match.Groups[1].Value;
                if (foreColor != string.Empty)
                    Console.ForegroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), foreColor, true);
                string backColor = match.Groups[2].Value;
                if (backColor != string.Empty)
                    Console.BackgroundColor = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), backColor, true);

                currentPosition = match.Index + match.Length;
            }

            string lastText = resolvedText.Substring(currentPosition);
            Console.Write(lastText);
        }

        public static void Write(ConsoleColor? foreColor, ConsoleColor? backColor, string text, params object[] args)
        {
            if (foreColor.HasValue)
                Console.ForegroundColor = foreColor.Value;
            if (backColor.HasValue)
                Console.BackgroundColor = backColor.Value;
            Console.Write(text, args);
            Console.ResetColor();
        }

        public static void WriteLine(string text, params object[] args)
        {
            ConsoleEx.Write(text, args);
            Console.WriteLine();
        }

        public static void WriteLine(ConsoleColor? foreColor, ConsoleColor? backColor, string text, params object[] args)
        {
            ConsoleEx.Write(foreColor, backColor, text, args);
            Console.WriteLine();
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }

        #endregion
    }
}
