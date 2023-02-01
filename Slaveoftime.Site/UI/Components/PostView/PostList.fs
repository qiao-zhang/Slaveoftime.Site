namespace Slaveoftime.UI.Components.PostView

open System.Linq
open Microsoft.AspNetCore.Http
open FSharp.Data.Adaptive
open Giraffe
open Fun.Blazor
open Slaveoftime.Db

type PostList =
    
    static member private PostCard (post: Post) = div {
        class' "px-6 py-5 rounded-md bg-gray-600/10 mb-5"
        childContent [
            h2 {
                class' "text-teal-600/80 hover:text-teal-400 first-letter:text-2xl first-letter:text-teal-400 underline text-xl font-semibold"
                a {
                    //href $"blog/{post.Id}?title={post.Title}"
                    href $"blog/{post.Slug}" // To use slug we have to make sure our title is unique
                    post.Title
                }
            }
            p {
                class' "text-teal-500/80 text-xs my-2 flex items-center gap-2"
                childContent [
                    span { post.CreatedTime.ToString("yyyy-MM-dd") }
                    span {
                        class' "font-semibold"
                        post.Author
                    }
                    PostView.ViewCount post.ViewCount
                    PostView.LiksView post.Likes
                ]
            }
            PostView.Keywords post.Keywords
            p {
                class' "dark:text-neutral-400/90 text-neutral-600/90 mt-2 text-sm"
                post.Description
            }
        ]
    }

    static member Create() = 
        html.inject (fun (db: SlaveoftimeDb, ctx: IHttpContextAccessor) ->
            let ctx = ctx.HttpContext

            let postsQuery =
                match ctx.TryGetQueryStringValue("search") with
                | Some query ->
                    let query = query.ToLower()
                    db.Posts
                        .Where(fun x -> 
                            x.Title.ToLower().Contains(query) 
                            || x.Keywords.ToLower().Contains(query) 
                            || x.Description.ToLower().Contains(query)
                        )

                | _ -> db.Posts

            let posts = postsQuery.OrderByDescending(fun x -> x.CreatedTime).ToList()

            if posts.Count = 0 then
                div {
                    class' "p-10 my-10 text-center text-danger-400/50 font-semibold text-2xl"
                    "No posts are found"
                }
            else
                posts
                |> Seq.map PostList.PostCard
                |> html.fragment
        )
