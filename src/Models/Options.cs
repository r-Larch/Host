using System.Text;
using CommandLine;
using CommandLine.Text;


namespace Larch.Host.Models {
    internal class Options {
        [ValueOption(0)]
        public string Value { get; set; }

        [Option('e', "edit", HelpText = "Edit the hosts file in editor. set %EDITOR% to use your favorite editor.")]
        public bool Edit { get; set; }

        [Option('l', "list", HelpText = "List using wildcards or regex")]
        public bool List { get; set; }

        [Option('a', "add", HelpText = "Add to hosts file")]
        public bool Add { get; set; }

        [Option('r', "remove", HelpText = "Remove from hosts file")]
        public bool Remove { get; set; }

        [Option('f', "force", HelpText = "Use force (e.g. force remove)")]
        public bool Force { get; set; }

        [Option('i', "ip", HelpText = "Filter by ip address")]
        public bool Ip { get; set; }

        [Option('n', "line", HelpText = "Filter by line number")]
        public bool Line { get; set; }

        [Option('c', "comment", HelpText = "Filter by comment")]
        public bool Commentar { get; set; }

        [Option('s', "disabled", HelpText = "show disabled lines")]
        public bool IsDisabled { get; set; }

        [Option('R', "regex", HelpText = "Use regex for filter")]
        public bool Regex { get; set; }

        [Option('d', "debug", HelpText = "Enables debuging")]
        public bool Debug { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage() {
            var sb = new StringBuilder();
            sb.AppendLine(" Usage: hosts [OPTIONS] VALUE");
            sb.AppendLine(" Shorthand for add: hosts VALUE");

            var helpText = new HelpText() {
                Heading = sb.ToString(),
                AdditionalNewLineAfterOption = false,
                AddDashesToOption = true,
                Copyright = "Copyright 2017 René Larch"
            };

            helpText.AddOptions(this);

            return helpText;
        }
    }
}