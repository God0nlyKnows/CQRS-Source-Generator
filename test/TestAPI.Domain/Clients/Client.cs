using System;
using System.Collections.Generic;
using System.Text;
using TestConsoleApp.Domain.Clients;
using TestConsoleApp.Domain.Clients.ValueObjects;
using TestConsoleApp.Domain.Common.Models;
using TestConsoleApp.Domain.Common.ValueObjects;

namespace TestConsoleApp.Domain.Clients
{
    public sealed class Client : AggregateRoot<ClientId>
    {


        public static Client Create(ClientId clientId, string name, string surname, string email, string? peselOrNip, string? description, Address? address)
        {
            return new Client(clientId, name, surname, email, peselOrNip, description, address);
        }

        private Client(ClientId id, string name, string surname, string email, string? peselOrNip, string? description, Address? address) : base(id)
        {
            Name = name;
            Surname = surname;
            Email = email;
            PeselOrNip = peselOrNip;
            Description = description;
            Address = address;
        }


        public string Name { get; set; }
        public string Surname { get; set; }

        public string Email { get; set; }

        public string? PeselOrNip { get; set; }

        public string? Description { get; set; }

        public Address? Address { get; set; } = null!;
        private Client() { }


    }
}
