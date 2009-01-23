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

using System.Collections.Generic;
using System.Globalization;

namespace ConsoleFx.Internal
{
    #region SpecifiedSwitchesCollection class

    //Contains the list of switches actually specified on the command line.
    internal sealed class SpecifiedSwitchesCollection : Dictionary<string, SpecifiedSwitchParameters>
    {
        public SpecifiedSwitchParameters FindBySwitchNames(string name, string shortName, bool ignoreCase)
        {
            foreach (KeyValuePair<string, SpecifiedSwitchParameters> kvp in this)
                if (string.Compare(name, kvp.Key, ignoreCase, CultureInfo.InvariantCulture) == 0 ||
                    (!string.IsNullOrEmpty(shortName) &&
                    string.Compare(shortName, kvp.Key, ignoreCase, CultureInfo.InvariantCulture) == 0))
                    return kvp.Value;
            return null;
        }
    }

    #endregion

    #region SpecifiedSwitchParameters class

    //Contains the list of parameters specified for a particular switch. Since it is
    //allowed to specify the same switch multiple times on the same command line, this
    //structure is actually a collection of parameter collections. The total of all the
    //inner collection values are the values specified by all switches.
    internal sealed class SpecifiedSwitchParameters : List<List<string>>
    {
    }

    #endregion
}
