using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using SFR_Serverless_Assignment.Models;



namespace SFR_Serverless_Assignment
{
    public class MonthlyReport
    {
        private readonly ILogger<MonthlyReport> _logger;
        private readonly FeedOptions queryOptions = new() { EnableCrossPartitionQuery = true };

        public MonthlyReport(ILogger<MonthlyReport> log)
        {
            _logger = log;
        }

        [FunctionName("MonthlyReport")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "MonthlyReport" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "iban", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **IBAN** parameter")]
        [OpenApiParameter(name: "month", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Month** parameter as an integer (1 - 12)")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            [CosmosDB(ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient dbClient)
        {
            string iban = req.Query["iban"];
            string month = req.Query["month"];

            if(int.TryParse(month, out int monthInt) == false)
            {
                _logger.LogError("Invalid value for 'month' submitted");
                return new NotFoundResult();
            }

            if(string.IsNullOrWhiteSpace(iban))
            {
                _logger.LogError("Empty IBAN submitted");
                return new NotFoundResult();
            }

            Uri transactionCollectionUri = UriFactory.CreateDocumentCollectionUri("SFR_Serverless_Assignment", "Transactions");
            Uri customerCollectionUri = UriFactory.CreateDocumentCollectionUri("SFR_Serverless_Assignment", "Customers");

            Customer? customer;
                
            IDocumentQuery<Customer> customerQuery = dbClient.CreateDocumentQuery<Customer>(customerCollectionUri, queryOptions)
                .Where(c => c.Iban == iban)
                .AsDocumentQuery();

            List<Customer> customers = await ExecuteQueryAsync(customerQuery);

            if(customers.Count == 0)
            {
                return new OkObjectResult(new Report());
            }

            customer = customers[0];

            DateTime from = new(DateTime.Now.Year, monthInt, 1);
            DateTime to = new(DateTime.Now.Year, monthInt, DateTime.DaysInMonth(DateTime.Now.Year, monthInt));

            IDocumentQuery<OutboundTransaction> transactionQuery = dbClient.CreateDocumentQuery<OutboundTransaction>(transactionCollectionUri, queryOptions)
                .Where(t => (t.CreditorIban == customer.Iban || t.DebitorIban == customer.Iban) && t.Date >= from && t.Date <= to)
                .AsDocumentQuery();

            List<OutboundTransaction> transactions = await ExecuteQueryAsync(transactionQuery);

            Report report = new()
            {
                Customer = customer,
                Month = monthInt,
                MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthInt)
            };
            report.Transactions.AddRange(transactions);

            return new OkObjectResult(report);
        }

        private async Task<List<T>> ExecuteQueryAsync<T>(IDocumentQuery<T> query)
        {
            List<T> results = new();
            while (query.HasMoreResults)
            {
                foreach (T document in await query.ExecuteNextAsync())
                {
                    results.Add(document);
                }
            }

            return results;
        }
    }
}

