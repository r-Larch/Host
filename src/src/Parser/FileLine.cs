using System.Text;


namespace Larch.Host.Parser {
    public class FileLine : IFileLine {
        public bool IsDisabled { get; set; }
        public string Ip { get; set; }
        public string Domain { get; set; }
        public string Commentar { get; set; }
        public bool IsCommentarLine { get; set; }
    }
}