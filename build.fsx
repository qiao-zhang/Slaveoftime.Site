#r "nuget: Fake.Core.Process,5.20.0"
#r "nuget: Fake.IO.FileSystem,5.20.0"
#r "nuget: Fake.Core.Environment,5.20.0"
#r "nuget: Fake.IO.Zip,5.20.0"
#r "nuget: BlackFox.Fake.BuildTask,0.1.3"

open System
open System.IO
open Fake.Core
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.IO.FileSystemOperators
open BlackFox.Fake


let serverPath          = __SOURCE_DIRECTORY__ </> "Slaveoftime.Site"
let deployDir           = __SOURCE_DIRECTORY__ </> "deploy"
let publishDir          = deployDir </> "publish"



fsi.CommandLineArgs
|> Array.skip 1
|> BuildTask.setupContextFromArgv 


let run cmd args workingDir =
    let arguments = args |> Arguments.OfWindowsCommandLine
    Command.RawCommand (cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore


let npm = run @"C:\Program Files\nodejs\npm.cmd"
let pnpm = run "pnpm.cmd"
let dotnet = run "dotnet.exe"
let msdeploy = run "C:/Program Files (x86)/IIS/Microsoft Web Deploy V3/msdeploy.exe"


let checkEnv =
    BuildTask.create "CheckEnv" [] {
        npm "install pnpm -g" serverPath
        pnpm "install" serverPath
    }


let bundle =
    BuildTask.create "Bundle" [checkEnv] {
        Shell.cleanDir publishDir

        pnpm "run build-css" serverPath

        let publishArgs = sprintf "publish -c Release -o %s" publishDir
        dotnet publishArgs serverPath

        Shell.deleteDir (publishDir </> "BlazorDebugProxy")

        !!(publishDir </> "**/*.*")
        |> Zip.zip publishDir (deployDir </> DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".zip")
    }


let deploy =
    BuildTask.create "Deploy" [ bundle ] {
        let packagePath = deployDir </> DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".zip"

        if Directory.Exists deployDir |> not then Shell.mkdir deployDir

        !!(publishDir </> "**/*.*")
        |> Zip.zip publishDir packagePath

        let host = Environment.environVar "IIS_SERVER_COMPUTER_NAME"
        let user = Environment.environVar "IIS_SERVER_USERNAME"
        let pwd = Environment.environVar "IIS_SERVER_PASSWORD"
        let appName = Environment.environVar "RECYCLE_APP_NAME"
        let siteName = Environment.environVar "IIS_WEBSITE_NAME"
        let targetDir = Environment.environVar "WEBSITE_CONTENT_PATH"

        msdeploy $"-verb:sync -allowUntrusted -source:recycleApp -dest:recycleApp=\"{ appName }\",recycleMode=\"StopAppPool\",computerName=\"{ host }/msdeploy.axd?site={ siteName }\",username=\"{ user }\",password=\"{ pwd }\",AuthType=\"Basic\"" ""
        msdeploy $"-verb:sync -allowUntrusted -source:package=\"{ packagePath }\" -dest:contentPath=\"{ targetDir }\",computerName=\"{ host }/msdeploy.axd?site={ siteName }\",username=\"{ user }\",password=\"{ pwd }\",AuthType=\"Basic\" -enableRule:DoNotDeleteRule" ""
        msdeploy $"-verb:sync -allowUntrusted -source:recycleApp -dest:recycleApp=\"{ appName }\",recycleMode=\"StartAppPool\",computerName=\"{ host }/msdeploy.axd?site={ siteName }\",username=\"{ user }\",password=\"{ pwd }\",AuthType=\"Basic\"" ""
    }

BuildTask.runOrDefault bundle
