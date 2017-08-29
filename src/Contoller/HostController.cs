using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Larch.Host.Parser;
using LarchConsole;


namespace Larch.Host.Contoller {
    internal class HostController {
        private readonly HostsFile _hostsFile;

        public HostController(HostsFile hostsFile) {
            _hostsFile = hostsFile;
        }

        public void Add(string ip, string url) {
            string line;
            using (new Watch("add")) {
                // throws if invalid
                IPAddress.Parse(ip);

                var host = url;
                try {
                    var uri = new Uri(url);
                    host = uri.Host;
                }
                catch {
                    // ignore
                }

                line = _hostsFile.Append(new FileLine() {
                    Ip = ip,
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

        public void Duplicates(Filter filter, FilterProp what) {
            List<HostsFileLine> hosts;

            using (new Watch("read file")) {
                hosts = _hostsFile.GetHosts().ToList();
            }

            var filterd = Filter(hosts, filter, what);

            var duplicates = (from f in filterd
                group f by $"{f.Ip.Value} {f.Domain.Value}"
                into g
                where g.Count() > 1
                select g).ToList();

            ConsoleEx.PrintWithPaging(
                list: duplicates,
                countAll: hosts.Count,
                line: (models, i) => new ConsoleWriter()
                    .FormatLine("{count,5} {domain}", parms => parms
                        .Add("count", models.Count())
                        .Add("domain", models.Key)
                    )
                    .FormatLines("          {ip} {domain}", models.ToList(), (model, parms) => parms
                        .Add("ip", model.Ip)
                        .Add("domain", model.Domain)
                    )
            );

            if (duplicates.Any()) {
                var toremove = duplicates.SelectMany(x => x.Skip(1).Select(_ => _.Model)).ToList();
                if (ConsoleEx.AskForYes($"remove {toremove.Count} duplicates?")) {
                    _hostsFile.Remove(toremove);
                    toremove.ForEach(x => Console.WriteLine($"removed: {HostsFile.CreateTextLine(x)}"));
                }
            }
        }

        public void List(Filter filter, FilterProp what) {
            List<HostsFileLine> hosts;

            using (new Watch("read file")) {
                hosts = _hostsFile.GetHosts().ToList();
            }

            var filterd = Filter(hosts, filter, what);

            Print(filterd, hosts.Count, what);
        }

        private void Print(List<HostModel> filterd, int countall, FilterProp what) {
            using (new Watch("print")) {
                ConsoleEx.PrintWithPaging(
                    list: filterd,
                    countAll: countall,
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