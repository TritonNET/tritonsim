using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace TritonSim.GUI.Browser.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
                context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";

                var customHeaders = app.Configuration.GetSection("SecurityHeaders").GetChildren();
                foreach (var header in customHeaders)
                {
                    if (!string.IsNullOrEmpty(header.Key))
                    {
                        if (string.IsNullOrEmpty(header.Value))
                            context.Response.Headers.Remove(header.Key);
                        else
                            context.Response.Headers[header.Key] = header.Value;
                    }
                }

                await next();
            });

            string wwwrootPath;

            if (app.Environment.IsDevelopment())
            {
                var devPath = app.Configuration["DevelopmentWwwRootPath"];
                if (!string.IsNullOrEmpty(devPath) && Directory.Exists(devPath))
                {
                    wwwrootPath = devPath;
                    app.Logger.LogInformation($"Development Mode: Serving static files from '{wwwrootPath}'");
                }
                else
                {
                    wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
                }
            }
            else
            {
                wwwrootPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
            }

            if (!Directory.Exists(wwwrootPath))
            {
                Directory.CreateDirectory(wwwrootPath);
                app.Logger.LogWarning($"'wwwroot' directory was missing and has been created at: {wwwrootPath}");
            }

            var contentTypeProvider = new FileExtensionContentTypeProvider();
            contentTypeProvider.Mappings[".js"] = "application/javascript";
            contentTypeProvider.Mappings[".mjs"] = "application/javascript";
            contentTypeProvider.Mappings[".wasm"] = "application/wasm";
            contentTypeProvider.Mappings[".dat"] = "application/octet-stream";
            contentTypeProvider.Mappings[".blat"] = "application/octet-stream";
            contentTypeProvider.Mappings[".dll"] = "application/octet-stream";

            var fso = new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(wwwrootPath),
                RequestPath = "",
                EnableDirectoryBrowsing = false,
                StaticFileOptions =
                {
                    ContentTypeProvider = contentTypeProvider,
                    ServeUnknownFileTypes = true,
                    DefaultContentType = "application/octet-stream"
                }
            };

            app.UseFileServer(fso);

            app.Run("http://+:8080");
        }
    }
}