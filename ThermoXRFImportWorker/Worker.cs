using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ThermoXRFImportWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<XrfDataModel> options;
        private readonly FileSystemWatcher _fileSystemWatcher;
        //private string lastFileProcessed = string.Empty;
        private TcpClient client;
        private uint updateId = 0;
        //private readonly Dictionary<string, ITagInfo> oxideValues = new Dictionary<string, ITagInfo>();


        public Worker(ILogger<Worker> logger, IOptions<XrfDataModel> options, FileSystemWatcher fileSystemWatcher, TcpClient tcpClient)
        {
            _logger = logger;
            this.options = options;
            _fileSystemWatcher = fileSystemWatcher;
            client = tcpClient;
            InitializeCommunications();
        }

        private void InitializeCommunications()
        {
            try
            {
                client.Connect(options.Value.DatapoolIp, options.Value.Port);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error attempting to connect to datapool.  {1}", ex.Message);
            }
            
            if (client.Connected)
            {
                foreach (var oxide in options.Value.Oxides)
                {
                    SendToDatapool($"CreateTag [<{options.Value.DpGroup}><{oxide}><Double>]");
                }
                SendToDatapool($"CreateTag [<{options.Value.Update}><Update><Int>]");
            }
            //else
            //{
            //    DatapoolSvr.IpAddress = options.Value.DatapoolIp;

            //    foreach (var oxide in options.Value.Oxides)
            //    {
            //        oxideValues.Add(oxide, DatapoolSvr.CreateTagInfo(options.Value.DpGroup, oxide, dpTypes.FLOAT));
            //    }
            //    updateTag = DatapoolSvr.CreateTagInfo(options.Value.Update, "Update", dpTypes.INT);
            //}
        }

        public override async Task StartAsync(CancellationToken token)
        {
            _fileSystemWatcher.Created += FileSystemWatcher_Created;
            _fileSystemWatcher.Changed += FileSystemWatcher_Changed;
            _fileSystemWatcher.Path = options.Value.Path;
            _fileSystemWatcher.EnableRaisingEvents = true;
            foreach (var extension in options.Value.Extensions)
            {
                _fileSystemWatcher.Filters.Add(extension);
            }           
            _logger.LogInformation($"Service started: {DateTime.Now}");
            //_logger.LogInformation("Data from appsettings: Path = {0}, DatapoolIp = {1}, DpGroup = {2}, Update = {3}, SamplePeriod = {4}",
            //    _configuration["Data:Path"], _configuration["Data:DatapoolIp"], _configuration["Data:DpGroup"],
            //    _configuration["Data:Update"], _configuration["Data:SamplePeriod"]);
        }
                
        public override async Task StopAsync(CancellationToken token)
        {
            _fileSystemWatcher.Created -= FileSystemWatcher_Created;
            _fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
            //foreach (var oxide in oxideValues)
            //{
            //    oxide.Value.Dispose();
            //}
            //updateTag.Dispose();
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
                    retryCount++;
                    if (retryCount % 5 == 0) { _logger.LogError("File Read Error: {0}", ex.Message); }  // log every 5th attempt
                }
                if (retryCount >= 5)
                {
                    _logger.LogError("File read failure on file {0}", filename);
                    return;
                }
            }   // Retries the file read 100 times then exits if failure            
            
            using (var process = new Process())
            {
                process.StartInfo.FileName = @"rexx";
                process.StartInfo.Arguments = $"{options.Value.Path}ParseAssayFile.rex {assay}";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.OutputDataReceived += (sender, data) =>
                { ProcessData(data.Data); };
                process.ErrorDataReceived += (sender, data) =>
                {
                    if (!string.IsNullOrEmpty(data.Data))
                        _logger.LogWarning("error results: {data.Data}", data.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                var exited = process.WaitForExit(1000 * 10);
                _logger.LogInformation("exited: {process.ExitCode}", process.ExitCode);
            }
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
            }
            else
            {
            _logger.LogInformation(data);
            }
            SendToDatapool(data);
        }

        private void SendToDatapool(string data, bool useTcpMessaging)
        {
            if (string.IsNullOrEmpty(data))
            {
                SendToDatapool(FormatMessage(options.Value.Update, "Update", updateId.ToString()));
                return;
                
                //else
                //{
                //    updateTag.Write(updateId, DateTime.UtcNow, options.Value.SamplePeriod);
                //    return;
                //}
            }

            var info = data.Split(',');
            //_logger.LogInformation($"DpGroup: {_configuration["Data.DpGroup"]}");
            SendToDatapool(FormatMessage(options.Value.DpGroup, info[0], info[1]));
            return;
            
            //oxideValues[info[0]].Write(info[1], DateTime.UtcNow, options.Value.SamplePeriod);
        }

        private string FormatMessage(string groupname, string tagname, string value ) => 
            $"Write [<{groupname}><{tagname}><{value}><{DateTime.UtcNow.ToShortDateString()}><{DateTime.UtcNow.ToString("HH:mm:ss:FFF")}><{options.Value.SamplePeriod}>]";
        
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
            try
            {

            NetworkStream stream = client.GetStream();

            stream.Write(data, 0, data.Length);
            data = new byte[256];

            string responseData = string.Empty;

            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            return responseData;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error writing to datapool. {1}", ex.Message);
            }
            return string.Empty;
        }

    }
}
