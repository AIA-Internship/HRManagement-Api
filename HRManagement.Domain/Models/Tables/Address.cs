namespace HRManagement.Domain.Models.Tables;

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

    public Address() { }

    public Address(string street, string city, string province, string zipCode)
    {
        Street = street;
        City = city;
        Province = province;
        ZipCode = zipCode;
    }
}
