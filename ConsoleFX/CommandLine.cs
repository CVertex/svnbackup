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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using ConsoleFx.Internal;
using ConsoleFx.Validators;

namespace ConsoleFx
{
    public sealed partial class CommandLine
    {
        #region Private fields

        private Type _programType;
        private string[] _args;
        private object _program;

        //Metadata fields
        private CommandLineAttribute _commandLine;
        private SwitchMethodCollection _switches = new SwitchMethodCollection();
        private List<BaseValidatorAttribute> _parameterValidators = new List<BaseValidatorAttribute>();
        private List<ParameterUsageAttribute> _parameterUsages = new List<ParameterUsageAttribute>();
        private ParameterPropertyCollection _parameterProperties = new ParameterPropertyCollection();
        private ErrorHandlerMethodCollection _errorHandlers = new ErrorHandlerMethodCollection();
        private ExecutionPointMethodCollection _executionPoints = new ExecutionPointMethodCollection();
        private MethodInfo _validatorMethod;
        private MethodInfo _modeSetterMethod;
        private MethodInfo _usageMethod;

        //Collection of the parameters specified on the command-line.
        private List<string> _specifiedParameters = new List<string>();

        //Collection of switches specified on the command-line.
        private SpecifiedSwitchesCollection _specifiedSwitches = new SpecifiedSwitchesCollection();

        #endregion

        #region Constructors

        public CommandLine(Type programType, params string[] args)
        {
            _programType = programType;
            _args = args;
            _program = Activator.CreateInstance(_programType);
            this.LoadCommandLineMetadata();
        }

        public CommandLine(object program, params string[] args)
        {
            _args = args;
            _program = program;
            _programType = program.GetType();
            this.LoadCommandLineMetadata();
        }

        #endregion

        #region Command line execution code

        private int Execute(bool callExecutor)
        {
            this.LoadMemberMetadata();
            try
            {
                this.ParseCommandLine();
                this.PreValidateAndExecuteSwitches();

                //Get an array of the non-switch parameters.
                string[] parameters = _specifiedParameters.ToArray();

                //Now that the state has been set after processing all the switches, call the
                //SetModeValue method to set the mode of the application. This will be used in the
                //validations that follow.
                int mode = _modeSetterMethod != null ?
                    (int)_modeSetterMethod.Invoke(_program, new object[] { parameters }) :
                    ProgramMode.Normal;

                //If a mode is specified, validate the switches according to the current mode. If a
                //switch does not contain any mode specification for the current mode, do not
                //validate it.
                if (mode >= 0)
                {
                    this.ValidateParameters(parameters, mode);
                    this.ValidateSwitches(mode);
                }

                this.CheckSwitchValidators();
                this.CheckParameterValidators(parameters);
                this.AssignParameterProperties(mode, parameters);
                this.InvokeValidator(parameters);

                //Attempt to run the main execution method. First check if any executors exist for
                //the selected mode, and if so, use them to execute. If not, just use the default
                //executor.
                if (callExecutor)
                {
                    MethodInfo executionMethod = _executionPoints[mode];
                    int programResult = 0;
                    if (executionMethod != null)
                        programResult = (int)executionMethod.Invoke(_program, new object[] { parameters });
                    return programResult;
                }
                else
                    return mode;
            }
            catch (Exception ex)
            {
                Exception actualException = ex is TargetInvocationException && ex.InnerException != null ?
                    ex.InnerException : ex;
                bool exceptionHandled = this.InvokeErrorHandler(actualException);
                if (!exceptionHandled)
                {
                    Console.WriteLine(actualException.Message);
                    if (_commandLine.DisplayUsageOnError && _usageMethod != null)
                    {
                        Console.WriteLine();
                        _usageMethod.Invoke(_program, null);
                    }
                }
                if (actualException is CommandLineException)
                    return ((CommandLineException)actualException).ErrorCode;
                return -1;
            }
        }

        #endregion

        #region Metadata loading

        //Checks the console program class for the CommandLine attribute, and throws an exception if
        //not found. If found, the attribute details are cached for future usage.
        private void LoadCommandLineMetadata()
        {
            if (!_programType.IsDefined(typeof(CommandLineAttribute), false))
                throw new CommandLineException(CommandLineException.Codes.InvalidCommandLineClass,
                    CommandLineException.Messages.InvalidCommandLineClass, _programType.FullName);
            _commandLine = (CommandLineAttribute)_programType.GetCustomAttributes(typeof(CommandLineAttribute), false)[0];

            object[] parameterUsageAttributes = _programType.GetCustomAttributes(typeof(ParameterUsageAttribute), false);
            _parameterUsages.AddRange((ParameterUsageAttribute[])parameterUsageAttributes);

            object[] parameterValidatorAttributes = _programType.GetCustomAttributes(typeof(BaseValidatorAttribute), false);
            _parameterValidators.AddRange((BaseValidatorAttribute[])parameterValidatorAttributes);
        }

        //Retrieve all methods and properties decorated with ConsoleFX attributes and cache them in
        //local fields.
        private void LoadMemberMetadata()
        {
            MethodInfo[] methods = _programType.GetMethods();
            foreach (MethodInfo method in methods)
            {
                if (method.IsDefined(typeof(SwitchAttribute), true))
                {
                    CommandLine.ValidateMethodSignature(method, typeof(void), typeof(string[]));
                    object[] switchAttributes = method.GetCustomAttributes(typeof(SwitchAttribute), true);
                    foreach (SwitchAttribute switchAttribute in switchAttributes)
                    {
                        SwitchMethodInfo switchMethodInfo = new SwitchMethodInfo(method);

                        object[] groupUsageAttributes = method.GetCustomAttributes(typeof(SwitchUsageAttribute), true);
                        switchMethodInfo.ModeUsages.AddRange((SwitchUsageAttribute[])groupUsageAttributes);

                        object[] validatorAttributes = method.GetCustomAttributes(typeof(BaseValidatorAttribute), true);
                        switchMethodInfo.Validators.AddRange((BaseValidatorAttribute[])validatorAttributes);

                        _switches.Add(switchAttribute, switchMethodInfo);
                    }
                    continue;
                }

                object[] errorHandlerAttributes = method.GetCustomAttributes(typeof(ErrorHandlerAttribute), true);
                if (errorHandlerAttributes.Length > 0)
                {
                    foreach (ErrorHandlerAttribute errorHandlerAttribute in errorHandlerAttributes)
                    {
                        CommandLine.ValidateMethodSignature(method, typeof(void), typeof(Exception));
                        _errorHandlers.Add(errorHandlerAttribute, method);
                    }
                    continue;
                }

                object[] executionAttributes = method.GetCustomAttributes(typeof(ExecutionPointAttribute), true);
                if (executionAttributes.Length > 0)
                {
                    foreach (ExecutionPointAttribute executionAttribute in executionAttributes)
                    {
                        CommandLine.ValidateMethodSignature(method, typeof(int), typeof(string[]));
                        _executionPoints.Add(executionAttribute, method);
                    }
                    continue;
                }

                object[] commonValidatorAttributes = method.GetCustomAttributes(typeof(ValidatorAttribute), true);
                if (commonValidatorAttributes.Length > 0)
                {
                    CommandLine.ValidateMethodSignature(method, typeof(bool), typeof(string[]));
                    _validatorMethod = method;
                    continue;
                }

                object[] modeSetterAttribute = method.GetCustomAttributes(typeof(ModeSetterAttribute), true);
                if (modeSetterAttribute.Length > 0)
                {
                    CommandLine.ValidateMethodSignature(method, typeof(int), typeof(string[]));
                    _modeSetterMethod = method;
                    continue;
                }

                object[] usageAttributes = method.GetCustomAttributes(typeof(UsageAttribute), true);
                if (usageAttributes.Length > 0)
                {
                    CommandLine.ValidateMethodSignature(method, typeof(void));
                    _usageMethod = method;
                    continue;
                }
            }

            PropertyInfo[] properties = _programType.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!property.IsDefined(typeof(ParameterPropertyAttribute), true))
                    continue;
                ParameterPropertyAttribute[] parameterPropertyAttributes = ReflectionHelper.GetCustomAttributes<ParameterPropertyAttribute>(property, true);
                foreach (ParameterPropertyAttribute parameterPropertyAttribute in parameterPropertyAttributes)
                {
                    Dictionary<int, ParameterPropertyInfo> subPair;
                    if (!_parameterProperties.TryGetValue(parameterPropertyAttribute.Mode, out subPair))
                    {
                        subPair = new Dictionary<int, ParameterPropertyInfo>();
                        _parameterProperties.Add(parameterPropertyAttribute.Mode, subPair);
                    }
                    if (!subPair.ContainsKey(parameterPropertyAttribute.Index))
                    {
                        ParameterPropertyInfo parameterPropertyInfo = new ParameterPropertyInfo(property);
                        parameterPropertyInfo.Validators.AddRange(ReflectionHelper.GetCustomAttributes<BaseValidatorAttribute>(property, true));
                        subPair.Add(parameterPropertyAttribute.Index, parameterPropertyInfo);
                    }
                }
            }
        }

        private static void ValidateMethodSignature(MethodInfo method, Type returnType, params Type[] parameterTypes)
        {
            if (method.ReturnType != returnType)
                throw new CommandLineException(1000, "Invalid return type for method {0}. Expected type is {1}", method.Name, returnType.FullName);
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != parameterTypes.Length)
                throw new CommandLineException(1001, "Invalid parameters specified for method {0}", method.Name);
            for (int parameterIdx = 0; parameterIdx < parameters.Length; parameterIdx++)
                if (parameters[parameterIdx].ParameterType != parameterTypes[parameterIdx])
                    throw new CommandLineException(1002, "Parameter {0} on method {1} is the wrong type. It should be {2}", parameters[parameterIdx].Name, method.Name, parameterTypes[parameterIdx].FullName);
        }

        #endregion

        #region Command-line parsing

        //Iterate through the command line arguments, parse for switches and cache any switches and
        //parameters in local fields.
        private void ParseCommandLine()
        {
            Regex switchRegex = new Regex(@"^[\-\/]([\w\?]+)");
            Regex paramRegex = new Regex(@"([\s\S\w][^,]*)");

            ArgumentType previousType = ArgumentType.NotSet;
            ArgumentType currentType = ArgumentType.NotSet;

            foreach (string arg in _args)
            {
                this.VerifyCommandLineGrouping(previousType, currentType);

                previousType = currentType;

                Match switchMatch = switchRegex.Match(arg);
                if (!switchMatch.Success)
                {
                    currentType = ArgumentType.Parameter;
                    _specifiedParameters.Add(arg);
                    continue;
                }

                currentType = ArgumentType.Switch;

                string switchName = switchMatch.Groups[1].Value;
                SpecifiedSwitchParameters specifiedSwitchParameters;
                if (!_specifiedSwitches.TryGetValue(switchName, out specifiedSwitchParameters))
                {
                    specifiedSwitchParameters = new SpecifiedSwitchParameters();
                    _specifiedSwitches.Add(switchName, specifiedSwitchParameters);
                }

                List<string> switchParameters = new List<string>();
                specifiedSwitchParameters.Add(switchParameters);

                //If no switch parameters are specified, stop processing
                if (arg.Length == switchName.Length + 1)
                    continue; ;

                if (arg[switchName.Length + 1] != ':')
                    throw new Exception("Invalid switch parameter separator");

                MatchCollection parameterMatches = paramRegex.Matches(arg, switchMatch.Length + 1);
                foreach (Match parameterMatch in parameterMatches)
                {
                    string value = parameterMatch.Groups[1].Value;
                    if (value.StartsWith(","))
                        value = value.Remove(0, 1);
                    switchParameters.Add(value);
                }
            }

            this.VerifyCommandLineGrouping(previousType, currentType);
        }

        //This method is used by the code that validates the command-line grouping. It is
        //called for every iteration of the arguments, and theb 
        private void VerifyCommandLineGrouping(ArgumentType previousType, ArgumentType currentType)
        {
            if (_commandLine.Grouping != CommandLineGrouping.DoesNotMatter &&
                previousType != ArgumentType.NotSet && currentType != ArgumentType.NotSet)
            {
                if (_commandLine.Grouping == CommandLineGrouping.SwitchesAfterArguments &&
                    previousType == ArgumentType.Switch && currentType == ArgumentType.Parameter)
                    throw new CommandLineException(CommandLineException.Codes.SwitchesAfterParameters,
                        CommandLineException.Messages.SwitchesAfterParameters);
                else if (_commandLine.Grouping == CommandLineGrouping.SwitchesBeforeArguments &&
                    previousType == ArgumentType.Parameter && currentType == ArgumentType.Switch)
                    throw new CommandLineException(CommandLineException.Codes.SwitchesBeforeParameters,
                        CommandLineException.Messages.SwitchesBeforeParameters);
            }
        }

        internal enum ArgumentType
        {
            NotSet,
            Switch,
            Parameter
        }

        #endregion

        #region Remaining program flow from the Execute method

        //Iterate through all the switches specified in the command line and
        //execute their switch methods, after performing basic validation.
        private void PreValidateAndExecuteSwitches()
        {
            foreach (KeyValuePair<string, SpecifiedSwitchParameters> kvp in _specifiedSwitches)
            {
                //Check if the switch is a valid one by checking whether a method with
                //the corresponding SwitchAttribute decoration is present in the
                //_switchMethods cache field.9
                SwitchAttribute switchAttribute;
                SwitchMethodInfo switchMethodInfo;
                _switches.GetPairByName(kvp.Key, out switchAttribute, out switchMethodInfo);
                if (switchMethodInfo == null)
                    throw new CommandLineException(CommandLineException.Codes.InvalidSwitchSpecified,
                        CommandLineException.Messages.InvalidSwitchSpecified, kvp.Key);

                foreach (List<string> parameters in kvp.Value)
                {
                    //If the switch is valid, check whether the number of it's parameters
                    //specified is consistent with the MinParameters and MaxParameters value
                    //of its corresponding SwitchAttribute decoration.
                    if ((switchAttribute.MinParameters >= 0 && parameters.Count < switchAttribute.MinParameters) ||
                        (switchAttribute.MaxParameters >= 0 && parameters.Count > switchAttribute.MaxParameters))
                        throw new CommandLineException(CommandLineException.Codes.InvalidSwitchParametersSpecified,
                            CommandLineException.Messages.InvalidSwitchParametersSpecified, kvp.Key);

                    //Attempt to execute the method for the specified switch. The method
                    //can perform some basic validation, and if it fails, it can throw an
                    //exception.
                    switchMethodInfo.MethodInfo.Invoke(_program, new object[] { parameters.ToArray() });
                }
            }
        }

        //Check the number of parameters specified on the command-line, against the min
        //and max specified by the corresponding ParameterUsage attribute.
        private void ValidateParameters(string[] parameters, int mode)
        {
            ParameterUsageAttribute parameterUsage = _parameterUsages.Find(
                delegate(ParameterUsageAttribute parameterUsage_)
                {
                    return parameterUsage_.Mode == mode;
                });
            if (parameterUsage != null)
            {
                if (parameters.Length < parameterUsage.MinOccurences)
                    throw new CommandLineException(CommandLineException.Codes.TooFewParameters,
                        CommandLineException.Messages.TooFewParameters, parameterUsage.MinOccurences);

                if (parameters.Length > parameterUsage.MaxOccurences)
                    throw new CommandLineException(CommandLineException.Codes.TooManyParameters,
                        CommandLineException.Messages.TooManyParameters, parameterUsage.MaxOccurences);
            }
        }

        //Iterate through each switch in the list of allowed switches.
        private void ValidateSwitches(int mode)
        {
            foreach (KeyValuePair<SwitchAttribute, SwitchMethodInfo> kvp in _switches)
            {
                //Get the switch parameter sets for the given switch.
                SpecifiedSwitchParameters specifiedParameters = _specifiedSwitches.FindBySwitchNames(
                    kvp.Key.Name, kvp.Key.ShortName, !kvp.Key.CaseSensitive);

                //If the switch is specified on the command-line, check the number of it's
                //occurences. If they do not match the number specified by it's Switch attribute,
                //throw an exception.
                if (specifiedParameters != null)
                {
                    if (specifiedParameters.Count < kvp.Key.MinOccurences)
                        throw new CommandLineException(CommandLineException.Codes.TooFewSwitches,
                            CommandLineException.Messages.TooFewSwitches, kvp.Key.MinOccurences, kvp.Key.Name);

                    if (specifiedParameters.Count > kvp.Key.MaxOccurences)
                        throw new CommandLineException(CommandLineException.Codes.TooManySwitches,
                            CommandLineException.Messages.TooManySwitches, kvp.Key.MaxOccurences, kvp.Key.Name);
                }

                //Check if a switch usage is specified for the switch in the current mode. If not,
                //simply ignore it and continue with the iteration.
                SwitchUsageAttribute switchUsageAttribute = kvp.Value.FindByModeUsageByMode(mode);
                if (switchUsageAttribute == null)
                    continue;

                //Check if the iterated switch is actually specified in the current command-line by
                //looking in the _specifiedSwitches field. Check this against it's mode usage
                //specification for the current mode, and if there a problem, throw an exception.
                bool switchSpecified = specifiedParameters != null;
                if (switchUsageAttribute.SwitchUsage == SwitchUsage.Mandatory && !switchSpecified)
                    throw new CommandLineException(CommandLineException.Codes.MandatorySwitchAbsent,
                        CommandLineException.Messages.MandatorySwitchAbsent, kvp.Key.Name);
                if (switchUsageAttribute.SwitchUsage == SwitchUsage.NotAllowed && switchSpecified)
                    throw new CommandLineException(CommandLineException.Codes.NonRequiredSwitchPresent,
                        CommandLineException.Messages.NonRequiredSwitchPresent, kvp.Key.Name);
            }
        }

        //If all the switches are valid, validate the switch parameters against any parameter
        //validators decorated on the method.
        private void CheckSwitchValidators()
        {
            foreach (KeyValuePair<string, SpecifiedSwitchParameters> kvp in _specifiedSwitches)
            {
                SwitchMethodInfo switchMethodInfo = _switches[kvp.Key];
                if (switchMethodInfo.Validators.Count == 0)
                    continue;

                foreach (List<string> parameters in kvp.Value)
                    foreach (BaseValidatorAttribute validator in switchMethodInfo.Validators)
                        if (validator.ParameterIndex >= 0)
                        {
                            if (validator.ParameterIndex < parameters.Count)
                                validator.Validate(parameters[validator.ParameterIndex]);
                        }
                        else
                        {
                            foreach (string parameter in parameters)
                                validator.Validate(parameter);
                        }
            }
        }

        //Validate the parameters against any parameter validators that are decorated on the
        //program class.
        private void CheckParameterValidators(string[] parameters)
        {
            foreach (BaseValidatorAttribute parameterValidator in _parameterValidators)
                if (parameterValidator.ParameterIndex >= 0)
                {
                    if (parameterValidator.ParameterIndex < parameters.Length)
                        parameterValidator.Validate(parameters[parameterValidator.ParameterIndex]);
                }
                else
                {
                    foreach (string parameter in parameters)
                        parameterValidator.Validate(parameter);
                }
        }

        private void AssignParameterProperties(int mode, string[] parameters)
        {
            Dictionary<int, ParameterPropertyInfo> subPair;
            if (!_parameterProperties.TryGetValue(mode, out subPair))
                return;

            for (int parameterIdx = 0; parameterIdx < parameters.Length; parameterIdx++)
            {
                ParameterPropertyInfo parameterPropertyInfo;
                if (!subPair.TryGetValue(parameterIdx, out parameterPropertyInfo))
                    continue;

                string parameterValue = parameters[parameterIdx];
                foreach (BaseValidatorAttribute validator in parameterPropertyInfo.Validators)
                    validator.Validate(parameterValue);

                object parameterValueToSet;
                if (parameterPropertyInfo.PropertyInfo.PropertyType == typeof(string))
                    parameterValueToSet = parameterValue;
                else
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(parameterPropertyInfo.PropertyInfo.PropertyType);
                    parameterValueToSet = (converter != null && converter.CanConvertFrom(typeof(string))) ?
                        converter.ConvertFromString(parameterValue) : parameterValue;
                }
                parameterPropertyInfo.PropertyInfo.SetValue(_program, parameterValueToSet, null);
            }
        }

        //Call the custom Validate method to validate the state after the switches have been
        //processed. This can perform any additional validations not supported by the framework.
        private void InvokeValidator(string[] parameters)
        {
            if (_validatorMethod != null)
            {
                bool isValid = (bool)_validatorMethod.Invoke(_program, new object[] { parameters });
                if (!isValid)
                    throw new CommandLineException(CommandLineException.Codes.ValidationFailed,
                        CommandLineException.Messages.ValidationFailed);
            }
        }

        private bool InvokeErrorHandler(Exception exception)
        {
            foreach (KeyValuePair<ErrorHandlerAttribute, MethodInfo> kvp in _errorHandlers)
            {
                if (kvp.Key.ExceptionType.IsInstanceOfType(exception))
                {
                    kvp.Value.Invoke(_program, new object[] { exception });
                    if (kvp.Key.DisplayUsage && _usageMethod != null)
                        _usageMethod.Invoke(_program, null);
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
