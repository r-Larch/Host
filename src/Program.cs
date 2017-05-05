using System;
using Larch.Host.Contoller;
using Larch.Host.Models;
using Larch.Host.Parser;
using LarchConsole;


namespace Larch.Host {
    public class Program {
        public static void Main(string[] args) {
            try {
                var p = new Program();
                var options = new Options();

                var parser = new CommandLine.Parser(settings => settings.CaseSensitive = true);
                if (parser.ParseArguments(args, options)) {
                    p.Run(options);
                } else {
                    // print help
                    Console.WriteLine(options.GetUsage());
                }

                if (options.Debug) {
                    Watch.PrintTasks();
                }
            } catch (Exception e) {
                ConsoleEx.PrintException(e.Message, e);
            }
        }


        private void Run(Options options) {
            var hostfile = new HostsFile();
            var host = new HostController(hostfile);

            // edit
            if (options.Edit) {
                host.Edit();
                return;
            }


            // setup filter
            var filter = new Filter(options.Value,
                options.Regex
                    ? CampareType.Regex
                    : CampareType.WildCard,
                CompareMode.CaseIgnore
                );
            var filterProp = FilterProp.Domain;
            if (options.Ip) {
                filterProp = FilterProp.Ip;
            }
            if (options.Line) {
                filterProp = FilterProp.Line;
            }
            if (options.Commentar) {
                filterProp = FilterProp.Commentar;
            }
            if (options.IsDisabled) {
                filterProp = FilterProp.IsDisabled;
            }

            // list
            if (options.List) {
                filter.OnEmptyMatchAll = true;
                host.List(filter, filterProp);
                return;
            }

            // handle empty value
            if (string.IsNullOrEmpty(options.Value)) {
                Console.WriteLine(options.GetUsage());
                return;
            }

            // remove value
            if (options.Remove) {
                host.Remove(filter, filterProp, options.Force);
                return;
            }

            // add value
            if (options.Add || !string.IsNullOrEmpty(options.Value)) {
                host.Add(options.Value);
                return;
            }
        }
    }
}