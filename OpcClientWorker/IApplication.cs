using System.Threading.Tasks;

namespace OpcClientWorker
{
    public interface IApplication
    {
        Task AddClientAsync();
        Task RemoveClientAsync();
    }
}