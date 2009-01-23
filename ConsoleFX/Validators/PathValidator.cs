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
using System.IO;

namespace ConsoleFx.Validators
{
    public sealed class PathValidatorAttribute : BaseValidatorAttribute
    {
        private PathType _pathType = PathType.File;
        private bool _checkIfExists = false;

        public PathValidatorAttribute(int parameterIndex)
            : base(parameterIndex)
        {
        }

        public override void Validate(string parameterValue)
        {
            if (_checkIfExists)
            {
                Predicate<string> checker = (_pathType == PathType.File) ?
                    new Predicate<string>(File.Exists) : new Predicate<string>(Directory.Exists);
                if (!checker(parameterValue))
                    throw new CommandLineException(CommandLineException.Codes.ValidationFailed,
                        "The path '{0}' does not exist", parameterValue);
            }
        }

        #region Public properties

        public bool CheckIfExists
        {
            get
            {
                return _checkIfExists;
            }
            set
            {
                _checkIfExists = value;
            }
        }

        public PathType PathType
        {
            get
            {
                return _pathType;
            }
            set
            {
                _pathType = value;
            }
        }

        #endregion
    }

    public enum PathType
    {
        File,
        Folder
    }
}
