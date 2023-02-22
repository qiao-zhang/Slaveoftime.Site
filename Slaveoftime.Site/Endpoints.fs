module Slaveoftime.Endpoints

open System
open Microsoft.AspNetCore.Http
open Fun.AspNetCore
open Slaveoftime
open Slaveoftime.UI.Components.PostView
open Slaveoftime.UI.Pages

let endpoints =
    endpoints "" {
        get "feed" { handle (Feed.create Feed.RSS) }
        get "feed/rss" { handle (Feed.create Feed.RSS) }
        get "feed/atom" { handle (Feed.create Feed.ATOM) }

        get "signin" { SigninPage.Create() }
        post "signin" { handle Authenticate.signin }
        post "signout" {
            authorization
            handle Authenticate.signout
        }

        endpoints "view" {
            get "post-list" { PostList.Create() }
            get "post/{id}" { handle (fun (id: Guid) -> Results.View(PostDetail.Create id)) }
            get "post/feed/{id}" { handle (fun (id: Guid) -> Results.View(PostDetail.CreateForFeed id)) }
            get "post/{postId}/comment" { handle (fun (postId: Guid) -> Results.View(PostComment.Create postId)) }
            post "post/{postId}/comment" {
                authorization
                handle (fun postId parentComment -> Results.View(PostComment.CreateNewComment(postId, parentComment)))
            }
        }

        get "blog/{id:guid}" { handle (fun (id: Guid) -> Results.View(PostDetailPage.Create id)) }
        get "blog/{slug}" { handle (fun (slug: string) -> Results.View(PostDetailPage.Create slug)) }
        get "/" { PostListPage.Create() }
    }
