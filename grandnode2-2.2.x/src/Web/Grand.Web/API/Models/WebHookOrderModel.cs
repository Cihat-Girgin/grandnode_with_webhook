namespace Grand.Web.API.Models
{
    public record WebHookOrderModel(
        string IdempotencyKey,
        string StoreName,
        int PaymentStatusId,
        string PaymentMethod,
        WebHookCustomerModel Customer,
        WebHookCustomerAddressModel Address,
        IReadOnlyList<WebHookOrderItemModel> OrderItems
    );

    public record WebHookCustomerModel(
        string Email,
        string FirstName,
        string LastName
    );

    public record WebHookCustomerAddressModel(
        string Country,
        string City,
        string Address
        );

    public record WebHookOrderItemModel(
        string Sku,
        double UnitPrice,
        int Quantity
    );
}
