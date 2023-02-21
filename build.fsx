#r "nuget: Fake.Core.Process,5.20.0"
#r "nuget: Fake.IO.FileSystem,5.20.0"
#r "nuget: Fake.Core.Environment,5.20.0"
#r "nuget: Fake.IO.Zip,5.20.0"
#r "nuget: BlackFox.Fake.BuildTask,0.1.3"
#r "nuget: Fun.Result"
#r "nuget: Fun.Build"

open System
open System.IO
open Fun.Result
open Fun.Build
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
        stage "server" {
            workingDir serverPath
            run "pnpm install"
        }
    }


pipeline "dev" {
    description "Watch and run related tools and projects"
    checkEnv
    stage "open-tool" {
        paralle
        noPrefixForStep
        workingDir serverPath
        run "dotnet watch run"
        run "pnpm tailwindcss -i ./wwwroot/css/tailwind-source.css -o ./wwwroot/css/tailwind-generated.css --watch"
    }
    runIfOnlySpecified
}


pipeline "deploy" {
    description $"Publish to folder: {deployDir}. You can enable deploy to some cloud by yourshelf."
    stage "deploy-env" { run "npm install pnpm -g" }
    checkEnv
    stage "bundle" {
        workingDir serverPath
        run "pnpm tailwindcss -i ./wwwroot/css/tailwind-source.css -o ./wwwroot/css/tailwind-generated.css --minify"
        run $"dotnet publish -c Release -o {publishDir}"
    }
    stage "push" {
        whenEnvVar "GITHUB_ACTION"
        run (fun _ -> asyncResult {
            let packagePath = deployDir </> DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".zip"

            if Directory.Exists deployDir |> not then Shell.mkdir deployDir

            !!(publishDir </> "**/*.*") |> Zip.zip publishDir packagePath
        })
    }
    runIfOnlySpecified
}


tryPrintPipelineCommandHelp ()
