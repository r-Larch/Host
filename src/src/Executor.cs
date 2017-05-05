using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Win32;


namespace Larch.Host {
    public class Executor {
        private static readonly string Program64 = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
        private static readonly string Program32 = Environment.ExpandEnvironmentVariables("%programfiles(x86)%");

        public static ProcessRunner OpenEditor(FileInfo fileInfo) {
            var editor = GetEnvVar("%EDITOR%");
            if (string.IsNullOrEmpty(editor)) {
                var progDir = FindSubDir(Program64, "Sublime*") ?? FindSubDir(Program32, "Sublime*");
                if (progDir != null) {
                    editor = FindSubFileExe(progDir, "sublime*.exe") ?? FindSubFileExe(Path.Combine(progDir, "bin"), "sublime-*.exe");
                }
            }

            if (string.IsNullOrEmpty(editor) || !File.Exists(editor)) {
                editor = "notepad.exe";
            }

            var process = new Process {
                StartInfo = new ProcessStartInfo() {
                    FileName = editor,
                    Arguments = "\"" + fileInfo.FullName + "\""
                }
            };
            return new ProcessRunner(process, fileInfo);
        }

        public static ProcessRunner OpenWithGimp(FileInfo fileInfo) {
            var gimpProgDir = FindSubDir(Program64, "Gimp*") ?? FindSubDir(Program32, "Gimp*");
            if (gimpProgDir == null) return null;

            var gimpProgPath = FindSubFileExe(gimpProgDir, "gimp-*.exe") ?? FindSubFileExe(Path.Combine(gimpProgDir, "bin"), "gimp-*.exe");
            if (gimpProgPath == null) return null;

            var process = new Process {
                StartInfo = new ProcessStartInfo() {
                    FileName = gimpProgPath,
                    Arguments = "\"" + fileInfo.FullName + "\""
                }
            };
            return new ProcessRunner(process, fileInfo);
        }

        public static ProcessRunner OpenWithGallery(FileInfo fileInfo) {
            var process = new Process {
                StartInfo = new ProcessStartInfo() {
                    FileName = fileInfo.FullName
                }
            };
            return new ProcessRunner(process, fileInfo);
        }

        public static ProcessRunner OpenWithPhotoshop(FileInfo fileInfo) {
            var photoshop = Registry.LocalMachine?.OpenSubKey("SOFTWARE")?.OpenSubKey("Adobe")?.OpenSubKey("Photoshop") ??
                            Registry.LocalMachine?.OpenSubKey("SOFTWARE")?.OpenSubKey("Wow6432Node")?.OpenSubKey("Adobe")?.OpenSubKey("Photoshop");

            if (photoshop == null) return null;

            var versions = photoshop.GetSubKeyNames().Select(x => photoshop.OpenSubKey(x));
            var path = versions.Select(x => (string) x.GetValue("ApplicationPath")).FirstOrDefault(x => x != null);

            if (path == null) return null;

            if (path.EndsWith(@"App\Photoshop64\")) {
                path = path.Substring(0, path.Length - @"App\Photoshop64\".Length);
            }

            var progpath = Directory.GetFiles(path, "Photoshop*.exe").FirstOrDefault();

            if (progpath == null) return null;

            var process = new Process {
                StartInfo = new ProcessStartInfo() {
                    FileName = progpath,
                    Arguments = "\"" + fileInfo.FullName + "\""
                }
            };
            return new ProcessRunner(process, fileInfo);
        }

        private static string FindSubDir(string dir, string name) {
            return Directory.GetDirectories(dir, name).FirstOrDefault();
        }

        private static string FindSubFileExe(string dir, string name) {
            return Directory.GetFiles(dir, name).FirstOrDefault();
        }

        private static string GetEnvVar(string var) {
            var res = Environment.ExpandEnvironmentVariables(var);
            return res != var ? res : null;
        }
    }


    public class ProcessRunner {
        private readonly Process _process;
        private readonly FileInfo _fileInfo;
        private static readonly List<FileWatcher> FileWatcher = new List<FileWatcher>();

        public ProcessRunner(Process process) {
            _process = process;
        }

        public ProcessRunner(Process process, FileInfo fileInfo) {
            _process = process;
            _fileInfo = fileInfo;
        }

        public void StartNormal() {
            _process.Start();
        }

        public IDisposable StartAndWatchFile(Action<FileInfo> onFileChanged) {
            if (_fileInfo == null) throw new ArgumentNullException(nameof(_fileInfo));

            var watcher = FileWatcher.FirstOrDefault(x => x.FileInfo.FullName == _fileInfo.FullName);
            if (watcher != null) {
                _process.Start();
                return watcher;
            }

            var fileWatcher = new FileWatcher(_fileInfo, onFileChanged);
            FileWatcher.Add(fileWatcher);
            _process.Start();

            return fileWatcher;
        }

        public Process StartAndWriteOutputTo(Action<string> onOutputDataReceived) {
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.CreateNoWindow = true;
            _process.OutputDataReceived += (o, args) => onOutputDataReceived(args.Data);
            _process.Start();
            _process.BeginOutputReadLine();
            return _process;
        }
    }


    public class FileWatcher : IDisposable {
        public readonly FileInfo FileInfo;
        private readonly Action<FileInfo> _onChanged;
        private BackgroundWorker _worker;
        private DateTime _lastModified;

        public FileWatcher(FileInfo fileInfo, Action<FileInfo> onChanged) {
            FileInfo = fileInfo;
            _onChanged = onChanged;
            _lastModified = fileInfo.LastWriteTime;

            _worker = new BackgroundWorker() {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = true
            };
            _worker.DoWork += FileWatchWorker;
            _worker.RunWorkerAsync();
        }

        private void FileWatchWorker(object sender, DoWorkEventArgs doWorkEventArgs) {
            var worker = sender as BackgroundWorker;
            if (worker == null) return;

            while (true) {
                if (worker.CancellationPending) return;

                var lastModified = File.GetLastWriteTime(FileInfo.FullName);
                if (lastModified > _lastModified) {
                    _lastModified = lastModified;
                    _onChanged(FileInfo);
                }

                Thread.Sleep(500);
            }
        }

        public void Dispose() {
            _worker?.CancelAsync();
            _worker?.Dispose();
            _worker = null;
        }
    }
}