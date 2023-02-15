namespace Slaveoftime

open System
open System.IO
open System.Text
open System.Linq
open System.Reflection
open System.Runtime.CompilerServices
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Formats.Webp
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db
open Slaveoftime.UI.Components
open Slaveoftime.UI.Components.PostView

[<Extension>]
type DbCheck =

    [<Extension>]
    static member MigrateDb(sp: IServiceProvider) =
        let db = sp.GetService<SlaveoftimeDb>()
        let memoryCache = sp.GetService<IMemoryCache>()
        let logger = sp.GetService<ILoggerFactory>().CreateLogger("DbCheck")
        let deserializer = DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build()

        let addOrUpdatePost postType (meta: PostMeta) =
            let post = db.Posts.FirstOrDefault(fun x -> x.Id = meta.Id) |> Option.ofObj |> Option.defaultWith Post

            if post.Id = Guid.Empty then
                post.Id <- meta.Id
                db.Posts.Add post |> ignore

            match postType with
            | PostType.Static x -> post.ContentPath <- x
            | PostType.Dynamic -> post.IsDynamic <- true

            post.Title <- meta.Title
            post.Keywords <- meta.Keywords
            post.Description <- meta.Description
            post.UpdatedTime <- DateTime.Now
            post.CreatedTime <- meta.CreateTime
            post.Slug <- toSlug meta.Title
            post.MainImage <- meta.MainImage
            post.IsActive <- not meta.IsHidden


        let fetchPostsFromFiles () =
            logger.LogInformation("Fetch post from physical files")
            Directory.GetFiles(postsDir, "*.md", EnumerationOptions(RecurseSubdirectories = true))
            |> Seq.iter (fun path ->
                logger.LogInformation("Process {file} for post", path)
                let file = File.ReadLines path
                if Seq.head file = "---" then
                    try
                        let metaLines = file |> Seq.skip 1 |> Seq.takeWhile ((<>) "---")
                        // Because we copied all the files under UI/Pages/Posts and serve static files on /blog at the Startup.fs
                        let relativeUrl = Path.GetDirectoryName(path).Substring(postsDir.Length + 1).Replace("\\", "/")
                        let baseUrl = host <//> "blog" <//> relativeUrl
                        let htmlPath = Path.GetDirectoryName path </> Path.GetFileNameWithoutExtension path + ".html"

                        // Parse post meta
                        metaLines
                        |> String.concat Environment.NewLine
                        |> deserializer.Deserialize<PostMeta>
                        |> fun meta ->
                            if String.IsNullOrEmpty meta.MainImage |> not then
                                { meta with MainImage = relativeUrl <//> meta.MainImage }
                            else
                                meta
                        |> addOrUpdatePost (PostType.Static htmlPath)

                        // Convert post detail
                        file
                        |> Seq.skip (2 + metaLines.Count())
                        |> String.concat Environment.NewLine
                        |> fun lines -> Markdown.ConvertToHtml(baseUrl, lines)
                        |> fun html -> File.WriteAllText(htmlPath, html)

                        logger.LogInformation("Found post {path}", path)

                    with ex ->
                        logger.LogError(ex, "Parse post failed: {path}", path)
            )


        let fetchPostsFromReflection (updateDb: bool) =
            logger.LogInformation("Fetch post from reflection")
            Assembly.GetExecutingAssembly().GetTypes()
            |> Seq.iter (fun ty ->
                if
                    ty.Namespace <> null
                    && ty.Namespace.StartsWith "Slaveoftime.UI.Pages.Posts"
                    && ty.GetInterface(nameof IDynamicPost) <> null
                then
                    try
                        logger.LogInformation("Process {type} for dynamic post", ty.Name)

                        let meta = ty.GetProperty(nameof IDynamicPost.Meta).GetValue(null) :?> PostMeta
                        let view = ty.GetProperty(nameof IDynamicPost.View).GetValue(null) :?> NodeRenderFragment

                        if updateDb then
                            addOrUpdatePost PostType.Dynamic meta
                        
                        // We can use the cached node to render for the specific post dynamically
                        memoryCache.Set($"post-dynamic-{meta.Id}", view) |> ignore

                        logger.LogInformation("Found post {name}", ty.Name)

                    with ex ->
                        logger.LogError(ex, "Parse post failed: {name}", ty.Name)
            )


        let prepareCodeBlocksForDynamicPost () =
            logger.LogInformation("Prepare code blocks")
            Directory.GetFiles(postsDir, "*.fs", EnumerationOptions(RecurseSubdirectories = true))
            |> Seq.iter (fun path ->
                logger.LogInformation("Process {file} for code blocks", path)
                let file = File.ReadLines path
                
                let mutable lines = StringBuilder()
                let mutable codeBlockName = None
                let mutable indentLength = None

                for line in file do
                    try
                        if codeBlockName.IsNone && line.Trim().StartsWith(codeBlockStartPrefix) then
                            codeBlockName <- Some(line.Substring(line.IndexOf(codeBlockStartPrefix) + codeBlockStartPrefix.Length).Trim())
                            lines.AppendLine "```fsharp" |> ignore

                        else if codeBlockName.IsSome && line.Trim() <> codeBlockEndPrefix then
                            if indentLength.IsNone then
                                indentLength <- Some(line.Length - line.TrimStart().Length)
                            lines.AppendLine(if line.Length > indentLength.Value then line.Substring(indentLength.Value) else line) |> ignore

                        else if codeBlockName.IsSome then
                            lines.AppendLine "```" |> ignore
                            // Because we copied all the files under UI/Pages/Posts and serve static files on /blog at the Startup.fs
                            let relativeUrl = Path.GetDirectoryName(path).Substring(postsDir.Length + 1).Replace("\\", "/")
                            let baseUrl = host <//> "blog" <//> relativeUrl
                            let htmlPath = Path.GetDirectoryName path </> codeBlockName.Value + ".html"

                            let codeBlock = Markdown.ConvertToHtml(baseUrl, lines.ToString())
                            File.WriteAllText(htmlPath, codeBlock)

                            logger.LogInformation("Found {codeblock}", codeBlockName)
                            
                            lines.Clear() |> ignore
                            codeBlockName <- None
                            indentLength <- None

                    with ex ->
                        logger.LogError(ex, "Parse codeblock failed: {path}", path)
            )


        let optimizeImages () =
            logger.LogInformation("Optimize images")
            Directory.EnumerateFiles(postsDir, "*", SearchOption.AllDirectories)
            |> Seq.filter (
                function
                | SafeStringEndWithCi ".png"
                | SafeStringEndWithCi ".jpeg" -> true
                | _ -> false
            )
            |> Seq.iter (fun file ->
                let info = FileInfo file
                // Only optimize image size is bigger than 200kb
                if info.Length > 1024L * 200L then
                    try
                        logger.LogInformation("Optimize image: {file}", file)
                        let postfix = "-original"
                        let hasOriginal = Path.GetFileNameWithoutExtension(file).EndsWith(postfix)

                        let originalFile =
                            if hasOriginal then
                                file
                            else
                                Path.GetDirectoryName file </> Path.GetFileNameWithoutExtension file + postfix + Path.GetExtension file
                    
                        let lowQualityFile =
                            if hasOriginal then
                                Path.GetDirectoryName file </> Path.GetFileNameWithoutExtension(file).Replace(postfix, "") + Path.GetExtension file
                            else
                                file

                        if not hasOriginal then File.Copy(file, originalFile, true)

                        use img = Image.Load originalFile
                        img.SaveAsWebp(lowQualityFile, WebpEncoder(Quality = 1))

                    with ex ->
                        logger.LogError(ex, "Optimize image: {file} failed", file)
            )


        if isVersionChanged then
            logger.LogInformation("Migrate database")
            db.Database.Migrate()
            
            fetchPostsFromFiles ()
            fetchPostsFromReflection true
            prepareCodeBlocksForDynamicPost ()
            optimizeImages ()
            
            logger.LogInformation("Save database changes")
            db.SaveChanges() |> ignore

        else
            fetchPostsFromReflection false

        sp
