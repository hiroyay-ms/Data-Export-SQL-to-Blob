using System.Linq;
using System.Net;
using System.Text;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;

using WebApiProj.Data;
using WebApiProj.Models;

public static class SalesOrderEndpoints
{
    public static void RegisterSalesOrderEndpoints(this WebApplication app)
    {
        app.MapGet("/SalesOrders", ExportSalesOrders)
            .WithName("ExportSalesOrders")
            .WithOpenApi();
    }

    static async Task<IResult> ExportSalesOrders(AdventureWorksContext db)
    {
            var filename = $"SalesOrder_{DateTime.Now.ToString("yyyyMMddHHmm")}_api.csv";

            var storageAccountName = System.Environment.GetEnvironmentVariable("STORAGE_ACCOUNT") ?? throw new InvalidOperationException("Connection string 'STORAGE_ACCOUNT' not found.");
            DefaultAzureCredential credential = new();
            BlobServiceClient blobServiceClient = new BlobServiceClient(new System.Uri($"https://{storageAccountName}.blob.core.windows.net"), credential);
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient("output-from-api");

            BlobClient blobClient = container.GetBlobClient(filename);
            var query = from oh in db.SalesOrderHeader 
                        join od in db.SalesOrderDetail on oh.SalesOrderID equals od.SalesOrderID 
                        join p in db.Product on od.ProductID equals p.ProductID 
                        join pc in db.ProductCategory on p.ProductCategoryID equals pc.ProductCategoryID 
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
            
            var salesOrders = await query.ToListAsync<SalesOrder>();

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

            return salesOrders.Count == 0 ? TypedResults.NotFound() : TypedResults.Ok($"The file was successfully uploaded on blob storage with name {filename}");
    }
}