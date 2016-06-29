using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ConsoleExtender;
using Host.Extensions;

namespace Host
{
    // Some of the code used in this project can be found here: 
    // http://johnatten.com/2014/09/07/c-building-a-useful-extensible-net-console-application-template-for-development-and-testing/

    partial class Program
    {
        //	An internal pointer to the current Instance of the class
        //	used to reference Program and avoid using Activator Create Instance Calls
        //	'this' will not work for static method calls
        protected static Program m_this;
        protected static string m_prompt = ":>";
        protected static Type m_type = typeof(Program);
        protected static Dictionary<string, MethodDetails> _methodDictionary;

        internal Program()
        {
            m_this = this;
        }

        [STAThread]
        static void Main(string[] args)
        {
            CreateMethodCache();
            AddTraceListeners();
            SetupConsole();

            if (args.Length > 0)
            {
                ExecuteCommand(new HostCommand(args));
                return;
            }

            MainLoop();
        }

        #region Setup

        static void CreateMethodCache()
        {
            _methodDictionary = new Dictionary<string, MethodDetails>(StringComparer.OrdinalIgnoreCase);

            // Load method info into dictionary
            var methods = m_type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).ToList();
            foreach (var method in methods)
            {
                _methodDictionary.Add(method.Name, new MethodDetails(method));
            }

            List<string> x = new List<string>();
        }

        static void AddTraceListeners()
        {
            TextWriterTraceListener CWriter = new TextWriterTraceListener(Console.Out);
            Trace.Listeners.Add(CWriter);
        }

        static void SetupConsole()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.BufferHeight = 300;
            Console.BufferWidth = 100;
            Console.SetWindowSize(100, 25);
            Console.Title = "Host";

            ConsoleHelper.SetConsoleFont(9);
        }

        #endregion

        static void MainLoop()
        {
            bool pContinue = true;

            while (pContinue)
            {
                Console.Write(m_prompt);

                string input = Console.ReadLine();

                if (input == string.Empty)
                {
                    Console.Clear();
                    continue;
                }

                var command = new HostCommand(input);

                switch (command.MethodName)
                {
                    case "exit":
                    case "quit":
                        pContinue = false;
                        break;

                    default:
                        ExecuteCommand(command);
                        break;
                }

                Console.WriteLine("");
            }
        }

        static void ExecuteCommand(HostCommand cmd)
        {
            try
            {
                // Validate The Passed in Command
                if (!ValidateCommand(cmd))
                {
                    Console.WriteLine("{0} is an invalid command", cmd.MethodName);
                    Help();
                    return;
                }

                // Make sure the user provided the correct number of arguments
                MethodDetails methodDetails = _methodDictionary[cmd.MethodName];

                if (!ValidateParameters(methodDetails, cmd.Arguments.Count()))
                    return;

                object[] inputs = GetMethodParams(methodDetails, cmd.Arguments);

                InvokeMethod(methodDetails, inputs);
            }
            catch (Exception e)
             {
                Console.WriteLine();
                WriteExceptions(e);

                if (e.InnerException != null)
                {
                    Console.WriteLine();
                    WriteExceptions(e.InnerException);
                }
            }
        }

        static bool ValidateCommand(HostCommand cmd)
        {
            if (!_methodDictionary.ContainsKey(cmd.MethodName))
                return false;

            return true;
        }

        static bool ValidateParameters(MethodDetails md, int argumentCount)
        {
            int requiredCount = md.MethodParameters.Where(p => p.IsOptional == false).Count();
            int optionalCount = md.MethodParameters.Where(p => p.IsOptional == true).Count();
            int providedCount = argumentCount;

            if (requiredCount > providedCount)
            {
                Console.WriteLine("Failed to provide the correct number of required arguments.");
                Console.WriteLine("You provided {0} arguments, there are {1} required arguments, and {2} optional arguments." + Environment.NewLine,
                    providedCount, requiredCount, optionalCount);

                // Display Help Text for the Method
                md.DisplayHelpText();

                return false;
            }

            return true;
        }

        static object[] GetMethodParams(MethodDetails method, IEnumerable<string> args)
        {
            var methodParams = new List<object>();
            if (method.MethodParameters.Count() > 0)
            {
                foreach (var param in method.MethodParameters)
                {
                    methodParams.Add(param.DefaultValue);
                }

                if (args.Count() == 0)
                    return methodParams.ToArray();

                if (method.MethodParameters.HasParameterType<List<string>>())
                {
                    List<string> arguments = new List<string>();

                    for (int i = 0; i < args.Count(); ++i)
                    {
                        var arg = args.ElementAt(i);

                        if (args.ElementAt(i).Contains(","))
                        {
                            var s = arg.Replace(",", "");

                            if (s != string.Empty)
                                arguments.Add(s);
                        }
                        else
                            arguments.Add(arg);
                    }

                    // Reset args
                    args = new List<string>() { string.Join(",", arguments.ToArray()) };
                }

                for (int i = 0; i < args.Count(); ++i)
                {
                    var methodParam = method.MethodParameters.ElementAt(i);
                    var requiredType = methodParam.ParameterType;

                    object value = null;

                    try
                    {
                        MethodInfo changeTypeMethod = null;

                        if(!requiredType.IsGenericType)
                            changeTypeMethod = typeof(HostExtensions).GetMethod("ChangeType");
                        else
                            changeTypeMethod = typeof(HostExtensions).GetMethod("ChangeTypeList");

                        var genericMethod = changeTypeMethod.MakeGenericMethod(requiredType);

                        value = genericMethod.Invoke(null, new object[] { args.ElementAt(i), CultureInfo.CurrentCulture });

                        methodParams[i] = value;
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException(string.Format("The value for '{0}' cannot be parsed to '{1}' ", methodParam.Name, requiredType.Name), ex);
                    }
                }
            }

            return methodParams.ToArray();
        }

        static void InvokeMethod(MethodDetails method, object[] paramArray)
        {
            try
            {
                method.Method.Invoke(m_this, paramArray);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        static void WriteExceptions(Exception e)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;

            Trace.Write("Source:");
            Trace.Write(e.Source);
            Trace.WriteLine("\nMessage:");
            Trace.Write(e.Message);
            Trace.WriteLine("\nStack Trace:");
            Trace.Write(e.StackTrace);
            Trace.WriteLine("\nUser Defined Data:");

            foreach (System.Collections.DictionaryEntry de in e.Data)
            {
                Trace.WriteLine(string.Format("[{0}] :: {1}", de.Key, de.Value));
            }

            if (e.InnerException != null)
            {
                WriteExceptions(e.InnerException);
            }

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
