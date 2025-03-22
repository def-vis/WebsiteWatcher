using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;

namespace WebsiteWatcher
{
    public class Snapshot
    {
        [Function(nameof(Snapshot))]
        [SqlOutput("dbo.Snapshots", "WebsiteWatcher")]
        public List<SnapshotRecords> Run(
            [SqlTrigger("[dbo].[Websites]", "WebsiteWatcher")] IReadOnlyList<SqlChange<Website>> changes,
                FunctionContext context)
        {
            List<SnapshotRecords> snapshotRecords = new List<SnapshotRecords>();
            foreach (var change in changes)
            {
                SnapshotRecords snapshot = null;
                Console.WriteLine($"{change.Operation}");
                Console.WriteLine($"Id : {change.Item.Id} Url: {change.Item.Url}");

                if(change.Operation != SqlChangeOperation.Insert)
                {
                    continue;
                }

                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(change.Item.Url);

                var divContent = doc.DocumentNode.SelectSingleNode(change.Item.XPathExpression);
                var content = divContent != null ? divContent.InnerText.Trim() : "No content";

                 Console.WriteLine($"{content}");
                snapshot = new SnapshotRecords(change.Item.Id, content);
                snapshotRecords.Add(snapshot);
            }
            return snapshotRecords;
        }
        public record SnapshotRecords(Guid Id,string Content);
    }

}
