using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace TestControlTool.UpdateService
{
    /// <summary>
    /// Responses for all copying files from one folder to another
    /// </summary>
    internal class CopyJob : IJob
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IEnumerable<KeyValuePair<string, string>> _folders;
 
        public CopyJob(IEnumerable<KeyValuePair<string, string>> folders)
        {
            _folders = folders;
        }

        /// <summary>
        /// Starts the job
        /// </summary>
        public void Run()
        {
            Parallel.ForEach(_folders, pair => CopyDirectory(pair.Key, pair.Value));
        }

        private static void CopyDirectory(string source, string target)
        {
            Logger.Info("Copying files from '" + source + "' to '" + target + "'...");

            if (!Directory.Exists(source))
            {
                Logger.Error("Folder '" + source + "' doesn't exists.");

                throw new DirectoryNotFoundException("Folder '" + source + "' doesn't exists.");
            }
            
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }
            

            var files = Directory.EnumerateFiles(source).Select(x => new FileInfo(x));
            var directories = Directory.EnumerateDirectories(source).Select(x => new DirectoryInfo(x));

            foreach (var file in files)
            {
                Logger.Info("Copying '" + file.FullName + "' to the '" + target + "'..");

                file.CopyTo(target + Path.AltDirectorySeparatorChar + file.Name, true);

                Logger.Info("'" + file.FullName + "' has been copied successfully");
            }

            foreach (var directory in directories)
            {
                CopyDirectory(directory.FullName, target + Path.AltDirectorySeparatorChar + directory.Name);
            }

            Logger.Info("Directory '" + source + "' has been successfully copied.");
        }
    }
}