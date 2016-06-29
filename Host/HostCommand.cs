using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Host
{
    public class HostCommand
    {
        public string MethodName { get; set; }

        private List<string> _arguments;
        public IEnumerable<string> Arguments
        {
            get
            {
                return _arguments;
            }
        }

        public HostCommand()
        {
            MethodName = string.Empty;
            _arguments = new List<string>();
        }

        public HostCommand(string [] args) : this()
        {
            MethodName = args[0];

            if(args.Length > 1)
            {
                _arguments.AddRange(args.Skip(1));
            }
        }

        public HostCommand(string userInput) : this()
        {
            var parts = Regex.Matches(userInput, @"[\""].+?[\""]|[^ ]+")
                        .Cast<Match>()
                        .Select(x => x.Value.Replace("\"", ""))
                        .ToArray();

            if (parts.Length > 0)
            {
                MethodName = parts[0];
     
                _arguments.AddRange(parts.Skip(1));
            }
        }
    }
}