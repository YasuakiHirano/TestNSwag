using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSwag.Annotations.AzureFunctionsV2;
using NSwag.Annotations;
using System.Net;

namespace TestNSwag
{
    public static class TestNSwag
    {
        /// <summary>
        /// SwaggerTest
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [SwaggerQueryParameter("name", Required = false, Type = typeof(string), Description = "ユーザー名")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(string), Description = "表示結果")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(string), Description = "内部エラー")]
        [FunctionName("TestNSwag")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }
}
