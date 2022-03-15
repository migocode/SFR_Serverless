using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SFR_Serverless_Assignment.Models;

namespace SFR_Serverless_Assignment
{
    public class AddCustomer
    {
        private readonly ILogger<AddCustomer> _logger;
        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AddCustomer(ILogger<AddCustomer> log)
        {
            _logger = log;
        }

        [FunctionName("AddCustomer")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "AddCustomer" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Customer), Description = nameof(OutboundTransaction), Required = true)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "SFR_Serverless_Assignment", collectionName: "Customers",
                ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<Customer> customerCollector)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Customer? customer = JsonSerializer.Deserialize<Customer>(requestBody, jsonSerializerOptions);

            if(customer is null)
            {
                return new BadRequestResult();
            }

            try
            {
                await customerCollector.AddAsync(customer);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, "Error writing to CosmosDB");
                return new OkObjectResult(e.Message);
            }

            return new OkObjectResult(customer);
        }
    }
}

