using System.Threading.Tasks;

namespace OpcClientWorker
{
    public interface IClient
    {
        Task AddMonitoredItem();
        void ConfigureClient();
        Task ConnectAsync();
        Task CreateDataSubscriptionAsync();
        Task DeleteDataSubscriptionAsync();
        Task DisconnectAsync();
        Task RemoveMonitoredItems();
        Task TestConnectionAsync();
        Task TestDisconnectionAsync();
    }
}