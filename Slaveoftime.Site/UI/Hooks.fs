[<AutoOpen>]
module Slaveoftime.UI.Hooks

open System
open System.IO
open System.Linq
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


type IComponentHook with

    member hook.LoadPosts() =
        task {
            let sp = hook.ServiceProvider.CreateScope().ServiceProvider
            let cache = sp.GetService<IMemoryCache>()

            let key = "post-list"

            match cache.TryGetValue<Post list> key with
            | true, x -> return Ok x
            | false, _ ->
                try
                    let db = sp.GetService<SlaveoftimeDb>()
                    let! posts = db.Posts.OrderByDescending(fun x -> x.CreatedTime).ToArrayAsync() |> Task.map Array.toList

                    if posts.Length > 0 then
                        cache.Set(key, posts, TimeSpan.FromMinutes 5) |> ignore
                        return Ok posts
                    else
                        return Ok []
                with
                    | ex -> return Error ex.Message
        }


    member hook.LoadPost(postId: Guid) =
        task {
            let sp = hook.ServiceProvider.CreateScope().ServiceProvider
            let logger = sp.GetService<ILoggerFactory>().CreateLogger(nameof hook.LoadPost)
            let cache = sp.GetService<IMemoryCache>()

            let key = $"post-detail-{postId}"

            match cache.TryGetValue<Post * string> key with
            | true, (p, h) -> return Ok(p, h)
            | false, _ ->
                try
                    let db = sp.GetService<SlaveoftimeDb>()
                    let! post = db.Posts.FirstOrDefaultAsync(fun x -> x.Id = postId)

                    if post <> null then
                        let host = sp.GetService<IHostEnvironment>()
                        let file =
                            Path.Combine(host.ContentRootPath, "wwwroot", postId.ToString(), "index.html")
                        let fileContent = File.ReadAllText file
                        cache.Set(key, (post, fileContent), TimeSpan.FromMinutes 5) |> ignore
                        return Ok(post, fileContent)
                    else
                        return Error "Not Found"
                with
                    | ex ->
                        logger.LogError $"Load Post failed for {postId}: {ex.Message}"
                        return Error ex.Message
        }


    member hook.IncreaseViewCount(postId: Guid) =
        task {
            let sp = hook.ServiceProvider.CreateScope().ServiceProvider
            let logger =
                sp.GetService<ILoggerFactory>().CreateLogger(nameof hook.IncreaseViewCount)
            try
                let db = sp.GetService<SlaveoftimeDb>()
                let! post = db.Posts.FirstOrDefaultAsync(fun x -> x.Id = postId)

                if post <> null then
                    post.ViewCount <- post.ViewCount + 1
                    do! db.SaveChangesAsync() |> Task.map ignore
                    logger.LogInformation "Increased view count"
            with
                | ex -> logger.LogError $"Increase view count failed for blog {postId}: {ex.Message}"
        }
