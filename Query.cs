using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;

namespace WebsiteWatcher
{
    public class Query
    {
        private const string query = @"SELECT w.Id, w.Url, s.[Timestamp] AS LastTimestamp
FROM dbo.Websites w
LEFT JOIN dbo.Snapshots s ON w.Id = s.Id
WHERE s.[Timestamp] = (SELECT MAX([Timestamp]) FROM dbo.Snapshots WHERE Id = w.Id)
AND s.[Timestamp] BETWEEN DATEADD(hour, -3, GETUTCDATE()) AND GETUTCDATE();
";
        [Function("Query")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req,
            [SqlInput(query, "WebsiteWatcher")] IReadOnlyList<dynamic> websites)
        {
            return new OkObjectResult(websites);
        }
    }
}
