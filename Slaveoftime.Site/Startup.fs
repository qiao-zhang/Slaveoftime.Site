#nowarn "0020" // remove ignore warning

open System
open System.IO
open System.Text.Unicode
open System.Text.Encodings.Web
open System.Reflection
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.WebEncoders
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open SixLabors.ImageSharp.Web.DependencyInjection
open Serilog
open Serilog.Events
open Fun.AspNetCore
open Slaveoftime
open Slaveoftime.Db


Log.Logger <-
    LoggerConfiguration()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("./logs/sites.log", rollOnFileSizeLimit = true, fileSizeLimitBytes = 1024L * 1024L * 5L, retainedFileCountLimit = 20)
        .CreateLogger()


let builder = WebApplication.CreateBuilder(Environment.GetCommandLineArgs())
let host = builder.Host
let config = builder.Configuration
let services = builder.Services

host.UseSerilog()

services.Configure(fun (options: WebEncoderOptions) -> options.TextEncoderSettings <- new TextEncoderSettings(UnicodeRanges.All))
services.AddDbContext<SlaveoftimeDb>(fun options -> options.UseSqlite("Data Source=Slaveofitme.db") |> ignore)
services.AddMemoryCache()

services.AddControllersWithViews()
services.AddServerSideBlazor(fun options -> options.RootComponents.RegisterCustomElementForFunBlazor(Assembly.GetExecutingAssembly()))
services.AddFunBlazorServer()

services.AddResponseCompression()

services.AddResponseCaching(fun c -> c.MaximumBodySize <- 1024L * 1024L * 5L)
services.AddImageSharp()
services.Configure(fun (options: WebEncoderOptions) -> options.TextEncoderSettings <- new TextEncoderSettings(UnicodeRanges.All))

if config.GetSection("auth") <> null then
    let auth =
        services
            .AddAuthentication(fun options -> options.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(fun options ->
                options.LoginPath <- "/signin"
                options.LogoutPath <- "/signout"
            )

    if config.GetSection("auth:github") <> null then
        auth.AddGitHub(fun options ->
            options.ClientId <- config.GetValue("auth:github:clientId")
            options.ClientSecret <- config.GetValue("auth:github:clientSecret")
            options.Scope.Add("user:email")
        )
        |> ignore

let app = builder.Build()


try
    let scope = app.Services.GetService<IServiceScopeFactory>().CreateScope()
    scope.ServiceProvider.MigrateDb() |> ignore

    if not isDebug then
        app.Lifetime.ApplicationStarted.Register(fun () -> Feed.generateFeedFile scope.ServiceProvider |> ignore) |> ignore

    File.WriteAllText(versionStampFile, versionStamp)

with ex ->
    Log.Error(ex, "Prepare assets failed")
    raise ex


app.UseResponseCaching()
app.UseResponseCompression()

app.UseImageSharp()

app.UseStaticFiles()
app.UseStaticFiles(
    StaticFileOptions(RequestPath = "/blog", FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory() </> "UI/Pages/Posts"))
)

app.UseAuthentication()
app.UseAuthorization()

app.MapGroup(Endpoints.endpoints)

app.MapBlazorHub()

app.Run()

Log.CloseAndFlush()
