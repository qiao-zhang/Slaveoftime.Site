module Slaveoftime.Endpoints

open System
open Microsoft.AspNetCore.Http
open Fun.AspNetCore
open Slaveoftime
open Slaveoftime.UI.Components.PostView
open Slaveoftime.UI.Pages

let endpoints =
    endpoints "" {
        get "feed" { handle (fun () -> Feed.handle Feed.RSS) }
        get "feed/rss" { handle (fun () -> Feed.handle Feed.RSS) }
        get "feed/atom" { handle (fun () -> Feed.handle Feed.ATOM) }

        get "signin" { SigninPage.Create() }
        post "signin" { handle Authenticate.signin }
        post "signout" {
            authorization
            handle Authenticate.signout
        }

        endpoints "view" {
            get "post-list" { PostList.Create() }
            get "post/{id}" { handle (fun (id: Guid) -> PostDetail.Create id |> Results.View) }
            get "post/feed/{id}" { handle (fun (id: Guid) -> PostDetail.CreateForFeed id) }
            get "post/{postId}/comment" { handle (fun (postId: Guid) -> PostComment.Create postId |> Results.View) }
            post "post/{postId}/comment" {
                authorization
                handle (fun postId parentComment -> PostComment.NewCommentCustomElement(postId, parentComment) |> Results.View)
            }
        }

        get "blog/{id:guid}" { handle (fun (id: Guid) -> Results.View(PostDetailPage.Create id)) }
        get "blog/{slug}" { handle (fun (slug: string) -> Results.View(PostDetailPage.Create slug)) }
        get "/" { PostListPage.Create() }
    }
