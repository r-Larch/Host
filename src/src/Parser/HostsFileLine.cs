using System;
using System.Text.RegularExpressions;


namespace Larch.Host.Parser {
    public class HostsFileLine : IFileLine {

        /// <summary>
        /// Ignores Ipv6 [Time 44ms in test at 16000 lines]
        /// </summary>
        //private static readonly Regex Regex = new Regex(@"^([#]?)\s*(\d+.\d+.\d+.\d+)[\s|\t]+([a-zA-Z0-9-.]+)([#]?[^\n]*)");

        /// <summary>
        /// not a goot Ipv6 pattern [Time 47ms in test at 16000 lines]
        /// </summary>
        //private static readonly Regex RegexBetter = new Regex(@"^([#]?)\s*([0-9.a-fA-F:]+[%]?[l]?[o]?[0]?)[\s|\t]+([a-zA-Z0-9-.]+)([#]?[^\n]*)");

        /// <summary>
        /// Fale positive Ipv6 [Time 56ms in test at 16000 lines]
        /// </summary>
        //private static readonly Regex RegexBetter2 = new Regex(@"^([#]?)\s*((\d+.\d+.\d+.\d+)|([a-fA-F:%]+[\S]+?))[\s|\t]+([a-zA-Z0-9-.]+)([#]?[^\n]*)");

        /// <summary>
        /// group: 1 = '#' => disabled
        /// group: 2 => ip
        /// group: 3 => ipv4
        /// group: 4 => ipv6
        /// group: 34 => host
        /// group: 35 => commentar
        /// [Time 91ms in test at 16000 lines] takes longer, but is more secure
        /// </summary>
        private static readonly Regex RegexBest = new Regex(@"^\s*([#]?)\s*((\d+.\d+.\d+.\d+)|(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))(?=\s|$))[\s|\t]+([a-zA-Z0-9-.]+)([#]?[^\n]*)");


        public HostsFileLine(int lineNumber) {
            LineNumber = lineNumber;
        }

        //public void Parse1(string line) {
        //    var match = Regex.Match(line);
        //    if (match.Success) {
        //        IsDisabled = match.Groups[1].Value == "#";
        //        Ipv4 = match.Groups[2].Value;
        //        //Ipv6 = match.Groups[4].Value;
        //        Domain = match.Groups[3].Value;
        //        Commentar = match.Groups[4].Value;
        //    } else {
        //        IsCommentarLine = true;
        //    }
        //}

        //public void Parse2(string line) {
        //    var match = RegexBetter.Match(line);
        //    if (match.Success) {
        //        IsDisabled = match.Groups[1].Value == "#";
        //        Ipv4 = match.Groups[2].Value;
        //        //Ipv6 = match.Groups[4].Value;
        //        Domain = match.Groups[3].Value;
        //        Commentar = match.Groups[4].Value;
        //    } else {
        //        IsCommentarLine = true;
        //    }
        //}

        //public void Parse3(string line) {
        //    var match = RegexBetter2.Match(line);
        //    if (match.Success) {
        //        IsDisabled = match.Groups[1].Value == "#";
        //        Ipv4 = match.Groups[3].Value;
        //        Ipv6 = match.Groups[4].Value;
        //        Domain = match.Groups[5].Value;
        //        Commentar = match.Groups[6].Value;
        //    } else {
        //        IsCommentarLine = true;
        //    }
        //}

        public void Parse(string line) {
            var match = RegexBest.Match(line);
            if (match.Success) {
                IsDisabled = match.Groups[1].Value == "#";
                Ipv4 = match.Groups[3].Value;
                Ipv6 = match.Groups[4].Value;
                Domain = match.Groups[34].Value;
                Commentar = match.Groups[35].Value;
            } else {
                IsCommentarLine = true;
            }
        }

        public int LineNumber { get; set; }
        public bool IsDisabled { get; set; }

        public string Ip {
            get { return string.IsNullOrEmpty(Ipv4) ? Ipv6 : Ipv4; }
            set { throw new NotSupportedException("Ip is readonly");}
        }

        public string Ipv4 { get; set; }
        public string Ipv6 { get; set; }
        public string Domain { get; set; }
        public string Commentar { get; set; }

        public bool IsCommentarLine { get; set; }
    }
}