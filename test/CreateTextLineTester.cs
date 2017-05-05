using System;
using Larch.Host.Parser;
using NUnit.Framework;


namespace Larch.Host.Test {
    [TestFixture]
    public class CreateTextLineTester {
        [Test]
        public void CreateTextLineTest() {
            var line = HostsFile.CreateTextLine(new HostsFileLine(0) {
                Ipv4 = "127.0.0.1",
                Domain = "lokalhost",
            });
            Assert.AreEqual("127.0.0.1 lokalhost", line);
            Console.WriteLine(line);

            line = HostsFile.CreateTextLine(new FileLine {
                IsDisabled = true,
                Ip = "127.0.0.1",
                Domain = "lokalhost",
            });
            Assert.AreEqual("# 127.0.0.1 lokalhost", line);
            Console.WriteLine(line);

            line = HostsFile.CreateTextLine(new FileLine {
                Ip = "127.0.0.1",
                Domain = "lokalhost",
                Commentar = "Hallo "
            });
            Assert.AreEqual("127.0.0.1 lokalhost # Hallo", line);
            Console.WriteLine(line);

            line = HostsFile.CreateTextLine(new FileLine {
                IsDisabled = true,
                Ip = "127.0.0.1",
                Domain = "lokalhost",
                Commentar = "Hallo "
            });
            Assert.AreEqual("# 127.0.0.1 lokalhost # Hallo", line);
            Console.WriteLine(line);

            line = HostsFile.CreateTextLine(new FileLine {
                IsCommentarLine = true,
                IsDisabled = true,
                Ip = "127.0.0.1",
                Domain = "lokalhost",
                Commentar = "Hallo "
            });
            Assert.AreEqual("# Hallo", line);
            Console.WriteLine(line);
        }
    }
}