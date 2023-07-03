using CQRS_Source_Generator.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestConsoleApp.Domain.Clients;
using TestConsoleApp.Domain.Clients.ValueObjects;

namespace TestConsoleApp.Clients.Interfaces
{
    [GenerateQuery]
    public interface IClientRepository
    {
        Task<IEnumerable<Client>> GetClientsAsync();
        Task<Client> GetClientAsync(ClientId id);
        Task<Client> AddClientAsync(Client client);
        Task<Client> UpdateClientAsync(Client client);
        Task DeleteClientAsync(int id);
    }
}
