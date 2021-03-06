using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ThermoXRFImportWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly FileSystemWatcher _fileSystemWatcher;
        //private string lastFileProcessed = string.Empty;
        private readonly Int32 port = 8111;
        private TcpClient client;
        private uint updateId = 0;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, FileSystemWatcher fileSystemWatcher, TcpClient tcpClient)
        {
            _logger = logger;
            _configuration = configuration;
            _fileSystemWatcher = fileSystemWatcher;
            client = tcpClient;
            client.Connect(_configuration["Data:DatapoolIp"], port);
        }
        
        public override async Task StartAsync(CancellationToken token)
        {
            _fileSystemWatcher.Created += FileSystemWatcher_Created;
            _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            _fileSystemWatcher.Path = _configuration["Data:Path"];
            _fileSystemWatcher.EnableRaisingEvents = true;
            _fileSystemWatcher.Filters.Add("*.qan");
            _fileSystemWatcher.Filters.Add("*.QAN");
            _fileSystemWatcher.Filters.Add("*.txt");
            _fileSystemWatcher.Filters.Add("*.TXT");
            _fileSystemWatcher.Filters.Add("*.CSV");
            _fileSystemWatcher.Filters.Add("*.csv");
            _logger.LogInformation($"Service started: {DateTime.Now}");
            //_logger.LogInformation("Data from appsettings: Path = {0}, DatapoolIp = {1}, DpGroup = {2}, Update = {3}, SamplePeriod = {4}",
            //    _configuration["Data:Path"], _configuration["Data:DatapoolIp"], _configuration["Data:DpGroup"],
            //    _configuration["Data:Update"], _configuration["Data:SamplePeriod"]);
        }

        public override async Task StopAsync(CancellationToken token)
        {
            _fileSystemWatcher.Created -= FileSystemWatcher_Created;
            _fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
            _logger.LogInformation($"Service stopped: {DateTime.Now}");
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                RunScript(e.FullPath);
            }
            finally
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
            
        }
        private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                _fileSystemWatcher.EnableRaisingEvents = false;
                RunScript(e.FullPath);
            }
            finally
            {
                _fileSystemWatcher.EnableRaisingEvents = true;
            }

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

            }
        }

        private void RunScript(string filename)
        {
            bool fileNotProcessed = true;
            uint retryCount = 0;
            string assay = string.Empty;
            while (fileNotProcessed)
            {
                try
                {
                    assay = File.ReadAllText(filename);
                    fileNotProcessed = false;

                }
                catch (Exception ex)
                {
                    //if (retryCount % 5 == 5) { _logger.LogError(ex.Message); }  // log every 5th attempt
                    retryCount++;                    
                }
                if (retryCount >= 5)
                {
                    _logger.LogError($"File read failure on file {filename}");
                    return;
                }
            }   // Retries the file read 100 times then exits if failure            
            
            using (var process = new Process())
            {
                process.StartInfo.FileName = @"rexx";
                process.StartInfo.Arguments = $"{_configuration["Data:Path"]}ParseAssayFile.rex {assay}";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += (sender, data) =>
                { ProcessData(data.Data); };
                process.ErrorDataReceived += (sender, data) =>
                {
                    if (!string.IsNullOrEmpty(data.Data))
                        _logger.LogWarning($"error results: {data.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                var exited = process.WaitForExit(1000 * 10);
                _logger.LogInformation($"exited: {process.ExitCode}");
            }

            //Random rand = new Random();
            //var value = (rand.NextDouble() * 23).ToString();
            //string test = await SendToDatapoolAsync(FormatMessage("test", "abcd", value));
            return;
        }
        /// <summary>
        /// Processes the data from the xrf sample file.  File returns oxideName, oxideValue
        /// </summary>
        /// <param name="data">comma separated string oxideName, oxideValue</param>
        /// <returns></returns>
        private void ProcessData(string data)  
        {
            if (string.IsNullOrEmpty(data)) 
            { 
                updateId++; 
                SendToDatapool(FormatMessage(_configuration["Data:Update"], "Update", updateId.ToString())); 
                return; 
            }
            _logger.LogInformation(data);
            var info = data.Split(',');
            //_logger.LogInformation($"DpGroup: {_configuration["Data.DpGroup"]}");
            SendToDatapool(FormatMessage(_configuration["Data:DpGroup"], info[0], info[1]));
        }

        private string FormatMessage(string groupname, string tagname, string value ) => 
            $"Write [<{groupname}><{tagname}><{value}><{DateTime.UtcNow.ToShortDateString()}><{DateTime.UtcNow.ToString("HH:mm:ss:FFF")}><{_configuration["Data:SamplePeriod"]}>]";
        
        /// <summary>
        /// Encodes and sends the message to the datapool via tcp
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private string SendToDatapool(string msg)
        {
            //_logger.LogInformation($"Message to Datapool: {msg}");
            Encoding unicode = Encoding.Unicode;
            Byte[] data;
            data = unicode.GetBytes(msg);
            NetworkStream stream = client.GetStream();

            stream.Write(data, 0, data.Length);
            data = new byte[256];

            string responseData = string.Empty;

            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            return responseData;
        }

    }
}
