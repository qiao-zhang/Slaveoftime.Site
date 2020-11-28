#nowarn "0020" // remove ignore warning

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.ResponseCompression
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open SixLabors.ImageSharp.Web.DependencyInjection
open Slaveoftime
open Slaveoftime.Db
open Slaveoftime.Services


let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())
let services = builder.Services


services.AddDbContext<SlaveoftimeDb>(fun options -> options.UseSqlite("Data Source=Slaveofitme.db") |> ignore)
services.AddMemoryCache()

services.AddControllersWithViews()
services.AddServerSideBlazor()
services.AddFunBlazorServer()

services.AddTransient<GithubPoolingService>()
services.AddHostedService<PullingBackgroundService>()

services.AddResponseCompression(fun option ->
    option.Providers.Add<BrotliCompressionProvider>()
    option.Providers.Add<GzipCompressionProvider>()
    option.EnableForHttps <- true
    option.MimeTypes <-
        [|
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


let app = builder.Build()


let scope = app.Services.GetService<IServiceScopeFactory>().CreateScope()
let db = scope.ServiceProvider.GetService<SlaveoftimeDb>()
db.Database.Migrate()

app.UseResponseCaching()
app.UseResponseCompression()
app.UseImageSharp()
app.UseStaticFiles()

app.MapBlazorHub()
app.MapFunBlazor(UI.Index.page)

app.Run()
