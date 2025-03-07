using Grand.Domain.Customers;

namespace Grand.Web.API.Models
{
    public class CustomerInfo
    {
        public bool IsNewCustomer { get; set; }
        public Customer Customer { get; set; }
    }
}
