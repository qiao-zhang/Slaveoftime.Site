namespace Slaveoftime.UI.Pages

open System
open System.Linq
open Fun.Blazor
open Slaveoftime.Db
open Slaveoftime.UI.Components
open Slaveoftime.UI.Components.PostView

type PostDetailPage =

    static member Create(post: Post option) =
        match post with
        | Some post ->
            Layout.Create(
                headerNode = fragment {
                    title { post.Title }
                    headerKeywords post.Keywords
                    headerDescription post.Description
                },
                bodyNode = PostDetail.Create post
            )

        | None -> Layout.Create(bodyNode = PostDetail.PostNotFound)

    static member Create(postId: Guid) =
        html.inject (fun (db: SlaveoftimeDb) ->
            db.Posts.FirstOrDefault(fun x -> x.Id = postId)
            |> Option.ofObj
            |> PostDetailPage.Create
        )

    static member Create(postSlug: string) =
        html.inject (fun (db: SlaveoftimeDb) ->
            db.Posts.FirstOrDefault(fun x -> x.Slug = postSlug)
            |> Option.ofObj
            |> PostDetailPage.Create
        )
