using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace OpcClientWorker
{
    public class Client : IClient
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ClientOptions _clientOptions;
        private ApplicationDescription clientDescription;
        private DirectoryStore certificateStore;
        private UaTcpSessionChannel channel;
        private List<uint> subscriptions = new List<uint>();
        private List<uint> monitoredItems = new List<uint>();
        private List<IDisposable> tokens = new List<IDisposable>();
        
        public Client(ILoggerFactory loggerFactory, ClientOptions clientOptions)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger("Client");
            _clientOptions = clientOptions;
            ConfigureClient();
        }

        public void ConfigureClient()
        {
            clientDescription = new ApplicationDescription
            {
                ApplicationName = _clientOptions.ApplicationName,
                ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:{_clientOptions.ApplicationName}",
                ApplicationType = ApplicationType.Client
            };
            certificateStore = new DirectoryStore("./pki");
            channel = new UaTcpSessionChannel(
                clientDescription,
                null,
                new AnonymousIdentity(),
                _clientOptions.Endpoint,    //server endpoint
                SecurityPolicyUris.None,
                _loggerFactory
                );
        }
        public async Task CreateDataSubscriptionAsync()
        {
            try
            {
                var subscriptionRequest = new CreateSubscriptionRequest
                {
                    RequestedPublishingInterval =_clientOptions.RequestedPublishingInterval,
                    RequestedMaxKeepAliveCount = _clientOptions.RequestedMaxKeepAliveCount,
                    RequestedLifetimeCount = _clientOptions.RequestedLifetimeCount,
                    PublishingEnabled = _clientOptions.PublishingEnabled
                };

                var subscriptionResponse = await channel.CreateSubscriptionAsync(subscriptionRequest);
                var id = subscriptionResponse.SubscriptionId;
                subscriptions.Add(id);
                _logger.LogInformation($"Created subscription '{id}' at {DateTime.Now}.");
            }
            catch (Exception ex)
            {
                await channel.AbortAsync();
                _logger.LogError(ex.Message);
            }
        }
        public async Task DeleteDataSubscriptionAsync()
        {
            var deleteSubRequest = new DeleteSubscriptionsRequest { SubscriptionIds = subscriptions.ToArray() };
            var deleteSubResponse = await channel.DeleteSubscriptionsAsync(deleteSubRequest);
            _logger.LogInformation($"Deleted Subscriptions '{subscriptions[0]}' at {DateTime.Now}.");
            subscriptions.RemoveAt(0);            
        }
        public async Task AddMonitoredItem()
        {
            var createItemsRequest = new CreateMonitoredItemsRequest
            {
                SubscriptionId = subscriptions[0],                    //  Subscription to add to.  May need to choose the subscription to add to if multiple subs per client
                TimestampsToReturn = TimestampsToReturn.Both,
                ItemsToCreate = new MonitoredItemCreateRequest[]
                {
                        new MonitoredItemCreateRequest
                        {
                            ItemToMonitor = new ReadValueId{AttributeId=AttributeIds.Value, NodeId = NodeId.Parse(_clientOptions.Tag)},
                            MonitoringMode = MonitoringMode.Reporting,
                            RequestedParameters = new MonitoringParameters{ClientHandle = 42, QueueSize = 2, DiscardOldest = true, SamplingInterval=1000},
                        },
                },

            };
            var createItemResponse = await channel.CreateMonitoredItemsAsync(createItemsRequest);
            monitoredItems.Add(createItemResponse.Results[0].MonitoredItemId);
            _logger.LogInformation($"Monitored Item added: {monitoredItems[0]}");
            tokens.Add(channel
                    .Where(pr => pr.SubscriptionId == subscriptions[0])     //  Subscription to add to.  May need to choose the subscription to add to if multiple subs per client
                    .Subscribe(
                        pr =>
                        {
                            var dcns = pr.NotificationMessage.NotificationData.OfType<DataChangeNotification>();
                            foreach (var dcn in dcns)
                            {
                                foreach (var min in dcn.MonitoredItems)
                                {                                   
                                    _logger.LogInformation($"sub: {pr.SubscriptionId}; handle: {min.ClientHandle}; value: {min.Value}");
                                }
                            }
                        },
                        ex => _logger.LogError($"Exception in publish response handler: {ex.GetBaseException().Message}")
                        ));
            
        }
        public async Task RemoveMonitoredItems()
        {
            tokens[0].Dispose();
            tokens.RemoveAt(0);
            var removeMonitoredItemsRequest = new DeleteMonitoredItemsRequest { SubscriptionId = subscriptions[0], MonitoredItemIds = monitoredItems.ToArray() };
            var removeMonitoredItemsResponse = await channel.DeleteMonitoredItemsAsync(removeMonitoredItemsRequest);
            monitoredItems.RemoveAt(0);
            _logger.LogInformation($"Monitored Items Removed.");
        }
        public async Task ConnectAsync()
        {
            try
            {
                await channel.OpenAsync();

                _logger.LogInformation($"Opened session with endpoint '{channel.RemoteEndpoint.EndpointUrl}'.");
                _logger.LogInformation($"SecurityPolicy: '{channel.RemoteEndpoint.SecurityPolicyUri}'.");
                _logger.LogInformation($"SecurityMode: '{channel.RemoteEndpoint.SecurityMode}'.");
                _logger.LogInformation($"UserIdentityToken: '{channel.UserIdentity}'.");
            }
            catch (Exception ex)
            {
                await channel.AbortAsync();
                _logger.LogError(ex.Message);
            }
        }
        public async Task DisconnectAsync()
        {
            if (channel == null || channel.State == CommunicationState.Closed) { return; }   // if nothing connected, return
            await Task.Delay(3000);
            await channel.CloseAsync();
            _logger.LogInformation($"Close connection to '{channel.SessionId}' at {DateTime.Now}.");
        }
        public async Task TestConnectionAsync()
        {
            await ConnectAsync();
            await CreateDataSubscriptionAsync();
            await AddMonitoredItem();
        }
        public async Task TestDisconnectionAsync()
        {
            await RemoveMonitoredItems();
            await DeleteDataSubscriptionAsync();
            await DisconnectAsync();
            _clientOptions.RequestedLifetimeCount = 500;
        }

    }
}
