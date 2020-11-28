namespace Slaveoftime.Services

open System
open System.IO
open System.Linq
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open CliWrap
open Markdig
open Markdig.Parsers
open Markdig.Renderers
open Fun.Result
open Slaveoftime.Db


type GithubPoolingService(loger: ILogger<GithubPoolingService>, env: IHostEnvironment, db: SlaveoftimeDb) =
    let (</>) p1 p2 = Path.Combine(p1, p2)

    let (<?>) (x: 'T voption) (def: 'T) =
        match x with
        | ValueSome x -> x
        | _ -> def

    let wwwroot = env.ContentRootPath </> "wwwroot"


    let markdownToHtml baseUrl (markdown: string) =
        let pipeline = MarkdownPipelineBuilder().Build()
        use writer = new StringWriter()
        let renderer = new HtmlRenderer(writer)

        renderer.LinkRewriter <-
            fun url ->
                if url.StartsWith "/" then
                    baseUrl + url.Substring(1)
                elif url.StartsWith "./" then
                    baseUrl + url.Substring(2)
                elif url.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) then
                    url
                elif url.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase) then
                    url
                else
                    baseUrl + url

        pipeline.Setup(renderer)

        let document = MarkdownParser.Parse(markdown, pipeline)
        renderer.Render(document) |> ignore

        writer.Flush()
        writer.ToString()


    let processMarkdownFile (id: Guid) (file: string) =
        let markdownFile = File.ReadAllText(file)
        let index = markdownFile.IndexOf("---", 4)
        let baseUrl = id.ToString() + "/"
        let html = markdownToHtml baseUrl (markdownFile.Substring(index + 4))

        let targetFolder = wwwroot </> id.ToString()

        if Directory.Exists targetFolder |> not then
            Directory.CreateDirectory targetFolder |> ignore

        let indexFile = targetFolder </> "index.html"
        File.WriteAllText(indexFile, html)

        let originalFolder = Path.GetDirectoryName file

        Directory.GetFiles(originalFolder, "*.*", SearchOption.AllDirectories)
        |> Seq.filter ((<>) file)
        |> Seq.iter (fun x -> File.Copy(x, targetFolder </> Path.GetFileName x, true))


    let cachePost (file, author, id, title, keywords, description) =
        let fileInfo = FileInfo file
        let targetFolder = wwwroot </> id.ToString()
        let post = db.Posts.Where(fun x -> x.Id = id).FirstOrDefault()

        if post = null then
            processMarkdownFile id file
            let newPost =
                Post(
                    Id = id,
                    Title = title,
                    Keywords = (keywords <?> ""),
                    Description = (description <?> ""),
                    Author = author,
                    CreatedTime = DateTime.Now,
                    UpdatedTime = fileInfo.LastWriteTime
                )
            db.Posts.Add(newPost) |> ignore
            db.SaveChanges() |> ignore

        else if post.UpdatedTime < fileInfo.LastWriteTime || not (Directory.Exists targetFolder) then
            processMarkdownFile id file
            post.Title <- title
            post.Keywords <- keywords <?> ""
            post.Description <- description <?> ""
            post.UpdatedTime <- fileInfo.LastWriteTime
            db.SaveChanges() |> ignore


    let processMdFile (file: string) author =
        let mutable i = 0
        let mutable isDone = false

        let mutable id = ValueNone
        let mutable title = ValueNone
        let mutable description = ValueNone
        let mutable keywords = ValueNone

        use lines = File.ReadLines(file).GetEnumerator()

        while not isDone && lines.MoveNext() do
            if i > 10 || lines.Current = "---" then
                isDone <- true
            else
                match lines.Current with
                | SafeStringTail "- title:" tail -> title <- ValueSome tail
                | SafeStringTail "- keywords:" tail -> keywords <- ValueSome tail
                | SafeStringTail "- description:" tail -> description <- ValueSome tail
                | SafeStringTail "- id:" tail ->
                    id <-
                        match Guid.TryParse(tail.Trim()) with
                        | true, x -> ValueSome x
                        | _ -> ValueNone
                | _ -> ()

            i <- i + 1

        match title, id with
        | ValueSome title, ValueSome id -> cachePost (file, author, id, title, keywords, description)
        | _ -> ()


    let syncRepo (url: string) =
        task {

            let reposFolder =
#if DEBUG
                "../../Slaveoftime.Site.Test"
#else
                ""
#endif

            if reposFolder <> "" && Directory.Exists reposFolder |> not then
                Directory.CreateDirectory reposFolder |> ignore

            let segs = url.Split "/"
            let author = segs.[segs.Length - 2]
            let authorFolder = reposFolder </> author
            let name = segs.[segs.Length - 1].Split(".").[0]
            let repoFolder = authorFolder </> name

            if Directory.Exists authorFolder |> not then
                Directory.CreateDirectory authorFolder |> ignore

            let! result =
                if Directory.Exists repoFolder then
                    task {
                        let! result1 = Cli.Wrap("git").WithArguments("fetch").WithWorkingDirectory(repoFolder).ExecuteAsync()
                        if result1.ExitCode = -1 then
                            return result1
                        else
                            return! Cli.Wrap("git").WithArguments("pull").WithWorkingDirectory(repoFolder).ExecuteAsync()
                    }
                else
                    Cli.Wrap("git").WithArguments($"clone {url}").WithWorkingDirectory(authorFolder).ExecuteAsync()

            loger.LogInformation $"Process {url}: {result.ExitCode}"

            let files = Directory.GetFiles(repoFolder, "*.md", SearchOption.AllDirectories)
            for file in files do
                processMdFile file author
        }


    member _.Pull(url) =
        try
            loger.LogInformation $"Process {url}"
            syncRepo url
        with
            | ex ->
                loger.LogError(ex, $"Proccess {url} failed")
                Task.retn ()
