//
// NConsoler 0.9.3
// http://nconsoler.csharpus.com
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NConsoler
{
	/// <summary>
	/// Entry point for NConsoler applications
	/// </summary>
	public sealed class Consolery
	{
		#region Readonly & Static Fields

		private readonly List<MethodInfo> m_ActionMethods = new List<MethodInfo>();
		private readonly string[] m_Args;
		private readonly IMessenger m_Messenger;
		private readonly Type m_TargetType;

		#endregion

		#region Constructors

		private Consolery(Type targetType, string[] args, IMessenger messenger)
		{

            this.m_TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            this.m_Args = args ?? throw new ArgumentNullException(nameof(args));
            this.m_Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
			var methods = this.m_TargetType.GetMethods(BindingFlags.Public | BindingFlags.Static);
			foreach (var method in methods)
			{
				var attributes = method.GetCustomAttributes(false);
				if (attributes.OfType<ActionAttribute>().Any())
				{
                    this.m_ActionMethods.Add(method);
				}
			}
		}

		#endregion

		#region Instance Properties

		private bool IsMulticommand
		{
			get { return this.m_ActionMethods.Count > 1; }
		}

		#endregion

		#region Instance Methods

		private object[] BuildParameterArray(MethodInfo method)
		{
			var argumentIndex = this.IsMulticommand ? 1 : 0;
			var parameterValues = new List<object>();
			var aliases = new Dictionary<string, ParameterData>();
			foreach (var info in method.GetParameters())
			{
				if (IsRequired(info))
				{
					parameterValues.Add(ConvertValue(this.m_Args[argumentIndex], info.ParameterType));
				}
				else
				{
					var optional = GetOptional(info);

					foreach (var altName in optional.AltNames)
					{
						aliases.Add(altName.ToLower(),
							new ParameterData(parameterValues.Count, info.ParameterType));
					}
					aliases.Add(info.Name.ToLower(),
						new ParameterData(parameterValues.Count, info.ParameterType));
					parameterValues.Add(optional.Default);
				}
				argumentIndex++;
			}
			foreach (var optionalParameter in this.OptionalParameters(method))
			{
				var name = ParameterName(optionalParameter);
				var value = ParameterValue(optionalParameter);
				parameterValues[aliases[name].m_Position] = ConvertValue(value, aliases[name].m_Type);
			}
			return parameterValues.ToArray();
		}

		private void CheckActionMethodNamesAreNotReserved()
		{
			foreach (var method in this.m_ActionMethods)
			{
				if (method.Name.ToLower() == "help")
				{
					throw new NConsolerException("Method name \"{0}\" is reserved. Please, choose another name", method.Name);
				}
			}
		}

		private void CheckAllRequiredParametersAreSet(MethodInfo method)
		{
			var minimumArgsLengh = RequiredParameterCount(method);
			if (this.IsMulticommand)
			{
				minimumArgsLengh++;
			}
			if (this.m_Args.Length < minimumArgsLengh)
			{
				throw new NConsolerException("Not all required parameters are set");
			}
		}

		private void CheckAnyActionMethodExists()
		{
			if (this.m_ActionMethods.Count == 0)
			{
				throw new NConsolerException(
					"Can not find any public static method marked with [Action] attribute in type \"{0}\"", this.m_TargetType.Name);
			}
		}

		private void CheckOptionalParametersAreNotDuplicated(MethodInfo method)
		{
			var passedParameters = new List<string>();
			foreach (var optionalParameter in this.OptionalParameters(method))
			{
				if (!optionalParameter.StartsWith("/"))
				{
					throw new NConsolerException("Unknown parameter {0}", optionalParameter);
				}
				var name = ParameterName(optionalParameter);
				if (passedParameters.Contains(name))
				{
					throw new NConsolerException("Parameter with name {0} passed two times", name);
				}
				passedParameters.Add(name);
			}
		}

		private void CheckUnknownParametersAreNotPassed(MethodInfo method)
		{
			var parameterNames = new List<string>();
			foreach (var parameter in method.GetParameters())
			{
				if (IsRequired(parameter))
				{
					continue;
				}
				parameterNames.Add(parameter.Name.ToLower());
				var optional = GetOptional(parameter);
				parameterNames.AddRange(optional.AltNames.Select(altName => altName.ToLower()));
			}
			foreach (var optionalParameter in this.OptionalParameters(method))
			{
				var name = ParameterName(optionalParameter);
				if (!parameterNames.Contains(name.ToLower()))
				{
					throw new NConsolerException("Unknown parameter name {0}", optionalParameter);
				}
			}
		}

		private MethodInfo GetCurrentMethod()
		{
			if (!this.IsMulticommand)
			{
				return this.m_ActionMethods[0];
			}
			return this.GetMethodByName(this.m_Args[0].ToLower());
		}

		private MethodInfo GetMethodByName(string name)
		{
			return this.m_ActionMethods.FirstOrDefault(method => method.Name.ToLower() == name);
		}

		private void IfActionMethodIsSingleCheckMethodHasParameters()
		{
			if (this.m_ActionMethods.Count == 1 && this.m_ActionMethods[0].GetParameters().Length == 0)
			{
				throw new NConsolerException(
					"[Action] attribute applied once to the method \"{0}\" without parameters. In this case NConsoler should not be used",
                    this.m_ActionMethods[0].Name);
			}
		}

		private void InvokeMethod(MethodInfo method)
		{
			try
			{
				method.Invoke(null, this.BuildParameterArray(method));
			}
			catch (TargetInvocationException e)
			{
				if (e.InnerException != null)
				{
					throw new NConsolerException(e.InnerException.Message, e);
				}
				throw;
			}
		}

		private bool IsHelpRequested()
		{
			return (this.m_Args.Length == 0 && !this.SingleActionWithOnlyOptionalParametersSpecified())
				|| (this.m_Args.Length > 0 && (this.m_Args[0] == "/?"
				|| this.m_Args[0] == "/help"
				|| this.m_Args[0] == "/h"
				|| this.m_Args[0] == "help"));
		}

		private bool IsSubcommandHelpRequested()
		{
			return this.m_Args.Length > 0
				&& this.m_Args[0].ToLower() == "help"
					&& this.m_Args.Length == 2;
		}

		private IEnumerable<string> OptionalParameters(MethodInfo method)
		{
			var firstOptionalParameterIndex = RequiredParameterCount(method);
			if (this.IsMulticommand)
			{
				firstOptionalParameterIndex++;
			}
			for (var i = firstOptionalParameterIndex; i < this.m_Args.Length; i++)
			{
				yield return this.m_Args[i];
			}
		}

		private void PrintGeneralMulticommandUsage()
		{
            this.m_Messenger.Write(
				String.Format("usage: {0} <subcommand> [args]", this.ProgramName()));
            this.m_Messenger.Write(
				String.Format("Type '{0} help <subcommand>' for help on a specific subcommand.", this.ProgramName()));
            this.m_Messenger.Write(String.Empty);
            this.m_Messenger.Write("Available subcommands:");
			foreach (var method in this.m_ActionMethods)
			{
                this.m_Messenger.Write(method.Name.ToLower() + " " + GetMethodDescription(method));
			}
		}

		private void PrintMethodDescription(MethodInfo method)
		{
			var description = GetMethodDescription(method);
			if (description == String.Empty) return;
            this.m_Messenger.Write(description);
		}

		private void PrintParametersDescriptions(IEnumerable<KeyValuePair<string, string>> parameters)
		{
			var maxParameterNameLength = MaxKeyLength(parameters);
			foreach (var pair in parameters)
			{
				if (pair.Value != String.Empty)
				{
					var difference = maxParameterNameLength - pair.Key.Length + 2;
                    this.m_Messenger.Write("    " + pair.Key + new String(' ', difference) + pair.Value);
				}
			}
		}

		private void PrintSubcommandUsage()
		{
			var method = this.GetMethodByName(this.m_Args[1].ToLower());
			if (method == null)
			{
                this.PrintGeneralMulticommandUsage();
				throw new NConsolerException("Unknown subcommand \"{0}\"", this.m_Args[0].ToLower());
			}
            this.PrintUsage(method);
		}

		private void PrintUsage(MethodInfo method)
		{
            this.PrintMethodDescription(method);
			var parameters = GetParametersDescriptions(method);
            this.PrintUsageExample(method, parameters);
            this.PrintParametersDescriptions(parameters);
		}

		private void PrintUsage()
		{
			if (this.IsMulticommand && !this.IsSubcommandHelpRequested())
			{
                this.PrintGeneralMulticommandUsage();
			}
			else if (this.IsMulticommand && this.IsSubcommandHelpRequested())
			{
                this.PrintSubcommandUsage();
			}
			else
			{
                this.PrintUsage(this.m_ActionMethods[0]);
			}
		}

		private void PrintUsageExample(MethodInfo method, IDictionary<string, string> parameterList)
		{
			var subcommand = this.IsMulticommand ? method.Name.ToLower() + " " : String.Empty;
			var parameters = String.Join(" ", new List<string>(parameterList.Keys).ToArray());
            this.m_Messenger.Write("usage: " + this.ProgramName() + " " + subcommand + parameters);
		}

		private string ProgramName()
		{
			var entryAssembly = Assembly.GetEntryAssembly();
			if (entryAssembly == null)
			{
				return this.m_TargetType.Name.ToLower();
			}
			return new AssemblyName(entryAssembly.FullName).Name;
		}

		private void RunAction()
		{
            this.ValidateMetadata();
			if (this.IsHelpRequested())
			{
                this.PrintUsage();
				return;
			}

			var currentMethod = this.GetCurrentMethod();
			if (currentMethod == null)
			{
                this.PrintUsage();
				throw new NConsolerException("Unknown subcommand \"{0}\"", this.m_Args[0]);
			}

            this.ValidateInput(currentMethod);
            this.InvokeMethod(currentMethod);
		}

		private bool SingleActionWithOnlyOptionalParametersSpecified()
		{
			if (this.IsMulticommand)
			{
				return false;
			}

			var method = this.m_ActionMethods[0];
			return OnlyOptionalParametersSpecified(method);
		}

		private void ValidateInput(MethodInfo method)
		{
            this.CheckAllRequiredParametersAreSet(method);
            this.CheckOptionalParametersAreNotDuplicated(method);
            this.CheckUnknownParametersAreNotPassed(method);
		}

		private void ValidateMetadata()
		{
            this.CheckAnyActionMethodExists();
            this.IfActionMethodIsSingleCheckMethodHasParameters();
			foreach (var method in this.m_ActionMethods)
			{
                this.CheckActionMethodNamesAreNotReserved();
				CheckRequiredAndOptionalAreNotAppliedAtTheSameTime(method);
				CheckOptionalParametersAreAfterRequiredOnes(method);
				CheckOptionalParametersDefaultValuesAreAssignableToRealParameterTypes(method);
				CheckOptionalParametersAltNamesAreNotDuplicated(method);
			}
		}

		#endregion

		#region Class Methods

		/// <summary>
		/// Runs an appropriate Action method.
		/// Uses the class this call lives in as target type and command line arguments from Environment
		/// </summary>
		public static void Run()
		{
			var declaringType = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
			var args = new string[Environment.GetCommandLineArgs().Length - 1];
			new List<string>(Environment.GetCommandLineArgs()).CopyTo(1, args, 0, Environment.GetCommandLineArgs().Length - 1);
			Run(declaringType, args);
		}

		/// <summary>
		/// Runs an appropriate Action method
		/// </summary>
		/// <param name="targetType">Type where to search for Action methods</param>
		/// <param name="args">Arguments that will be converted to Action method arguments</param>
		public static void Run(Type targetType, string[] args)
		{
			Run(targetType, args, new ConsoleMessenger());
		}

		/// <summary>
		/// Runs an appropriate Action method
		/// </summary>
		/// <param name="targetType">Type where to search for Action methods</param>
		/// <param name="args">Arguments that will be converted to Action method arguments</param>
		/// <param name="messenger">Uses for writing messages instead of Console class methods</param>
		public static void Run(Type targetType, string[] args, IMessenger messenger)
		{
			try
			{
				new Consolery(targetType, args, messenger).RunAction();
			}
			catch (NConsolerException e)
			{
				messenger.Write(e.Message);
			}
		}

		/// <summary>
		/// Validates specified type and throws NConsolerException if an error
		/// </summary>
		/// <param name="targetType">Type where to search for Action methods</param>
		public static void Validate(Type targetType)
		{
			new Consolery(targetType, new string[] {}, new ConsoleMessenger()).ValidateMetadata();
		}

		private static bool CanBeConvertedToDate(string parameter)
		{
			try
			{
				ConvertToDateTime(parameter);
				return true;
			}
			catch (NConsolerException)
			{
				return false;
			}
		}

		private static bool CanBeNull(Type type)
		{
			return type == typeof (string)
				|| type == typeof (string[])
				|| type == typeof (int[]);
		}

		private static void CheckOptionalParametersAltNamesAreNotDuplicated(MethodBase method)
		{
			var parameterNames = new List<string>();
			foreach (var parameter in method.GetParameters())
			{
				if (IsRequired(parameter))
				{
					parameterNames.Add(parameter.Name.ToLower());
				}
				else
				{
					if (parameterNames.Contains(parameter.Name.ToLower()))
					{
						throw new NConsolerException(
							"Found duplicated parameter name \"{0}\" in method \"{1}\". Please check alt names for optional parameters",
							parameter.Name, method.Name);
					}

					parameterNames.Add(parameter.Name.ToLower());
					var optional = GetOptional(parameter);
					foreach (var altName in optional.AltNames)
					{
						if (parameterNames.Contains(altName.ToLower()))
						{
							throw new NConsolerException(
								"Found duplicated parameter name \"{0}\" in method \"{1}\". Please check alt names for optional parameters",
								altName, method.Name);
						}

						parameterNames.Add(altName.ToLower());
					}
				}
			}
		}

		private static void CheckOptionalParametersAreAfterRequiredOnes(MethodBase method)
		{
			var optionalFound = false;
			foreach (var parameter in method.GetParameters())
			{
				if (IsOptional(parameter))
				{
					optionalFound = true;
				}
				else if (optionalFound)
				{
					throw new NConsolerException(
						"It is not allowed to write a parameter with a Required attribute after a parameter with an Optional one. See method \"{0}\" parameter \"{1}\"",
						method.Name, parameter.Name);
				}
			}
		}

		private static void CheckOptionalParametersDefaultValuesAreAssignableToRealParameterTypes(MethodBase method)
		{
			foreach (var parameter in method.GetParameters())
			{
				if (IsRequired(parameter))
				{
					continue;
				}
				
				var optional = GetOptional(parameter);
				if (optional.Default != null && optional.Default.GetType() == typeof (string) &&
					CanBeConvertedToDate(optional.Default.ToString()))
				{
					return;
				}
				
				if ((optional.Default == null && !CanBeNull(parameter.ParameterType))
					|| (optional.Default != null && !optional.Default.GetType().IsAssignableFrom(parameter.ParameterType)))
				{
					throw new NConsolerException(
						"Default value for an optional parameter \"{0}\" in method \"{1}\" can not be assigned to the parameter",
						parameter.Name, method.Name);
				}
			}
		}

		private static void CheckRequiredAndOptionalAreNotAppliedAtTheSameTime(MethodBase method)
		{
			foreach (var parameter in method.GetParameters())
			{
				var attributes = parameter.GetCustomAttributes(typeof (ParameterAttribute), false);
				if (attributes.Length > 1)
				{
					throw new NConsolerException("More than one attribute is applied to the parameter \"{0}\" in the method \"{1}\"",
						parameter.Name, method.Name);
				}
			}
		}

		private static DateTime ConvertToDateTime(string parameter)
		{
			var parts = parameter.Split('-');
			if (parts.Length != 3)
			{
				throw new NConsolerException("Could not convert {0} to Date", parameter);
			}
			
			var day = (int) ConvertValue(parts[0], typeof (int));
			var month = (int) ConvertValue(parts[1], typeof (int));
			var year = (int) ConvertValue(parts[2], typeof (int));
			try
			{
				return new DateTime(year, month, day);
			}
			catch (ArgumentException)
			{
				throw new NConsolerException("Could not convert {0} to Date", parameter);
			}
		}

		private static object ConvertValue(string value, Type argumentType)
		{
			if (argumentType == typeof (int))
			{
				try
				{
					return Convert.ToInt32(value);
				}
				catch (FormatException)
				{
					throw new NConsolerException("Could not convert \"{0}\" to integer", value);
				}
				catch (OverflowException)
				{
					throw new NConsolerException("Value \"{0}\" is too big or too small", value);
				}
			}
			
			if (argumentType == typeof (string))
			{
				return value;
			}
			
			if (argumentType == typeof (bool))
			{
				try
				{
					return Convert.ToBoolean(value);
				}
				catch (FormatException)
				{
					throw new NConsolerException("Could not convert \"{0}\" to boolean", value);
				}
			}
			
			if (argumentType == typeof (string[]))
			{
				return value.Split('+');
			}
			
			if (argumentType == typeof (int[]))
			{
				var values = value.Split('+');
				var valuesArray = new int[values.Length];
				for (var i = 0; i < values.Length; i++)
				{
					valuesArray[i] = (int) ConvertValue(values[i], typeof (int));
				}
				return valuesArray;
			}
			
			if (argumentType == typeof (DateTime))
			{
				return ConvertToDateTime(value);
			}
			
			throw new NConsolerException("Unknown type is used in your method {0}", argumentType.FullName);
		}

		private static string GetDisplayName(ParameterInfo parameter)
		{
			if (IsRequired(parameter))
			{
				return parameter.Name;
			}
			
			var optional = GetOptional(parameter);
			var parameterName =
				(optional.AltNames.Length > 0) ? optional.AltNames[0] : parameter.Name;
			
			if (parameter.ParameterType != typeof (bool))
			{
				parameterName += ":" + ValueDescription(parameter.ParameterType);
			}
			
			return "[/" + parameterName + "]";
		}

		private static string GetMethodDescription(MethodInfo method)
		{
			var attributes = method.GetCustomAttributes(true);
			foreach (var attribute in attributes)
			{
				if (attribute is ActionAttribute)
				{
					return ((ActionAttribute) attribute).Description;
				}
			}
			
			throw new NConsolerException("Method is not marked with an Action attribute");
		}

		private static OptionalAttribute GetOptional(ICustomAttributeProvider info)
		{
			var attributes = info.GetCustomAttributes(typeof (OptionalAttribute), false);
			return (OptionalAttribute) attributes[0];
		}

		private static Dictionary<string, string> GetParametersDescriptions(MethodInfo method)
		{
			var parameters = new Dictionary<string, string>();
			foreach (var parameter in method.GetParameters())
			{
				var parameterAttributes =
					parameter.GetCustomAttributes(typeof (ParameterAttribute), false);
				if (parameterAttributes.Length > 0)
				{
					var name = GetDisplayName(parameter);
					var attribute = (ParameterAttribute) parameterAttributes[0];
					parameters.Add(name, attribute.Description);
				}
				else
				{
					parameters.Add(parameter.Name, String.Empty);
				}
			}
			
			return parameters;
		}

		private static bool IsOptional(ICustomAttributeProvider info)
		{
			return !IsRequired(info);
		}

		private static bool IsRequired(ICustomAttributeProvider info)
		{
			var attributes = info.GetCustomAttributes(typeof (ParameterAttribute), false);
			return attributes.Length == 0 || attributes[0].GetType() == typeof (RequiredAttribute);
		}

		private static int MaxKeyLength(IEnumerable<KeyValuePair<string, string>> parameters)
		{
			var maxLength = 0;
			foreach (var pair in parameters)
			{
				if (pair.Key.Length > maxLength)
				{
					maxLength = pair.Key.Length;
				}
			}
			
			return maxLength;
		}

		private static bool OnlyOptionalParametersSpecified(MethodBase method)
		{
			return method.GetParameters().All(parameter => !IsRequired(parameter));
		}

		private static string ParameterName(string parameter)
		{
			if (parameter.StartsWith("/-"))
			{
				return parameter.Substring(2).ToLower();
			}
			
			if (parameter.Contains(":"))
			{
				return parameter.Substring(1, parameter.IndexOf(":") - 1).ToLower();
			}
			
			return parameter.Substring(1).ToLower();
		}

		private static string ParameterValue(string parameter)
		{
			if (parameter.StartsWith("/-"))
			{
				return "false";
			}
			
			if (parameter.Contains(":"))
			{
				return parameter.Substring(parameter.IndexOf(":") + 1);
			}
			
			return "true";
		}

		private static int RequiredParameterCount(MethodInfo method)
		{
			return method.GetParameters().Count(IsRequired);
		}

		private static string ValueDescription(Type type)
		{
			if (type == typeof (int))
			{
				return "number";
			}
			
			if (type == typeof (string))
			{
				return "value";
			}
			
			if (type == typeof (int[]))
			{
				return "number[+number]";
			}
			
			if (type == typeof (string[]))
			{
				return "value[+value]";
			}
			
			if (type == typeof (DateTime))
			{
				return "dd-mm-yyyy";
			}
			
			throw new ArgumentOutOfRangeException(String.Format("Type {0} is unknown", type.Name));
		}

		#endregion

		#region Nested type: ParameterData

		private struct ParameterData
		{
			#region Readonly & Static Fields

			public readonly int m_Position;
			public readonly Type m_Type;

			#endregion

			#region Constructors

			public ParameterData(int position, Type type)
			{
                this.m_Position = position;
                this.m_Type = type;
			}

			#endregion
		}

		#endregion
	}

	/// <summary>
	/// Used for getting messages from NConsoler
	/// </summary>
	public interface IMessenger
	{
		#region Instance Methods

		void Write(string message);

		#endregion
	}

	/// <summary>
	/// Uses Console class for message output
	/// </summary>
	public class ConsoleMessenger : IMessenger
	{
		#region IMessenger Members

		public void Write(string message)
		{
			Console.WriteLine(message);
		}

		#endregion
	}

	/// <summary>
	/// Every action method should be marked with this attribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public sealed class ActionAttribute : Attribute
	{
		#region Fields

		private string _description = String.Empty;

		#endregion

		#region Constructors

		public ActionAttribute()
		{
		}

		public ActionAttribute(string description)
		{
            this._description = description;
		}

		#endregion

		#region Instance Properties

		/// <summary>
		/// Description is used for help messages
		/// </summary>
		public string Description
		{
			get { return this._description; }

			set { this._description = value; }
		}

		#endregion
	}

	/// <summary>
	/// Should not be used directly
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public class ParameterAttribute : Attribute
	{
		#region Fields

		private string _description = String.Empty;

		#endregion

		#region Constructors

		protected ParameterAttribute()
		{
		}

		#endregion

		#region Instance Properties

		/// <summary>
		/// Description is used in help message
		/// </summary>
		public string Description
		{
			get { return this._description; }

			set { this._description = value; }
		}

		#endregion
	}

	/// <summary>
	/// Marks an Action method parameter as optional
	/// </summary>
	public sealed class OptionalAttribute : ParameterAttribute
	{
		#region Constructors

		/// <param name="defaultValue">Default value if client doesn't pass this value</param>
		public OptionalAttribute(object defaultValue)
		{
            this.Default = defaultValue;
            this.AltNames = new string[0];
		}

		#endregion

		#region Instance Properties

		public string[] AltNames { get; set; }
		public object Default { get; private set; }

		#endregion
	}

	/// <summary>
	/// Marks an Action method parameter as required
	/// </summary>
	public sealed class RequiredAttribute : ParameterAttribute
	{
	}

	/// <summary>
	/// Can be used for safe exception throwing - NConsoler will catch the exception
	/// </summary>
	public sealed class NConsolerException : Exception
	{
		#region Constructors

		public NConsolerException()
		{
		}

		public NConsolerException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public NConsolerException(string message, params string[] arguments)
			: base(String.Format(message, arguments))
		{
		}

		#endregion
	}
}