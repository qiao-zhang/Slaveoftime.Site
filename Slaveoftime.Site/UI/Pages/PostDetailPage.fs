namespace Slaveoftime.UI.Pages

open System
open System.Linq
open Fun.Blazor
open Slaveoftime
open Slaveoftime.Db
open Slaveoftime.UI.Components
open Slaveoftime.UI.Components.PostView

type PostDetailPage =

    static member Create(post: Post option) =
        match post with
        | Some post ->
            Layout.Create(
                headerNode =
                    html.fragment [
                        headerTitle post.Title
                        headerKeywords post.Keywords
                        headerDescription post.Description
                        if String.IsNullOrEmpty post.MainImage then
                            meta {
                                name "twitter:card"
                                content "summary"
                            }
                        else
                            let imageUrl = host <//> "blog" <//> post.MainImage
                            meta {
                                name "twitter:card"
                                content "summary_large_image"
                            }
                            meta {
                                name "twitter:image"
                                content imageUrl
                            }
                            meta {
                                name "og:image"
                                content imageUrl
                            }
                    ],
                bodyNode = PostDetail.Create post
            )

        | None -> Layout.Create(bodyNode = PostDetail.PostNotFound)

    static member Create(postId: Guid) =
        html.inject (fun (db: SlaveoftimeDb) ->
            db.Posts.FirstOrDefault(fun x -> x.IsActive && x.Id = postId) |> Option.ofObj |> PostDetailPage.Create
        )

    static member Create(postSlug: string) =
        html.inject (fun (db: SlaveoftimeDb) ->
            db.Posts.FirstOrDefault(fun x -> x.IsActive && x.Slug = postSlug) |> Option.ofObj |> PostDetailPage.Create
        )
