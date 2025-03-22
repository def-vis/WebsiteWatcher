using System;
using Azure.Storage.Blobs;
using HtmlAgilityPack;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using static WebsiteWatcher.Snapshot;

namespace WebsiteWatcher
{
    public class Watcher(PdfCreatorService pdfCreatorService)
    {
        private const string SqlInputQuery = @"SELECT w.Id, w.Url, w.XPathExpression, s.Content AS LatestContent
                                            FROM dbo.Websites w
                                            LEFT JOIN dbo.Snapshots s ON w.Id = s.Id
                                            WHERE s.Timestamp = (SELECT MAX(Timestamp) FROM dbo.Snapshots WHERE Id = w.Id)";

        [Function(nameof(Watcher))]
        [SqlOutput("dbo.Snapshots", "WebsiteWatcher")]
        public async Task<SnapshotRecords> Run([TimerTrigger("*/20 * * * * *")] TimerInfo myTimer,
            [SqlInput(SqlInputQuery,"WebsiteWatcher")] IReadOnlyList<WebsiteModel> websites)
        {
            SnapshotRecords result = null;
            foreach(var website in websites)
            {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(website.Url);

                var divContent = doc.DocumentNode.SelectSingleNode(website.XPathExpression);
                var content = divContent != null ? divContent.InnerText.Trim() : "No content";
                var contentHasChanged = content != website.LatestContent;

                if (contentHasChanged)
                {
                    var newPdf = await pdfCreatorService.ConvertPageToPdfAsAsync(website.Url);

                    var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                    var blobClient = new BlobClient(connectionString, "pdfs", $"{website.Id}-{DateTime.UtcNow}.pdf");
                    await blobClient.UploadAsync(newPdf);
                    result = new SnapshotRecords(website.Id, content);
                }
            }
            return result;
        }

        public class WebsiteModel
        {
            public Guid Id { get; set; }
            public string Url { get; set; }
            public string? XPathExpression { get; set; }

            public string LatestContent {  get; set; }
        }
    }
}
