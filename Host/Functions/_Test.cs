using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Host
{
    /// <summary>
    /// Test functions for the host process that demo it's capabilities
    /// </summary>
    public partial class Program
    {
        [Description("Test1 - Displays a single int inputted by the user")]
        static void Test_Test1(int num)
        {
            int total = num;

            Console.WriteLine(total);
        }

        [Description("Test1 (Overload) - Adds two ints inputted by the user and displays the result")]
        static void Test_Test1(int num, int num2)
        {
            int total = num + num2;

            Console.WriteLine(total);
        }

        [Description("Test2 - Single Default Parameter")]
        static void Test_Test2(int num = 5)
        {
            int total = num;

            Console.WriteLine(total);
        }

        [Description("Test3 - Multiple Default Parameters")]
        static void Test_Test3(int num = 10, int num2 = 20, int num3 = 30, int num4 = 40)
        {
            int total = num + num2 + num3 + num4;

            Console.WriteLine(total);
        }
    }
}
