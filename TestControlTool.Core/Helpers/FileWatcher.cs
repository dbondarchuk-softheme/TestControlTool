using System;
using System.IO;
using System.Linq;

namespace TestControlTool.Core.Helpers
{
    /// <summary>
    /// Watching for file changes
    /// </summary>
    public class FileWatcher : IDisposable
    {
        private FileSystemWatcher _watcher;
        private long _lastSize = 0;

        public delegate void FileChandedDelegate(string file, string newText);

        /// <summary>
        /// Path to the file
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Emits, when some new text was added to the file
        /// </summary>
        public event FileChandedDelegate FileChanged;

        /// <summary>
        /// Creates new file watcher
        /// </summary>
        /// <param name="fileName">Path to the file</param>
        public FileWatcher(string fileName)
        {
            FileName = fileName;

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                _watcher = new FileSystemWatcher(FileName.Remove(FileName.LastIndexOf('\\')))
                {
                    Filter = FileName.Split('\\', '/').Last(),
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.LastAccess
                };
            }
            catch (Exception e)
            {
               return;
            }
            
            _watcher.Changed += (sender, args) =>
                {
                    if (args.ChangeType != WatcherChangeTypes.Deleted)
                    {
                        using (var streamReader = new StreamReader(new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                        {
                            streamReader.BaseStream.Seek(_lastSize, SeekOrigin.Begin);

                            var newText = streamReader.ReadToEnd().Replace("\0", "");

                            _lastSize = streamReader.BaseStream.Length;

                            OnFileChanged(FileName, newText);
                        }
                    }
                };

            _watcher.EnableRaisingEvents = true;
        }

        private void OnFileChanged(string file, string newText)
        {
            var handler = FileChanged;
            if (handler != null) handler(file, newText);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
            }
        }
    }
}
