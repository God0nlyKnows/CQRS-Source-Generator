using CQRS_Source_Generator.Attributes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestConsoleApp.Domain.Clients;
using TestConsoleApp.Domain.Clients.ValueObjects;

namespace TestConsoleApp.Clients.Interfaces
{
    public record GetClientsReq() : IRequest<IEnumerable<Client>>;
    public record GetClientReq(ClientId Id) : IRequest<Client>;


    public interface IClientRepository
    {
        [GenerateQuery<GetClientsReq>]
        Task<IEnumerable<Client>> GetClientsAsync();

        [GenerateQuery<GetClientReq>]
        Task<Client> GetClientAsync(ClientId id);

        Task<Client> AddClientAsync(Client client);

        Task<Client> UpdateClientAsync(Client client);

        Task DeleteClientAsync(int id);
    }
}
