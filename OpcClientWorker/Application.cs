using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpcDatapoolLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpcClientWorker
{
    public class Application : IApplication
    {
        private readonly IServiceProvider _services;
        private List<IClient> clients = new List<IClient>();
        private Dictionary<uint, OpcDatapoolModel> dataTags = new Dictionary<uint, OpcDatapoolModel>();
        private IConfiguration _configuration;
        private readonly DatapoolService datapoolService;

        public Application(IServiceProvider services, IConfiguration configuration, DatapoolService datapoolService)
        {
            _configuration = configuration;
            this.datapoolService = datapoolService;
            _services = services;
        }
        public async Task AddClientAsync()
        {            
            var temp = _services.GetService<IClient>();                        
            await temp.TestConnectionAsync();
            clients.Add(temp);
            AddDatapool();
        }

        public void AddDatapool()
        {
            datapoolService.Connect(_configuration["Datapool:IpAddress"]);
            datapoolService.Datapool.ConnectionChange += Datapool_ConnectionChange;
            datapoolService.Datapool.Create("testgroup", "testTag", Thermo.Datapool.Datapool.dpTypes.FLOAT);
            AddTestData();
        }

        private void Datapool_ConnectionChange(bool isConnected)
        {
            
        }

        public async Task RemoveClientAsync()
        {
            if (clients.Count > 0)
            {
                await clients[0].TestDisconnectionAsync();
                clients.RemoveAt(0);
            }
        }

        public void AddTestData()
        {
            var data = datapoolService.Datapool.CreateTagInfo("testGroup", "testTag", Thermo.Datapool.Datapool.dpTypes.FLOAT);
            var NodeId = Workstation.ServiceModel.Ua.NodeId.Parse("ns=2;s=Local.test.newTag:F");
            dataTags.Add(0, new OpcDatapoolModel { Datapool = datapoolService, TagInfo = datapoolService.Datapool.CreateTagInfo("testGroup", "testTag", Thermo.Datapool.Datapool.dpTypes.FLOAT), NodeId = Workstation.ServiceModel.Ua.NodeId.Parse("ns=2;s=Local.test.newTag:F") });
        }
    }
}
