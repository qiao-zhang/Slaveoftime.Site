namespace Slaveoftime.Services

open Microsoft.Extensions.Logging
open Microsoft.Extensions.Caching.Memory
open Microsoft.EntityFrameworkCore
open Slaveoftime.Db
open Fun.Result


type PostService(db: SlaveoftimeDb, memoryCache: IMemoryCache, logger: ILogger<PostService>) =

    member _.IncreaseViewCount(id) = task {
        try
            let! doc = db.Posts.FirstOrDefaultAsync(fun x -> x.Id = id)

            if doc <> null then
                doc.ViewCount <- doc.ViewCount + 1
                do! db.SaveChangesAsync() |> Task.map ignore

                match memoryCache.TryGetValue<Post list>("posts") with
                | false, _ -> ()
                | true, posts -> posts |> List.iter (fun (x: Post) -> if x.Id = id then x.ViewCount <- doc.ViewCount)

        with ex ->
            logger.LogError(ex, "Increase view count failed")
    }
