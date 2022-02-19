[<AutoOpen>]
module Slaveoftime.UI.Store

open System
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


type IShareStore with

    member store.IsPrerendering = store.CreateCVal(nameof store.IsPrerendering, false)


type IGlobalStore with

    member store.UsePosts(page: int) =
        store.CreateCVal(nameof store.UsePosts + string page, DeferredState<CachePosts, string>.NotStartYet)

    member store.UsePost(postId: Guid) =
        store.CreateCVal(nameof store.UsePost + string postId, DeferredState<CachePost, string>.NotStartYet)


and CachePosts = { ExpireDate: DateTime; Posts: Post list }
and CachePost = { ExpireDate: DateTime; Post: Post; PostContent: string }
