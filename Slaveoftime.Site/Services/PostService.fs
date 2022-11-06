namespace Slaveoftime.Services

open System
open System.IO
open System.Linq
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Caching.Memory
open Microsoft.EntityFrameworkCore
open Slaveoftime.Db
open Fun.Result


type PostWithContent = { Post: Post; PostContent: string }


type PostService(db: SlaveoftimeDb, memoryCache: IMemoryCache, logger: ILogger<PostService>, host: IHostEnvironment) as this =

    let postCacheKey (postId: Guid) = $"post-detail-{postId}"


    static member PostsCacheKey = "posts"


    member _.GetPosts() =
        let getter (entry: ICacheEntry) = task {
            let query = db.Posts.OrderByDescending(fun x -> x.CreatedTime)

            let! result = query.ToListAsync() |> Task.map Seq.toList

            if result.Length > 0 then
                entry.SetValue(result).SetSlidingExpiration(TimeSpan.FromDays 1) |> ignore

            return result
        }

        memoryCache.GetOrCreateAsync(PostService.PostsCacheKey, getter)


    member _.GetPost(postId) =
        let cackeKey = postCacheKey postId

        let getter (entry: ICacheEntry) = task {
            try
                logger.LogInformation $"Load post detail from db: {postId}"

                let file = Path.Combine(host.ContentRootPath, "wwwroot", postId.ToString(), "index.html")
                let fileContent = File.ReadAllText file

                logger.LogInformation $"Loaded post detail from db: {postId}"

                entry.SetValue(fileContent).SetSlidingExpiration(TimeSpan.FromMinutes 5) |> ignore
                return Ok fileContent

            with ex ->
                logger.LogError(ex, "Get post detail failed")
                return Error ex.Message
        }

        taskResult {
            let! posts = this.GetPosts() |> Task.map Ok
            let post = posts |> List.tryFind (fun x -> x.Id = postId)

            match post with
            | None -> return! TaskResult.ofError "Post is not found"
            | Some post ->
                let! content = memoryCache.GetOrCreateAsync(cackeKey, getter)
                return { Post = post; PostContent = content }
        }


    member _.IncreaseViewCount(id) = task {
        try
            let! doc = db.Posts.FirstOrDefaultAsync(fun x -> x.Id = id)

            if doc <> null then
                doc.ViewCount <- doc.ViewCount + 1
                do! db.SaveChangesAsync() |> Task.map ignore

                match memoryCache.TryGetValue<Post list>(PostService.PostsCacheKey) with
                | false, _ -> ()
                | true, posts -> posts |> List.iter (fun (x: Post) -> if x.Id = id then x.ViewCount <- doc.ViewCount)

        with ex ->
            logger.LogError(ex, "Increase view count failed")
    }
