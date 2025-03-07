using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Stores;
using Grand.Web.API.Attributes;
using Grand.Web.API.Exceptions;
using Grand.Web.API.Models;
using Grand.Web.API.Utils;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Polly;
using Polly.Retry;
using Serilog;
using System.Net;

namespace Grand.Web.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebHookController : ControllerBase
    {
        #region Fields
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IStoreService _storeService;
        private Store _store;
        #endregion

        #region Constructors
        public WebHookController(ICustomerService customerService, IProductService productService, IOrderService orderService, IStoreService storeService)
        {
            _customerService = customerService;
            _productService = productService;
            _orderService = orderService;
            _storeService = storeService;

            _retryPolicy = Policy
            .Handle<WebHookCreateOrderException>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), (result, timeSpan, retryCount, context) =>
            {
                Log.Error($"{WebHookError.CreateOrder.OrderCouldNotBeCreated}\nRetry Count:{retryCount}");

            });
        }
        #endregion

        #region Endpoints
        [HttpPost]
        [CreateOrderAuthorize]
        public async Task<IActionResult> CreateOrder(WebHookOrderModel order)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                return await CreateOrderProcess(order);
            });
        }
        #endregion

        #region Create Order Helper Methods
        [NonAction]
        private async Task<IActionResult> CreateOrderProcess(WebHookOrderModel order)
        {
            //Idempotency Check
            var orderProcessed = await _orderService.GetOrderByIdempotencyKey(order.IdempotencyKey);

            if (orderProcessed is not null)
            {
                return Ok(orderProcessed.Id);
            }

            //Store Check
            _store = await GetStoreByName(order.StoreName);

            if (_store is null)
            {
                return BadRequest(WebHookError.CreateOrder.StoreNotFound);
            }

            //Order Items SKU Check
            var validateOrderItems = await ValidateOrderItems(order);

            if (validateOrderItems.IsValid is false)
            {
                return BadRequest(WebHookError.CreateOrder.ProductMap);
            }

            //Set Customer
            var customerInfo = await SetCustomer(order);

            var products = validateOrderItems.Products;

            try
            {
                Order createdOrder = await BuildOrder(order, customerInfo.Customer, products);

                return Ok(createdOrder.Id);
            }
            catch (Exception ex)
            {
                if (customerInfo.IsNewCustomer)
                {
                    await _customerService.DeleteCustomer(customerInfo.Customer);
                }

                Log.Error($"Key:{order.IdempotencyKey} {WebHookError.CreateOrder.OrderCouldNotBeCreated}: {ex.Message}\n{WebHookError.StackTrace}: {ex.StackTrace}");

                throw new WebHookCreateOrderException(WebHookError.CreateOrder.OrderCouldNotBeCreated, HttpStatusCode.InternalServerError);

            }
        }

        private async Task<Order> BuildOrder(WebHookOrderModel order, Customer customer, List<Product> products)
        {
            var orderEntity = new Order() {
                CustomerId = customer.Id,
                StoreId = _store.Id,
                IdempotancyKey = order.IdempotencyKey,
                CustomerEmail = customer.Email,
                PaymentMethodSystemName = order.PaymentMethod,
                PaymentStatusId = (PaymentStatus)order.PaymentStatusId,
                OrderTotal = order.OrderItems.Sum(oi => oi.UnitPrice * oi.Quantity)
            };

            orderEntity.SetOrderItems(BuildOrderItems(order.OrderItems.ToList(), products));

            orderEntity.BillingAddress = new Address() {
                FirstName = order.Customer.FirstName,
                LastName = order.Customer.LastName,
                Email = order.Customer.Email,
                City = order.Address.City,
                Address1 = order.Address.Address
            };

            var createdOrder = await _orderService.InsertWebHookOrder(orderEntity);

            return createdOrder;
        }

        private async Task<(bool IsValid, List<Product> Products)> ValidateOrderItems(WebHookOrderModel order)
        {
            var skuList = order.OrderItems.Select(oi => oi.Sku).ToArray();
            
            var products = await _productService.GetProductsBySkuList(skuList);

            var isValid = skuList.Length == products.Count;

            return (isValid, products);
        }
        
        private async Task<(bool IsNewCustomer, Customer Customer)> SetCustomer(WebHookOrderModel order)
        {
            var customer = await _customerService.GetCustomerByEmail(order.Customer.Email);
            bool isNewCustomer = false;

            if (customer is null)
            {
                var newCustomer = new Customer() {
                    Username = order.Customer.Email,
                    Email = order.Customer.Email,
                    SystemName = "WebHookUser",
                    UserFields = new List<UserField>() {
                        new UserField() {
                            Key = nameof(order.Customer.FirstName),
                            Value = order.Customer.FirstName
                        },
                        new UserField() {
                            Key = nameof(order.Customer.LastName),
                            Value = order.Customer.LastName
                        }
                    }

                };

                isNewCustomer = true;
                customer = await _customerService.InsertWebHookCustomer(newCustomer);
            }

            return (isNewCustomer, customer);
        }
        
        private ICollection<OrderItem> BuildOrderItems(List<WebHookOrderItemModel> orderItemModels, List<Product> products)
        {
            var responseModel = new List<OrderItem>();

            foreach (var itemModel in orderItemModels)
            {
                var orderItem = new OrderItem();
                var product = products.FirstOrDefault(p => p.Sku == itemModel.Sku);

                if (product is null)
                {
                    throw new WebHookCreateOrderException(WebHookError.CreateOrder.ProductMap, HttpStatusCode.InternalServerError);
                }

                orderItem.ProductId = product.Id;
                orderItem.Sku = itemModel.Sku;
                orderItem.Quantity = itemModel.Quantity;

                responseModel.Add(orderItem);
            }

            return responseModel;
        }
        
        private Task<Store> GetStoreByName(string storeName)
        {
            return _storeService.GetStoreByName(storeName);
        }
        #endregion
    }
}
