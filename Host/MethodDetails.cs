using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Host.Extensions;

namespace Host
{
    public class MethodDetails
    {
        public MethodInfo Method { get; }

        public string Category { get; }

        public string Description { get; }

        public string HelpText { get; }

        public IEnumerable<ParameterInfo> MethodParameters { get; }

        public MethodDetails(MethodInfo method)
        {
            Method = method;
            Description = (method.HasAttribute<DescriptionAttribute>() ? method.GetCustomAttribute<DescriptionAttribute>(false).Description : string.Empty);
            Category = ( method.HasAttribute<CategoryAttribute>() ? method.GetCustomAttribute<CategoryAttribute>(false).Category : string.Empty );
            HelpText = string.Empty;

            if (Description != string.Empty)
            {
                HelpText = GenerateHelpText();
            }

            MethodParameters = method.GetParameters();
        }

        private string GenerateHelpText()
        {
            StringBuilder sb = new StringBuilder();
            ParameterInfo[] parms = Method.GetParameters();

            sb.Append(Method.Name + "/");
            sb.Append("(");

            for (int i = 0; i < parms.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");

                if (parms[i].HasDefaultValue)
                    sb.Append(string.Format("({0}){1}={2}", parms[i].ParameterType.Name, parms[i].Name, parms[i].DefaultValue));
                else
                {
                    if (!parms[i].ParameterType.IsGenericType)
                        sb.Append(string.Format("({0}){1}", parms[i].ParameterType.Name, parms[i].Name));
                    else
                        sb.Append(string.Format("({0}[{1}]){2}", parms[i].ParameterType.Name, parms[i].ParameterType.GenericTypeArguments[0].Name, parms[i].Name));
                }

            }

            sb.Append(")/");
            sb.Append(string.Format("\n\t{0}", Description));

            return sb.ToString();
        }

        public void DisplayHelpText()
        {
            if (HelpText != string.Empty)
            {
                var text = HelpText.Split('/');

                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(text[0]);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(text[1]);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(text[2]);
            }
        }
    }
}
