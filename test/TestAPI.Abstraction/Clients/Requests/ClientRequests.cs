using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestConsoleApp.Domain.Clients;
using TestConsoleApp.Domain.Clients.ValueObjects;

namespace TestConsoleApp.Abstraction.Clients.Requests
{

        public record GetClientsReq() : IRequest<IEnumerable<Client>>;

        public record GetClientReq(ClientId Id) : IRequest<Client>;
}
