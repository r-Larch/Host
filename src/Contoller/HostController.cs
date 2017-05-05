using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Larch.Host.Parser;
using LarchConsole;


namespace Larch.Host.Contoller {
    internal class HostController {
        private readonly HostsFile _hostsFile;

        public HostController(HostsFile hostsFile) {
            _hostsFile = hostsFile;
        }

        public void Add(string host) {
            string line;
            using (new Watch("add")) {
                line = _hostsFile.Append(new FileLine() {
                    Ip = "127.0.0.1",
                    Domain = host.Trim()
                });
            }

            Console.WriteLine($"added successfully '{line}'");
            Console.WriteLine();
        }

        public void Edit() {
            Executor.OpenEditor(new FileInfo(_hostsFile.FilePath)).StartNormal();
            Console.WriteLine("editor is starting...");
        }

        public void List(Filter filter, FilterProp what) {
            List<HostsFileLine> hosts;

            using (new Watch("read file")) {
                hosts = _hostsFile.GetHosts().ToList();
            }

            var filterd = Filter(hosts, filter, what);

            using (new Watch("print")) {
                ConsoleEx.PrintWithPaging(
                    list: filterd,
                    countAll: hosts.Count,
                    header: ConsoleWriter.CreateLine(" Line |"),
                    line: (x, nr) => new ConsoleWriter()
                        .FormatLine("{line,6}|{disabled} {ip}   {domain}    {comment}", parms => parms
                            .Add("line", x.LineNumber, what == FilterProp.Line)
                            .Add("disabled", x.IsDisabled ? "#" : "")
                            .Add("ip", x.Ip, what == FilterProp.Domain)
                            .Add("domain", x.Domain, what == FilterProp.Domain)
                            .Add("comment", x.Commentar, what == FilterProp.Commentar)
                        )
                    );
            }
        }

        public void Remove(Filter filter, FilterProp what, bool force) {
            List<HostsFileLine> hosts;
            using (new Watch("read file")) {
                hosts = _hostsFile.GetHosts().ToList();
            }

            var filterd = Filter(hosts, filter, what);

            if (!force) {
                Console.WriteLine($"found {filterd.Count} to remove\r\n");
                filterd = ConsoleEx.AskYesOrNo(filterd, x => new ConsoleWriter()
                    .FormatLine("remove '{line} {disabled} {ip}    {domain} {comment}'?", parms => parms
                        .Add("line", x.LineNumber, what == FilterProp.Line)
                        .Add("disabled", x.IsDisabled ? "#" : "")
                        .Add("ip", x.Ip, what == FilterProp.Domain)
                        .Add("domain", x.Domain, what == FilterProp.Domain)
                        .Add("comment", x.Commentar, what == FilterProp.Commentar)
                    )
                );
            }

            if (filterd.Count == 0) {
                Console.WriteLine($"-- nothing to remove");
                return;
            }

            using (new Watch("delete")) {
                _hostsFile.Remove(filterd.Select(x => x.Model));
            }
            filterd.ForEach(x => Console.WriteLine($"removed: {HostsFile.CreateTextLine(x.Model)}"));
        }

        private List<HostModel> Filter(List<HostsFileLine> hosts, Filter filter, FilterProp what) {
            using (new Watch("filter")) {
                var filterd = hosts.Select(x => new HostModel() {
                    Model = x,
                    LineNumber = filter.GetMatch(x.LineNumber),
                    Ip = filter.GetMatch(x.Ip),
                    Domain = filter.GetMatch(x.Domain),
                    Commentar = filter.GetMatch(x.Commentar),
                    IsCommentarLine = x.IsCommentarLine,
                    IsDisabled = x.IsDisabled
                });

                switch (what) {
                    default:
                    case FilterProp.Domain:
                        return filterd.Where(x => x.Domain.IsSuccess).ToList();
                    case FilterProp.Ip:
                        return filterd.Where(x => x.Ip.IsSuccess).ToList();
                    case FilterProp.Line:
                        return filterd.Where(x => x.LineNumber.IsSuccess).ToList();
                    case FilterProp.Commentar:
                        return filterd.Where(x => x.Commentar.IsSuccess).ToList();
                    case FilterProp.IsDisabled:
                        return filterd.Where(x => x.IsDisabled).ToList();
                }
            }
        }
    }


    internal class HostModel {
        public HostsFileLine Model { get; set; }
        public Match<int> LineNumber { get; set; }
        public Match<string> Ip { get; set; }
        public Match<string> Domain { get; set; }
        public Match<string> Commentar { get; set; }
        public bool IsCommentarLine { get; set; }
        public bool IsDisabled { get; set; }
    }


    internal enum FilterProp {
        Domain,
        Ip,
        Line,
        Commentar,
        IsDisabled
    }
}