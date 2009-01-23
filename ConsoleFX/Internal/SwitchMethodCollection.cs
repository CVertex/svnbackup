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
using System.Reflection;
using ConsoleFx.Validators;

namespace ConsoleFx.Internal
{
    #region SwitchMethodCollection class

    //Contains the list of all allowed switches and their corresponding method details.
    //The switch details are stored ascending order of the switch's Order property.
    internal class SwitchMethodCollection : SortedList<SwitchAttribute, SwitchMethodInfo>
    {
        internal SwitchMethodCollection()
            : base(new SwitchAttributesComparer())
        {
        }

        //Searches for a particular switch entry, by the switch long name or short name.
        internal void GetPairByName(string name, out SwitchAttribute switchAttribute,
            out SwitchMethodInfo switchMethodInfo)
        {
            switchAttribute = null;
            switchMethodInfo = null ;
            foreach (KeyValuePair<SwitchAttribute, SwitchMethodInfo> kvp in this)
                if (string.Compare(kvp.Key.Name, name, !kvp.Key.CaseSensitive, CultureInfo.InvariantCulture) == 0 ||
                    (!string.IsNullOrEmpty(kvp.Key.ShortName) &&
                    string.Compare(kvp.Key.ShortName, name, !kvp.Key.CaseSensitive, CultureInfo.InvariantCulture) == 0))
                {
                    switchAttribute = kvp.Key;
                    switchMethodInfo = kvp.Value;
                }
        }

        internal SwitchMethodInfo this[string name]
        {
            get
            {
                foreach (KeyValuePair<SwitchAttribute, SwitchMethodInfo> kvp in this)
                    if (string.Compare(kvp.Key.Name, name, !kvp.Key.CaseSensitive, CultureInfo.InvariantCulture) == 0 ||
                       (!string.IsNullOrEmpty(kvp.Key.ShortName) &&
                       string.Compare(kvp.Key.ShortName, name, !kvp.Key.CaseSensitive, CultureInfo.InvariantCulture) == 0))
                        return kvp.Value;
                return null;
            }
        }

        #region SwitchAttributeComparer internal class

        //Used to sort the switches in ascending order of their Order properties.
        internal sealed class SwitchAttributesComparer : IComparer<SwitchAttribute>
        {
            int IComparer<SwitchAttribute>.Compare(SwitchAttribute x, SwitchAttribute y)
            {
                int result = x.Order - y.Order;
                if (result == 0)
                    result = string.Compare(x.Name, y.Name);
                return result;
            }
        }

        #endregion
    }

    #endregion

    #region SwitchMethodInfo class

    internal sealed class SwitchMethodInfo
    {
        private readonly MethodInfo _methodInfo;
        private readonly List<SwitchUsageAttribute> _modeUsages = new List<SwitchUsageAttribute>();
        private readonly List<BaseValidatorAttribute> _validators = new List<BaseValidatorAttribute>();

        internal SwitchMethodInfo(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
        }

        internal SwitchUsageAttribute FindByModeUsageByMode(int mode)
        {
            return _modeUsages.Find(
                delegate(SwitchUsageAttribute modeUsageAttribute_)
                {
                    return modeUsageAttribute_.Mode == mode;
                });
        }

        internal List<SwitchUsageAttribute> ModeUsages
        {
            get
            {
                return _modeUsages;
            }
        }

        internal MethodInfo MethodInfo
        {
            get
            {
                return _methodInfo;
            }
        }

        internal List<BaseValidatorAttribute> Validators
        {
            get
            {
                return _validators;
            }
        }
    }

    #endregion
}
