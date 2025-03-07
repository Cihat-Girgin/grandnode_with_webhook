using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Grand.Web.API.Attributes
{
    public class CreateOrderAuthorizeAttribute  : Attribute, IAuthorizationFilter
    {
        private const string ApiKeyHeaderName = "X-Api-Key";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var config = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            var apiKey = config?.GetValue<string>("Application:ApiSettings:ApiKey");

            var requestApiKey = context.HttpContext.Request.Headers[ApiKeyHeaderName];

            if (string.IsNullOrEmpty(requestApiKey) || requestApiKey != apiKey)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
