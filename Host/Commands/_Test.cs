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
        [Category("Test")]
        [Description("Test1 - Displays a single int inputted by the user")]
        static void Test1(int num)
        {
            int total = num;

            Console.WriteLine(total);
        }

        [Category("Test")]
        [Description("Test2 - Single Default Parameter")]
        static void Test2(int num = 5)
        {
            int total = num;

            Console.WriteLine(total);
        }

        [Category("Test")]
        [Description("Test3 - List of Ints")]
        static void Test3(List<int> nums)
        {
            int total = 0;

            foreach (int num in nums)
            {
                total += num;
            }

            Console.WriteLine(total);
        }

        [Category("Test")]
        [Description("Test4 - List of Ints, int parameter")]
        static void Test4(List<int> nums, int multiplier)
        {
            int total = 0;

            foreach (int num in nums)
            {
                total += num;
            }

            total *= multiplier;

            Console.WriteLine(total);
        }

        [Category("Test")]
        [Description("Test5 - string")]
        static void Test5(string s)
        {
            Console.WriteLine(s);
        }

        [Category("Test")]
        [Description("Test6 - List of Strings")]
        static void Test6(List<string> strings)
        {
            foreach (string s in strings)
            {
                Console.WriteLine(s);
            }
        }
    }
}
