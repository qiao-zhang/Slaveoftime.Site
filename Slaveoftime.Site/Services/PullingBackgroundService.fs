namespace Slaveoftime.Services

open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection


type PullingBackgroundService(logger: ILogger<PullingBackgroundService>, scf: IServiceScopeFactory, config: IConfiguration) =
    inherit BackgroundService()

    override _.ExecuteAsync token =
        task {
            while not token.IsCancellationRequested do
                let urls = config.GetSection("repos").GetChildren()
                let sp = scf.CreateScope().ServiceProvider
                let githubPullingSvc = sp.GetService<GithubPoolingService>()

                for kv in urls do
                    do! githubPullingSvc.Pull kv.Value

                let delay = 1000 * 60 * 5
                logger.LogInformation $"Pulling will pause {delay} ms"
                do! Task.Delay delay
        }
