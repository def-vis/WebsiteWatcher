using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using Azure.Storage.Blobs;

namespace WebsiteWatcher
{
    public class PdfCreator(PdfCreatorService pdfCreatorService)
    {
        [Function(nameof(PdfCreator))]
        public async Task Run(
            [SqlTrigger("[dbo].[Websites]", "WebsiteWatcher")] SqlChange<Website>[] changes,
                FunctionContext context)
        {
            foreach (var change in changes)
            {
                if(change.Operation == SqlChangeOperation.Insert )
                {
                    var result = await pdfCreatorService.ConvertPageToPdfAsAsync(change.Item.Url);                 

                    var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                    var blobClient = new BlobClient(connectionString, "pdfs", $"{change.Item.Id}.pdf");
                    await blobClient.UploadAsync(result);
                }
            }
        }

    }

}
