#nowarn "0020" // remove ignore warning

open System
open System.IO
open System.Text.Unicode
open System.Text.Encodings.Web
open System.Reflection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.ResponseCompression
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.WebEncoders
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open SixLabors.ImageSharp.Web.DependencyInjection
open Giraffe
open Serilog
open Serilog.Events
open Slaveoftime
open Slaveoftime.Db
open Slaveoftime.UI.Components
open Slaveoftime.UI.Pages


Log.Logger <-
    LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("./logs/sites.log", rollOnFileSizeLimit = true, fileSizeLimitBytes = 1024L * 1024L * 5L, retainedFileCountLimit = 20)
        .CreateLogger()


let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())
let host = builder.Host
let services = builder.Services

host.UseSerilog()

services.Configure(fun (options: WebEncoderOptions) -> options.TextEncoderSettings <- new TextEncoderSettings(UnicodeRanges.All))
services.AddDbContext<SlaveoftimeDb>(fun options -> options.UseSqlite("Data Source=Slaveofitme.db") |> ignore)
services.AddMemoryCache()

services.AddControllersWithViews()
services.AddServerSideBlazor(fun options -> options.RootComponents.RegisterCustomElementForFunBlazor(Assembly.GetExecutingAssembly()))
services.AddFunBlazorServer()

services.AddResponseCompression(fun option ->
    option.Providers.Add<BrotliCompressionProvider>()
    option.Providers.Add<GzipCompressionProvider>()
    option.EnableForHttps <- true
    option.MimeTypes <- [|
        "text/plain"
        "text/css"
        "text/html"
        "text/xml"
        "text/json"
        "application/javascript"
        "image/*"
        "image/png"
        "image/jpeg"
        "image/svg+xml"
        "font/woff2"
        "image/x-icon"
    |]
)

services.AddResponseCaching(fun c -> c.MaximumBodySize <- 1024L * 1024L * 5L)
services.AddImageSharp()
services.Configure(fun (options: WebEncoderOptions) -> options.TextEncoderSettings <- new TextEncoderSettings(UnicodeRanges.All))


let app = builder.Build()

let scope = app.Services.GetService<IServiceScopeFactory>().CreateScope()
scope.ServiceProvider.MigrateDb()

app.UseResponseCaching()
app.UseResponseCompression()

app.UseImageSharp()

app.UseStaticFiles()
app.UseStaticFiles(
    StaticFileOptions(RequestPath = "/blog", FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory() </> "UI/Pages/Posts"))
)

app.UseGiraffe(Slaveoftime.UI.Pages.Routes.uiRoutes)

app.MapBlazorHub()

app.Run()

Log.CloseAndFlush()
