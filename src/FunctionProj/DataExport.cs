using System.Linq;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Storage.Blobs;
using FunctionProj.Data;
using FunctionProj.Models;
using System.Text;

namespace FunctionProj
{
    public class DataExport
    {
        private readonly ILogger _logger;
        private readonly AdventureWorksContext _context;

        public DataExport(ILoggerFactory loggerFactory, AdventureWorksContext context)
        {
            _logger = loggerFactory.CreateLogger<DataExport>();
            _context = context;
        }

        [Function("DataExport")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var filename = $"SalesOrder_{DateTime.Now.ToString("yyyyMMddHHmm")}_api.csv";

            // var connectionString = System.Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING") ?? throw new InvalidOperationException("Connection string 'BLOB_CONNECTION_STRING' not found.");
            // BlobContainerClient container = new BlobContainerClient(connectionString, "output-from-api");

            var storageAccountName = System.Environment.GetEnvironmentVariable("STORAGE_ACCOUNT") ?? throw new InvalidOperationException("Connection string 'STORAGE_ACCOUNT' not found.");
            DefaultAzureCredential credential = new();
            BlobServiceClient blobServiceClient = new BlobServiceClient(new System.Uri($"https://{storageAccountName}.blob.core.windows.net"), credential);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient("output-from-api");

            BlobClient blobClient = container.GetBlobClient(filename);

            var query = from oh in _context.SalesOrderHeader 
                        join od in _context.SalesOrderDetail on oh.SalesOrderID equals od.SalesOrderID 
                        join p in _context.Product on od.ProductID equals p.ProductID 
                        join pc in _context.ProductCategory on p.ProductCategoryID equals pc.ProductCategoryID 
                        select new SalesOrder
                        {
                            SalesOrderDetailID = od.SalesOrderDetailID,
                            ProductID = p.ProductID,
                            ProductCategory = pc.Name,
                            ProductName = p.Name,
                            Color = p.Color,
                            Size = p.Size,
                            UnitPrice = od.UnitPrice,
                            UnitPriceDiscount = od.UnitPriceDiscount,
                            OrderQty = od.OrderQty,
                            LineTotal = od.LineTotal,
                            OrderDate = oh.OrderDate
                        };
            
            var salesOrders = query.ToList<SalesOrder>();
            string headerLine = string.Join(",", salesOrders.First().GetType().GetProperties().Select(property => property.Name));

            IEnumerable<string> lines = from so in salesOrders 
                                        let line = string.Join(",", so.GetType().GetProperties().Select(property => property.GetValue(so)))
                                        select line;
            
            List<string> csvData = new List<string>();
            csvData.AddRange(lines);

            StringBuilder csv = new StringBuilder();
            csv.AppendLine(headerLine);

            csvData.ForEach(line => csv.AppendLine(line));

            blobClient.Upload(BinaryData.FromString(csv.ToString()), overwrite: true);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString($"The file was successfully uploaded on blob storage with name {filename}");

            return response;
        }
    }
}
