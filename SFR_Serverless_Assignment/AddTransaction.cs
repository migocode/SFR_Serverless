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
using Newtonsoft.Json;
using SFR_Serverless_Assignment.Models;

namespace SFR_Serverless_Assignment
{
    public class AddTransaction
    {
        private readonly ILogger<AddTransaction> _logger;

        public AddTransaction(ILogger<AddTransaction> log)
        {
            _logger = log;
        }

        [FunctionName("AddTransaction")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "AddTransaction" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Transaction), Description = nameof(Transaction), Required = true)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(databaseName: "SFR_Serverless_Assignment", collectionName: "Transactions",
                ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<Transaction> transactionsCollector)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Transaction transaction = JsonConvert.DeserializeObject<Transaction>(requestBody);

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
    }
}

