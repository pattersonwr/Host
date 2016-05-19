using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ConsoleExtender;

namespace Host
{
    /// <summary>
    /// Contains System Functions For the Host Process
    /// </summary>
    public partial class Program
    {
        [Description("Shows Help for All Commands or specific commands based on a keyword")]
        static void System_Help(string keyWord = "")
        {
            var criteria = GetSearchCriteria(keyWord);

            foreach (MethodInfo m in criteria)
            {
                DescriptionAttribute[] attribs = (DescriptionAttribute[])m.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attribs != null && attribs.Length > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;

                    if (m.Name.Contains('_'))
                        Console.Write(m.Name.Split('_')[1]);
                    else
                        Console.Write(m.Name);

                    ParameterInfo[] parm = m.GetParameters();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("(");

                    for (int i = 0; i < parm.Length; i++)
                    {
                        if (i > 0)
                            Console.Write(", ");

                        if (parm[i].HasDefaultValue)
                            Console.Write("({0}){1}={2}", parm[i].ParameterType.Name, parm[i].Name, parm[i].DefaultValue);
                        else
                            Console.Write("({0}){1}", parm[i].ParameterType.Name, parm[i].Name);

                    }

                    Console.Write(")");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\n\t{0}", attribs[0].Description);
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nUse 'exit' or 'quit' to leave the application");
            Console.ForegroundColor = ConsoleColor.White;

        }

        private static IEnumerable<MethodInfo> GetSearchCriteria(string keyWord)
        {
            var searchCriteria = (keyWord != string.Empty ? m_methods.Where(x => x.Name.ToLower().Contains(keyWord.ToLower())) : m_methods);

            if(keyWord == string.Empty)
            {
                Console.WriteLine("All Valid Commands");
            }

            else
            {
                Console.WriteLine("Valid {0} Commands", searchCriteria.ToArray()[0].Name.Split('_')[0]);
            }

            return searchCriteria;
        }

        [Description("Open Application Log Folder")]
        static void System_OpenLogFolder()
        {
            var path = Path.Combine(GetCurrentPath(), "ApplicationLogs");

            if (Directory.Exists(path) == false)
            {
                path = GetCurrentPath();
            }

            Process.Start(path);
        }

        private static string GetCurrentPath()
        {
            var asm = Assembly.GetExecutingAssembly();
            var fi = new FileInfo(asm.Location);
            return fi.DirectoryName;
        }

        [Description("Get Font Sizes for the Console Window")]
        static void System_GetFontSizes()
        {
            var fonts = ConsoleHelper.ConsoleFonts;

            for (int f = 0; f < fonts.Length; f++)
                Console.WriteLine("{0}: X={1}, Y={2}", fonts[f].Index, fonts[f].SizeX, fonts[f].SizeY);
        }

        [Description("Set the Font Size for the Console Window")]
        static void System_SetFontSize(string size, bool clearBuffer = false)
        {
            if (string.IsNullOrEmpty(size))
                return;

            uint x = 0;
            if (uint.TryParse(size, out x))
            {
                ConsoleHelper.SetConsoleFont(x);

                if (clearBuffer)
                    System_Clear();
            }
        }

        [Description("Clears the Current display Buffer")]
        static void System_Clear()
        {
            Console.Clear();
        }
    }
}
