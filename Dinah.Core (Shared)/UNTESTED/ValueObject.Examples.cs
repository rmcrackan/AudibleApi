using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dinah.Core
{
    // implementation
    internal class Address : ValueObject_Static<Address>
    {
        public string Street { get; }
        public string City { get; }
        public string ZipCode { get; }

        public Address(string street, string city, string zipCode)
        {
            Street = street;
            City = city;
            ZipCode = zipCode;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return ZipCode;
        }
    }

    // implementation with collection
    internal class AddressAndTenants : ValueObject_Static<Address>
    {
        public string Street { get; }
        public string City { get; }
        public string ZipCode { get; }
        public List<Tenant> Tenants { get; }

        public AddressAndTenants(string street, string city, string zipCode, List<Tenant> tenants)
        {
            Street = street;
            City = city;
            ZipCode = zipCode;
            Tenants = tenants;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Street;
            yield return City;
            yield return ZipCode;

            foreach (Tenant tenant in Tenants)
                yield return tenant;
        }
    }
    internal class Tenant : ValueObject_Static<Tenant>
    {
        public string Name { get; }
        public Tenant(string name) => Name = name;
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
        }
    }
}
