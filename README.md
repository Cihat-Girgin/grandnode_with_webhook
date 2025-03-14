# Web Hook API

An endpoint that processes incoming orders from external systems for GrandNode.

- Receives order data from external systems.  
- Validates whether the order is valid (e.g., product SKU information, customer details, etc.).  
- Creates a new customer if the customer is not registered in the GrandNode database.  
- Processes and saves the order in the GrandNode database.  
- Returns appropriate error messages for invalid orders.  

## Installation  

1. Start **MongoDB** either in Docker or locally.  
2. Then, create a sample database for GrandNode.  
3. Launch the application and complete the **GrandNode** setup using the database details you created.  

## API Workflow  

The relevant endpoint is located within the **Grand.Web** project.  

It starts with the application and accepts **POST** requests at **https://localhost:44350/api/webhook**.  

To test the endpoint, you can use the **Postman** documentation (**web_hook_order_request.json**) under **Solution Items**.  

![image](https://github.com/user-attachments/assets/b9d39e3c-4bac-4ab9-80f3-5ec40b68e0ce)  
![image](https://github.com/user-attachments/assets/8e7193e0-c1d4-402f-aa73-ef4f702c9a2c)  

## Validation  

The fields in the request model sent to the endpoint are validated using **Fluent Validation** (**WebHookOrderModelValidator**).  

- Logical validations such as **SKU**, **Store**, and **Order Items** checks are performed within the endpoint.  

## Authorization  

Requests to the endpoint must include an **X-Api-Key** in the **headers** section. If this value is missing or incorrect, the endpoint will return **401 Unauthorized** (**CreateOrderAuthorizeAttribute**).  

## Retry Mechanism  

A simple retry mechanism has been implemented using the **Polly** library to handle scenarios where the endpoint encounters an error.  

- Only system errors are handled; client-side validation errors are not retried.  
- If a failure occurs during the **order creation** process, the system will retry up to 3 times at specific intervals.  
- If an error still occurs after these retries, the request will be terminated.  

### Alternative Comprehensive Solution  

A more comprehensive, manageable, and traceable retry mechanism can be implemented using **Queue** and **BackgroundJob** (**Queue-Based Retry**).  

## Logging  

If a system error occurs within the endpoint, it is logged using **Serilog** under the **Logs** folder inside **AppData**.  

## Idempotency Check  

The endpoint expects a unique **IdempotencyKey** in the incoming request to ensure consistency between the client and the endpoint.  

- If an order already exists with this key, the endpoint will return the corresponding **order ID** directly.  

## Transactional Customer and Order Creation  

Before creating an **order**, the endpoint first checks if the customer exists in the system. If not, it creates a new customer.  

- Exceptionally, if an error occurs during the **order creation** process after creating the new customer, the newly created customer will be deleted from the database.  

## Rate Limiting  

As an example, the endpoint is configured to handle up to 100 requests per second. If more than 100 requests are made, the system will return a **429 Too Many Requests** error.  
