#r "nuget: Fake.Core.Process,5.20.0"
#r "nuget: Fake.IO.FileSystem,5.20.0"
#r "nuget: Fake.Core.Environment,5.20.0"
#r "nuget: Fake.IO.Zip,5.20.0"
#r "nuget: BlackFox.Fake.BuildTask,0.1.3"
#r "nuget: Fun.Build, 0.2.2"

open System
open System.IO
open System.Diagnostics
open Fun.Build
open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators


let serverPath = __SOURCE_DIRECTORY__ </> "Slaveoftime.Site"
let deployDir = __SOURCE_DIRECTORY__ </> "deploy"
let publishDir = deployDir </> "publish"


let checkEnv =
    stage "CheckEnv" {
        run "dotnet tool restore"
        run "dotnet build"
    }


pipeline "dev" {
    checkEnv
    stage "open-tool" {
        paralle
        run "code ."
        stage "cmds" {
            paralle
            workingDir serverPath
            run "dotnet run"
            run "dotnet fun-blazor watch Slaveoftime.Site.fsproj"
            run (fun _ -> async {
                do! Async.Sleep 5000
                return "dotnet tailwindcss -i ./wwwroot/css/app.css -o ./wwwroot/css/app-generated.css --watch"
            })
        }
    }
    runIfOnlySpecified
}


pipeline "deploy" {
    checkEnv
    stage "bundle" {
        workingDir serverPath
        run "dotnet tailwindcss -i ./wwwroot/css/app.css -o ./wwwroot/css/app-generated.css --minify"
        run $"dotnet publish -c Release -o {publishDir}"
    }
    stage "push" {
        whenEnvVar "GITHUB_ACTION"
        run (fun ctx ->
            let packagePath = deployDir </> DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".zip"

            if Directory.Exists deployDir |> not then Shell.mkdir deployDir

            !!(publishDir </> "**/*.*") |> Zip.zip publishDir packagePath

            let host = ctx.GetEnvVar "IIS_SERVER_COMPUTER_NAME"
            let user = ctx.GetEnvVar "IIS_SERVER_USERNAME"
            let pwd = ctx.GetEnvVar "IIS_SERVER_PASSWORD"
            let appName = ctx.GetEnvVar "RECYCLE_APP_NAME"
            let siteName = ctx.GetEnvVar "IIS_WEBSITE_NAME"
            let targetDir = ctx.GetEnvVar "WEBSITE_CONTENT_PATH"

            let msdeploy (x: string) =
                let cmd = ProcessStartInfo("C:/Program Files (x86)/IIS/Microsoft Web Deploy V3/msdeploy.exe", Arguments = x)
                let proc = Process.Start cmd
                proc.WaitForExit()

            msdeploy $"-verb:sync -allowUntrusted -source:recycleApp -dest:recycleApp=\"{appName}\",recycleMode=\"StopAppPool\",computerName=\"{host}/msdeploy.axd?site={siteName}\",username=\"{user}\",password=\"{pwd}\",AuthType=\"Basic\""
            msdeploy $"-verb:sync -allowUntrusted -source:package=\"{packagePath}\" -dest:contentPath=\"{targetDir}\",computerName=\"{host}/msdeploy.axd?site={siteName}\",username=\"{user}\",password=\"{pwd}\",AuthType=\"Basic\" -enableRule:DoNotDeleteRule"
            msdeploy $"-verb:sync -allowUntrusted -source:recycleApp -dest:recycleApp=\"{appName}\",recycleMode=\"StartAppPool\",computerName=\"{host}/msdeploy.axd?site={siteName}\",username=\"{user}\",password=\"{pwd}\",AuthType=\"Basic\""
        )
    }
    runIfOnlySpecified
}


tryPrintPipelineCommandHelp ()
