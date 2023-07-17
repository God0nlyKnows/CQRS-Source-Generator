using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using TestConsoleApp.Domain.Common.Models;

namespace TestConsoleApp.Domain.Common.ValueObjects
{

    public sealed class Address : ValueObject
    {
        private Address(string street, string postCode, string city)
        {
            Street = street;
            PostCode = postCode;
            City = city;
        }


        public static Address Create(string street, string postCode, string city)
        {
            return new Address(street, postCode, city);
        }

        public string Street { get; }

        public string PostCode { get; }

        public string City { get; }

        public override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return PostCode;
            yield return City;
        }

        private Address() { }
    }
}
