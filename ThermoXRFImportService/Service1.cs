using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace ThermoXRFImportService
{
    public partial class Service1 : ServiceBase
    {
        
        private readonly FileSystemWatcher fileSystemWatcher = new FileSystemWatcher();
        

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            fileSystemWatcher.Created += FileSystemWatcher_Created;
            fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            fileSystemWatcher.Path = @"C:\XRF\";
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.Filter = "*.qan";
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnStop()
        {
            fileSystemWatcher.Created -= FileSystemWatcher_Created;
            fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
        }
    }
}
