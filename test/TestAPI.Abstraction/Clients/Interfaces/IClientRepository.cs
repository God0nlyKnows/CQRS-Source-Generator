using CQRS_Source_Generator.Attributes;
using TestConsoleApp.Abstraction.Clients.Requests;
using TestConsoleApp.Domain.Clients;
using TestConsoleApp.Domain.Clients.ValueObjects;

namespace TestConsoleApp.Clients.Interfaces
{

    public interface IClientRepository
    {
        [GenerateQuery<GetClientsReq>]
        Task<IEnumerable<Client>> GetClientsAsync();

        [GenerateQuery<GetClientReq>]
        Task<Client> GetClientAsync(ClientId id);

        Task<Client> AddClientAsync(Client client);

        Task<Client> UpdateClientAsync(Client client);

        Task DeleteClientAsync(ClientId id);
    }
}
