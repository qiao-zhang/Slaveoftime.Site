[<AutoOpen>]
module Slaveoftime.UI.Hooks

open System
open System.IO
open System.Linq
open FSharp.Data.Adaptive
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Caching.Memory
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


type IShareStore with

    member store.Header = store.CreateCVal(nameof store.Header, "")
    member store.Keywords = store.CreateCVal(nameof store.Keywords, "")
    member store.IsPrerendering = store.CreateCVal(nameof store.IsPrerendering, false)


type PostWithContent = { Post: Post; PostContent: string }


type IComponentHook with

    member hook.ShareStore = hook.ServiceProvider.GetService<IShareStore>()
    member hook.GlobalStore = hook.ServiceProvider.GetService<IGlobalStore>()


    member hook.GetPosts() =
        let memoryCache, db = hook.ServiceProvider.GetMultipleServices<IMemoryCache * SlaveoftimeDb>()

        let cacheKey = "posts"

        let getter (entry: ICacheEntry) = task {
            let query = db.Posts.OrderByDescending(fun x -> x.CreatedTime)

            let! result =
                if hook.ShareStore.IsPrerendering.Value then
                    query.ToList() |> Seq.toList |> Task.retn
                else
                    query.ToListAsync() |> Task.map Seq.toList

            if result.Length > 0 then
                entry.SetValue(result).SetSlidingExpiration(TimeSpan.FromDays 1) |> ignore

            return result
        }

        if hook.ShareStore.IsPrerendering.Value then
            memoryCache.GetOrCreate(cacheKey, (fun entry -> getter(entry).Result)) |> LoadingState.Loaded |> AVal.constant
        else
            memoryCache.GetOrCreateAsync(cacheKey, getter) |> Task.map LoadingState.Loaded |> AVal.ofTask LoadingState.Loading


    member hook.GetPostDetail(postId: Guid) =
        let memoryCache, logFactory = hook.ServiceProvider.GetMultipleServices<IMemoryCache * ILoggerFactory>()

        let logger = logFactory.CreateLogger(nameof hook.GetPostDetail)
        let cackeKey = $"post-detail-{postId}"

        let getter (entry: ICacheEntry) = task {
            try
                logger.LogInformation $"Load post detail from db: {postId}"

                let host = hook.ServiceProvider.GetService<IHostEnvironment>()
                let file = Path.Combine(host.ContentRootPath, "wwwroot", postId.ToString(), "index.html")
                let fileContent = File.ReadAllText file

                logger.LogInformation $"Loaded post detail from db: {postId}"

                entry.SetValue(fileContent).SetSlidingExpiration(TimeSpan.FromMinutes 5) |> ignore
                return LoadingState.Loaded(Some fileContent)

            with ex ->
                logger.LogError(ex, "Get post detail failed")
                return LoadingState.Loaded None
        }

        adaptive {
            match! hook.GetPosts() with
            | LoadingState.NotStartYet -> return LoadingState.NotStartYet

            | LoadingState.Loaded posts
            | LoadingState.Reloading posts ->
                let post = posts |> List.tryFind (fun x -> x.Id = postId)
                match post with
                | None -> return LoadingState.Loaded None
                | Some post ->
                    let makeResult = LoadingState.map (Option.map (fun content -> { Post = post; PostContent = content }))
                    if hook.ShareStore.IsPrerendering.Value then
                        return memoryCache.GetOrCreate(cackeKey, (fun entry -> getter(entry).Result)) |> makeResult
                    else
                        let! content = memoryCache.GetOrCreateAsync(cackeKey, getter) |> AVal.ofTask LoadingState.Loading
                        return makeResult content

            | LoadingState.Loading -> return LoadingState.Loading
        }
