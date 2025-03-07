using FluentValidation;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Domain.Payments;
using Grand.Infrastructure.Validators;
using Grand.Web.API.Models;
using Grand.Web.API.Utils;
using Grand.Web.Models.Orders;

namespace Grand.Web.API.Validators
{
    public class WebHookOrderModelValidator : BaseGrandValidator<WebHookOrderModel>
    {
        public WebHookOrderModelValidator(IEnumerable<IValidatorConsumer<WebHookOrderModel>> validators, ITranslationService translationService) : base(validators)
        {
            RuleFor(x => x.Customer).NotNull().WithMessage(WebHookError.CreateOrder.Validation.CustomerNotNull);
            
            RuleFor(x => x.Customer.Email).NotEmpty().NotNull().WithMessage(WebHookError.CreateOrder.Validation.CustomerEmailNotNull);
            
            RuleFor(x => x.Address).NotEmpty().NotNull().WithMessage(WebHookError.CreateOrder.Validation.AddressNotNull);

            RuleFor(x => x.PaymentMethod).NotNull().NotEmpty().WithMessage(WebHookError.CreateOrder.Validation.PaymentMethodNotNull);

            RuleFor(x => x.PaymentStatusId).Must(status => Enum.IsDefined(typeof(PaymentStatus), status)).WithMessage(WebHookError.CreateOrder.Validation.PaymentStatusNotValid);

            RuleFor(x => x.OrderItems).NotEmpty().NotNull().WithMessage(WebHookError.CreateOrder.Validation.OrderItemsNotNull)
            .ForEach(item => item
             .NotNull()
                .ChildRules(orderItem =>
                {
                    orderItem.RuleFor(x => x.Sku)
                        .NotEmpty().NotNull().WithMessage(WebHookError.CreateOrder.Validation.SkuNotNull);
                })
            );
        }
    }
}
