[<AutoOpen>]
module Slaveoftime.UI.Hooks

open System
open System.IO
open System.Linq
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


type IComponentHook with

    member hook.TryLoadPosts(page) =
        task {
            let sp = hook.ServiceProvider.CreateScope().ServiceProvider
            let logger = sp.GetService<ILoggerFactory>().CreateLogger(nameof hook.TryLoadPosts)
            let store = sp.GetService<IGlobalStore>()

            let postsStore = store.UsePosts(page)

            match postsStore.Value with
            | DeferredState.Loading -> ()
            | DeferredState.Loaded x when x.ExpireDate > DateTime.Now -> ()
            | _ ->
                try
                    logger.LogInformation $"Load post from db: {page}"
                    let db = sp.GetService<SlaveoftimeDb>()
                    let! posts = db.Posts.OrderByDescending(fun x -> x.CreatedTime).ToArrayAsync() |> Task.map Array.toList
                    postsStore.Publish(DeferredState.Loaded { ExpireDate = DateTime.Now.AddMinutes 5; Posts = posts })
                    logger.LogInformation $"Loaded post from db: {page}"
                with
                    | ex -> logger.LogError $"Load posts failed for page {page}: {ex.Message}"
        }


    member hook.TryLoadPost(postId: Guid) =
        task {
            let sp = hook.ServiceProvider.CreateScope().ServiceProvider
            let logger = sp.GetService<ILoggerFactory>().CreateLogger(nameof hook.TryLoadPost)
            let store = sp.GetService<IGlobalStore>()
            let postStore = store.UsePost postId

            match postStore.Value with
            | DeferredState.Loading -> ()
            | DeferredState.Loaded x when x.ExpireDate > DateTime.Now -> ()
            | _ ->
                try
                    logger.LogInformation $"Load post detail from db: {postId}"
                    let db = sp.GetService<SlaveoftimeDb>()
                    let! post = db.Posts.FirstOrDefaultAsync(fun x -> x.Id = postId)

                    if post <> null then
                        let host = sp.GetService<IHostEnvironment>()
                        let file = Path.Combine(host.ContentRootPath, "wwwroot", postId.ToString(), "index.html")
                        let fileContent = File.ReadAllText file
                        postStore.Publish(
                            DeferredState.Loaded
                                {
                                    ExpireDate = DateTime.Now.AddMinutes 5
                                    Post = post
                                    PostContent = fileContent
                                }
                        )
                        logger.LogInformation $"Loaded post detail from db: {postId}"
                    else
                        postStore.Publish(DeferredState.LoadFailed "Not Found")
                        logger.LogInformation $"Load post detail from db failed: not found {postId}"
                with
                    | ex ->
                        logger.LogError $"Load Post failed for {postId}: {ex.Message}"
                        postStore.Publish(DeferredState.LoadFailed ex.Message)
        }


    member hook.IncreaseViewCount(postId: Guid) =
        task {
            let sp = hook.ServiceProvider.CreateScope().ServiceProvider
            let logger = sp.GetService<ILoggerFactory>().CreateLogger(nameof hook.IncreaseViewCount)
            try
                logger.LogInformation "Increasing view count"
                
                let db = sp.GetService<SlaveoftimeDb>()
                let! post = db.Posts.FirstOrDefaultAsync(fun x -> x.Id = postId)

                if post <> null then
                    post.ViewCount <- post.ViewCount + 1
                    do! db.SaveChangesAsync() |> Task.map ignore
                    logger.LogInformation "Increased view count"
            with
                | ex -> logger.LogError $"Increase view count failed for blog {postId}: {ex.Message}"
        }
