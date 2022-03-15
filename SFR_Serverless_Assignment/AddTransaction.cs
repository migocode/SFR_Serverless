using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using SFR_Serverless_Assignment.Models;
using System;

namespace SFR_Serverless_Assignment
{
    public class AddTransaction
    {
        private readonly ILogger<AddTransaction> _logger;
        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AddTransaction(ILogger<AddTransaction> log)
        {
            _logger = log;
        }

        [FunctionName("AddTransaction")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "AddTransaction" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(OutboundTransaction), Description = nameof(OutboundTransaction), Required = true)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "SFR_Serverless_Assignment", collectionName: "Transactions",
                ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<InboundTransaction> transactionsCollector)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            InboundTransaction? transaction = JsonSerializer.Deserialize<InboundTransaction>(requestBody, jsonSerializerOptions);

            if(transaction is null)
            {
                return new BadRequestResult();
            }

            try
            {
                transaction.Date = GenerateIso8601DateString(transaction.Date);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error parsing date");
                return new BadRequestResult();
            }

            try
            {
                await transactionsCollector.AddAsync(transaction);
            }
            catch (System.Exception e)
            {
                _logger.LogError(e, "Error writing to CosmosDB");
                return new OkObjectResult(e.Message);
            }

            return new OkObjectResult(transaction);
        }

        private string GenerateIso8601DateString(string date)
        {
            if(DateTime.TryParse(date, out DateTime parsedDate))
            {
                return parsedDate.ToUniversalTime().ToString("o");
            }

            throw new Exception("Date format invalid");
        }
    }
}

