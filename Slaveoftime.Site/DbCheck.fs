﻿namespace Slaveoftime

open System
open System.IO
open System.Linq
open System.Reflection
open System.Runtime.CompilerServices
open System.Globalization
open System.Text
open System.Text.RegularExpressions
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open YamlDotNet.Serialization
open YamlDotNet.Serialization.NamingConventions
open Fun.Blazor
open Slaveoftime.Db
open Slaveoftime.UI.Components

[<Extension>]
type DbCheck =

    [<Extension>]
    static member MigrateDb(sp: IServiceProvider) =
        let db = sp.GetService<SlaveoftimeDb>()
        let memoryCache = sp.GetService<IMemoryCache>()
        let logger = sp.GetService<ILoggerFactory>().CreateLogger("DbCheck")
        let deserializer = DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build()

        let toSlug (title: string) =
            let invalidChars = Regex.Escape(string (Path.GetInvalidFileNameChars()))
            let invalidRegStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars)

            let slug =
                Regex.Replace(title, invalidRegStr, "_").Normalize(NormalizationForm.FormD)
                |> Seq.filter (fun c -> CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark)
                |> String.Concat
                |> fun x -> x.Normalize(NormalizationForm.FormC)

            let slug = slug.Replace(" ", "-")
            slug.ToLowerInvariant()

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

        logger.LogInformation("Migrate database")
        db.Database.Migrate()


        logger.LogInformation("Fetch post from physical files")

        let postsDir = DirectoryInfo("UI/Pages/Posts").FullName
        Directory.GetFiles(postsDir, "*.md", EnumerationOptions(RecurseSubdirectories = true))
        |> Seq.iter (fun path ->
            logger.LogInformation("Process {file} for post", path)
            let file = File.ReadLines path
            if Seq.head file = "---" then
                try
                    let metaLines = file |> Seq.skip 1 |> Seq.takeWhile ((<>) "---")
                    // Because we copied all the files under UI/Pages/Posts and serve static files on /blog at the Startup.fs
                    let baseUrl = "blog/" + Path.GetDirectoryName(path).Substring(postsDir.Length + 1).Replace("\\", "/") + "/"
                    let htmlPath = Path.GetDirectoryName path </> Path.GetFileNameWithoutExtension path + ".html"

                    // Parse post meta
                    metaLines
                    |> String.concat Environment.NewLine
                    |> deserializer.Deserialize<PostMeta>
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

                    addOrUpdatePost PostType.Dynamic meta
                    // We can use the cached node to render for the specific post dynamically
                    memoryCache.Set($"post-dynamic-{meta.Id}", view) |> ignore

                    logger.LogInformation("Found post {name}", ty.Name)

                with ex ->
                    logger.LogError(ex, "Parse post failed: {name}", ty.Name)
        )

        logger.LogInformation("Save database changes")
        db.SaveChanges() |> ignore

        sp
