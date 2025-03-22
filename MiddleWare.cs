using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace WebsiteWatcher
{
    public class MiddleWare : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var request = await context.GetHttpRequestDataAsync();
            if(!context.BindingContext.BindingData.ContainsKey("Url"))
            {
                var response = request.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteStringAsync("no url ");
                return ;
            }
            await next(context);
        }
    }
}
