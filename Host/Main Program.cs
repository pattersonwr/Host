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
    partial class Program
    {
        //	An internal pointer to the current Instance of the class
        //	used to reference Program and avoid using Activator Create Instance Calls
        //	'this' will not work for static method calls
        protected static Program m_this;
        protected static string m_prompt = ":>";
        protected static Type m_type = typeof(Program);
        protected static List<MethodInfo> m_methods = null;

        internal Program()
        {
            m_this = this;
        }

        [STAThread]
        static void Main(string[] args)
        {
            CreateMethodCache();
            AddTraceListeners();

            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Clear();
            Console.BufferHeight = 300;
            Console.BufferWidth = 100;
            Console.SetWindowSize(100, 25);
            Console.Title = "Utility";

            ConsoleHelper.SetConsoleFont(9);

            if (args.Length > 0)
            {
                RunCommand(args);
                return;
            }

            MainLoop();
        }

        static void CreateMethodCache()
        {
            m_methods = m_type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).ToList();
        }

        static void AddTraceListeners()
        {
            TextWriterTraceListener CWriter = new TextWriterTraceListener(Console.Out);
            Trace.Listeners.Add(CWriter);
        }

        static void MainLoop()
        {
            bool pContinue = true;

            while (pContinue)
            {
                Console.Write(m_prompt);

                var parms = ParseCommand(Console.ReadLine());

                if (parms.Length == 0)
                    continue;

                switch (parms[0].ToLower())
                {
                    case "exit":
                    case "quit":
                        pContinue = false;
                        break;

                    default:
                        RunCommand(parms);
                        break;
                }

                Console.WriteLine("");
            }
        }

        static string[] ParseCommand(string args)
        {
            var parts = Regex.Matches(args, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(x => x.Value.Replace("\"", ""))
                .ToArray();

            return parts;
        }

        static void RunCommand(string[] args)
        {
            // We'll need the user's params to overwrite any defaults
            var userParams = args.Skip(1).Take(args.Count() - 1).ToArray();

            // Get the method
            var method = GetMethod(args[0], userParams.Length);

            if (method == null)
            {
                Console.WriteLine("Unknown Command");
                System_Help(string.Empty);
                return;
            }

            try
            {
                var parms = GetParams(method, userParams);

                method.Invoke(m_this, parms);
                return;
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    Console.WriteLine();
                    WriteExceptions(e.InnerException);
                }
                else
                {
                    Console.WriteLine();
                    WriteExceptions(e);
                }

                return;
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

        /// <summary>
        /// Returns the correct method based on user input
        /// </summary>
        /// <param name="method">Method Name Typed by the user</param>
        /// <param name="paramLength">Length of the parameters typed by user</param>
        /// <returns></returns>
        static MethodInfo GetMethod(string method, int paramLength)
        {
            MethodInfo methodInfo = (from m in m_methods
                                     where (m.Name.Contains("_") ? String.Compare(m.Name.Split('_')[1], method, true) == 0 : String.Compare(m.Name, method, true) == 0)
                                     && (m.GetParameters().Length == paramLength || m.GetParameters().HasDefaultParams())
                                     select m).FirstOrDefault();

            return methodInfo;
        }

        static object[] GetParams(MethodInfo methodCall, string[] userParams)
        {
            List<object> parameters = new List<object>();
            var methodParams = methodCall.GetParameters();

            // Get Default Parameters
            if(methodParams.HasDefaultParams())
                parameters = GetDefaultParams(methodCall, methodParams);

            for (int i = 0; i < methodParams.Count(); ++i)
            {
                if (userParams.Count() == 0 ||  i >= userParams.Count())
                    break;

                // We can't pass in a type variable into this method, so we'll need to use reflection
                // NOTE: Does not handle lists
                MethodInfo method = typeof(HostExtensions).GetMethod("ChangeType");
                MethodInfo generic = method.MakeGenericMethod(methodParams[i].ParameterType);
                var value = generic.Invoke(null, new object[] { userParams[i], CultureInfo.CurrentCulture });

                if (i < parameters.Count)
                    parameters[i] = value;
                else
                    parameters.Add(value);
            }

            return parameters.ToArray();
        }

        static List<object> GetDefaultParams(MethodInfo methodCall, ParameterInfo[] parameters)
        {
            List<object> defaults = new List<object>();

            foreach (var methodParam in parameters)
            {
                if (methodParam.HasDefaultValue)
                    defaults.Add(methodParam.DefaultValue);
                else
                {
                    // Add default depending on parameter type?
                    if (methodParam.ParameterType == typeof(string))
                        defaults.Add(string.Empty);

                    else
                        defaults.Add(0);
                }
            }

            return defaults;
        }
    }
}
