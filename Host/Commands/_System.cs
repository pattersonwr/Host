﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using ConsoleExtender;
using Host.Extensions;

namespace Host
{
    /// <summary>
    /// Contains System Functions For the Host Process
    /// </summary>
    public partial class Program
    {
        [Category("System")]
        [Description("Shows Help for All Commands or specific commands based on a keyword")]
        static void Help(string keyWord = "")
        {
            var criteria = GetSearchCriteria(keyWord);

            foreach (MethodDetails md in criteria)
            {
                md.DisplayHelpText();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nUse 'exit' or 'quit' to leave the application");
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static IEnumerable<MethodDetails> GetSearchCriteria(string keyWord)
        {
            var searchCriteria = _methodDictionary.Values.Where(x => x.Category.ToLower() == keyWord.ToLower());

            if (keyWord == string.Empty || searchCriteria.Count() == 0)
            {
                searchCriteria = _methodDictionary.Values;
                Console.WriteLine("All Valid Commands");
            }

            else
            {
                Console.WriteLine("Valid {0} Commands", searchCriteria.ToArray()[0].Category);
            }

            return searchCriteria;
        }

        [Category("System")]
        [Description("Open Application Log Folder")]
        static void OpenLogFolder()
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

        [Category("System")]
        [Description("Get Font Sizes for the Console Window")]
        static void GetFontSizes()
        {
            var fonts = ConsoleHelper.ConsoleFonts;

            for (int f = 0; f < fonts.Length; f++)
                Console.WriteLine("{0}: X={1}, Y={2}", fonts[f].Index, fonts[f].SizeX, fonts[f].SizeY);
        }

        [Category("System")]
        [Description("Set the Font Size for the Console Window")]
        static void SetFontSize(string size, bool clearBuffer = false)
        {
            if (string.IsNullOrEmpty(size))
                return;

            uint x = 0;
            if (uint.TryParse(size, out x))
            {
                ConsoleHelper.SetConsoleFont(x);

                if (clearBuffer)
                    Clear();
            }
        }

        [Category("System")]
        [Description("Clears the Current display Buffer")]
        static void Clear()
        {
            Console.Clear();
        }

        [Category("System")]
        [Description("List Local Drives")]
        static void LocalDrives()
        {
            string[] drives = Environment.GetLogicalDrives();
            IEnumerable<string> strs = drives.Select(s => s.Replace(":\\", ""));
            foreach (String s in strs)
            {
                DriveInfo drvi = new DriveInfo(s);
                if (drvi.DriveType == DriveType.CDRom)
                    continue;
                Console.WriteLine("{0}:\\", s);
            }
        }

        [Category("System")]
        [Description("List Available Providers")]
        static void LocalProviders()
        {
            var dt = System.Data.Common.DbProviderFactories.GetFactoryClasses();
            //	Name Description InvariantName
            Console.WriteLine("{0} {1}\t {2}\t", "", "Name", "InvariantName");
            Console.WriteLine("-------------------------------------");
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                Console.WriteLine("{0} {1}\t {2}\t", "", dr["Name"], dr["InvariantName"]);
            }
        }
    }
}
