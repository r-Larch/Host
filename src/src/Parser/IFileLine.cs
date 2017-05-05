namespace Larch.Host.Parser {
    public interface IFileLine {
        //int LineNumber { get; set; }
        bool IsDisabled { get; set; }
        string Ip { get; set; }
        //string Ipv4 { get; set; }
        //string Ipv6 { get; set; }
        string Domain { get; set; }
        string Commentar { get; set; }

        bool IsCommentarLine { get; set; }
    }
}