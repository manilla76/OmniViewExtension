using System;
using System.Collections.Generic;
using System.Text;

namespace OpcClientWorker
{
    public class ClientOptions
    {
        public string Endpoint { get; set; }
        public string Tag { get; set; }
        public double RequestedPublishingInterval { get; set; }
        public uint RequestedMaxKeepAliveCount { get; set; }
        public uint RequestedLifetimeCount { get; set; }
        public bool PublishingEnabled { get; set; }
        public string ApplicationName { get; set; }

    }
}
