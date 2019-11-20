using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NSwag.Annotations;
using NSwag.SwaggerGeneration.AzureFunctionsV2;

namespace TestNSwag
{
    public static class SwaggerEndpoints
    {
        /// <summary>
        /// Generates Swagger JSON.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [OpenApiIgnore]
        [FunctionName("swagger")]
        public static async Task<IActionResult> Swagger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            // swaggerUIの設定
            var settings = new AzureFunctionsV2ToSwaggerGeneratorSettings();
            settings.Title = "TestNSwag";
            var generator = new AzureFunctionsV2ToSwaggerGenerator(settings);

            // UIに表示したいクラスを記載する
            var funcClasses = new[]
            {
                typeof(TestNSwag),
            };
            var document = await generator.GenerateForAzureFunctionClassesAsync(funcClasses, null);
            
            // Workaround for NSwag global security bug, see https://github.com/RicoSuter/NSwag/pull/2305
            document.Security.Clear();

            var json = document.ToJson();
            return new OkObjectResult(json);
        }
        
        /// <summary>
        /// Serves SwaggerUI files.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="staticfile"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [OpenApiIgnore]
        [FunctionName("swaggerui")]
        public static async Task<IActionResult> SwaggerUi(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swaggerui/{staticfile}")] HttpRequest req,
            string staticfile,
            ILogger log)
        {
            var asm = Assembly.GetAssembly(typeof(SwaggerEndpoints));

            // アクセスされた時のパスをゴニョゴニョしているので環境に合わせる
            var files = asm.GetManifestResourceNames().Select(x => x.Replace("TestNSwag.SwaggerUi.", ""))
                .ToList();

            Console.WriteLine(files);

            int index = -1;
            if ((index = files.IndexOf(staticfile)) != -1)
            {
                var fileExt = staticfile.Split('.').Last();
                var types = new Dictionary<string, string>()
                {
                    {"png", "image/png"},
                    {"html", "text/html"},
                    {"js", "application/javascript"},
                    {"css", "text/css"},
                    {"map", "application/json"}
                };
                var fileMime = types.ContainsKey(fileExt) ? types[fileExt] : "application/octet-stream";
                using (var stream = asm.GetManifestResourceStream(asm.GetManifestResourceNames()[index]))
                {
                    var buf = new byte[stream.Length];
                    await stream.ReadAsync(buf, 0, buf.Length);
                    return new FileContentResult(buf, fileMime);
                }
            }

            return new NotFoundResult();

        }
    }
}
