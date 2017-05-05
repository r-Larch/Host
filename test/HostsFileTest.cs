using System.Collections.Generic;
using System.IO;
using System.Linq;
using Larch.Host.Parser;
using LarchConsole;
using NUnit.Framework;


namespace Larch.Host.Test {
    [TestFixture]
    public class HostsFileTest {
        public string MyFile = Path.GetTempFileName();
        public const int REPEAT = 1000;

        [OneTimeSetUp]
        public void Setup() {
            using (var fs = new FileStream(MyFile, FileMode.Create, FileAccess.Write)) {
                using (var fw = new StreamWriter(fs)) {

                    for (int i = 0; i < REPEAT; i++) {
                        fw.WriteLine();
                        fw.WriteLine("127.0.0.1 validation.sls.microsoft.com");
                        fw.WriteLine("#<localhost>");
                        fw.WriteLine("      "); // \t\t\t
                        fw.WriteLine("127.0.0.1	localhost");
                        fw.WriteLine("    \t "); // '   \t '
                        fw.WriteLine("127.0.0.1	localhost.localdomain");
                        fw.WriteLine("::1		localhost");
                        fw.WriteLine("#fe80::1%lo0	localhost");
                        fw.WriteLine("fe80::1%lo0	localhost");
                        fw.WriteLine("#127.0.0.1	localhost");
                        fw.WriteLine("# 127.0.0.1	localhost");
                        fw.WriteLine(" # 127.0.0.1	localhost");
                        fw.WriteLine(" # 127.0.0.1	localhost # commentar");
                        fw.WriteLine("127.0.0.1	localhost # commentar");
                        fw.WriteLine("# For example, to block unpleasant pages, try:"); // could be false positiv Ipv6
                    }
                }
            }
        }

        [Test]
        public void TestRead() {
            var hostsfile = new HostsFile(MyFile);
            List<HostsFileLine> hosts;
            using (new Watch("GetHosts")) {
                 hosts = hostsfile.GetHosts().ToList();
            }

            // print
            Watch.PrintTasks();
            var table = new Table();
            table.Create(1, 1, "Hosts", hosts);

            // validate
            Assert.IsNotEmpty(hosts);
            Assert.AreEqual(11 * REPEAT, hosts.Count, "Count all");
            Assert.AreEqual(0 * REPEAT, hosts.Count(x => x.IsCommentarLine), "CommentarLine count");
            Assert.AreEqual(5 * REPEAT, hosts.Count(x => x.IsDisabled), "Disabled count");
            Assert.AreEqual(3 * REPEAT, hosts.Count(x => !string.IsNullOrEmpty(x.Ipv6)), "Ipv6 count");
            Assert.AreEqual(2 * REPEAT, hosts.Count(x => !string.IsNullOrEmpty(x.Commentar)), "Commentar count");
        }


        [OneTimeTearDown]
        public void Clean() {
            File.Delete(MyFile);
        }
    }
}