using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Connector.CodatTypes
{
    internal class Phone
    {
        public string Number { get; init; }
        public string Type { get; init; }
    }

    internal class Contact
    {
        public string Name { get; init; }
        public List<Phone> Phone { get; init; }
        public string Status { get; init; }
        public string Email { get; init; }
    }

    internal class Customer
    {
        public string Id { get; init; }
        public string CustomerName { get; init; }
        public string DefaultCurrency { get; init; }
        public string Phone { get; init; }
        public List<Address> Addresses { get; init; }
        public List<Contact> Contacts { get; init; }
        public string Status { get; init; }
        public DateTime ModifiedDate { get; init; }
        public DateTime SourceModifiedDate { get; init; }
        public string ContactName { get; init; }
        public string RegistrationNumber { get; init; }
        public string EmailAddress { get; init; }
    }

    internal class CustomersContainer : IPaginated<Customer>
    {
        public List<Customer> Results { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalResults { get; init; }
        
        [JsonProperty("_links")]
        public Links Links { get; init; }
    }
}