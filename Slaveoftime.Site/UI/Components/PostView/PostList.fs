namespace Slaveoftime.UI.Components.PostView

open System
open System.Linq
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging
open FSharp.Data.Adaptive
open Fun.Blazor
open Slaveoftime.Db

[<CLIMutable>]
type private PostWithCount = { Id: Guid; Count: int }

type PostList =

    static member private PostCard(post: Post, commentCount: int) = div {
        class' "px-6 py-5 rounded-md bg-gray-600/10 mb-5"
        childContent [
            h2 {
                class'
                    "text-primary/90 hover:text-primary first-letter:text-2xl first-letter:text-primary first-letter:uppercase underline text-xl font-semibold"
                a {
                    //href $"blog/{post.Id}?title={post.Title}"
                    href $"blog/{post.Slug}" // To use slug we have to make sure our title is unique
                    post.Title
                }
            }
            section {
                class' "text-primary/80 text-xs my-2 flex items-center gap-2"
                childContent [
                    span { post.CreatedTime.ToString("yyyy-MM-dd") }
                    span {
                        class' "font-semibold"
                        post.Author
                    }
                    if post.ViewCount > 0 then PostView.ViewCount post.ViewCount
                    if post.Likes > 0 then PostView.LiksView post.Likes
                    if commentCount > 0 then
                        span {
                            class' "badge badge-accent badge-sm opacity-90"
                            $"Comments {commentCount}"
                        }
                ]
            }
            div {
                class' "flex flex-wrap items-center gap-2"
                PostView.Keywords post.Keywords
            }
            p {
                class' "dark:text-neutral-400/90 text-neutral-600/90 mt-2 text-sm"
                post.Description
            }
        ]
    }

    static member private NotFound = div {
        class' "p-10 my-10 text-center text-danger-400/50 font-semibold text-2xl"
        "No posts are found"
    }

    static member Create() =
        html.inject (fun (db: SlaveoftimeDb, ctx: IHttpContextAccessor, logger: ILogger<PostList>) -> task {
            try
                let ctx = ctx.HttpContext

                let postsQuery =
                    match ctx.Request.Query.TryGetValue("search") with
                    | true, query ->
                        let query = query.ToString().ToLower()
                        db.Posts.Where(fun x ->
                            x.Title.ToLower().Contains(query) || x.Keywords.ToLower().Contains(query) || x.Description.ToLower().Contains(query)
                        )
                    | _ -> db.Posts.AsQueryable()

                let postsQuery = postsQuery.Where(fun x -> x.IsActive)

                let! posts = postsQuery.OrderByDescending(fun x -> x.CreatedTime).ToListAsync()
                let! postsCommentCount = postsQuery.Select(fun x -> { Id = x.Id; Count = x.Comments.Count }).ToListAsync()

                if posts.Count = 0 then
                    return PostList.NotFound
                else
                    return
                        posts
                        |> Seq.map (fun p ->
                            let count =
                                postsCommentCount |> Seq.tryFind (fun x -> x.Id = p.Id) |> Option.map (fun x -> x.Count) |> Option.defaultValue 0
                            p, count
                        )
                        |> Seq.map PostList.PostCard
                        |> html.fragment

            with ex ->
                logger.LogError(ex, "Create post list failed")
                return PostList.NotFound
        })
