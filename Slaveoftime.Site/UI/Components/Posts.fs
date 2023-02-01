#nowarn "3535"

namespace Slaveoftime.UI.Components

open System
open System.Linq
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Components
open FSharp.Data.Adaptive
open Giraffe
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


[<RequireQualifiedAccess>]
type PostType =
    | Static of relativePath: string
    | Dynamic

[<CLIMutable>]
type PostMeta = {
    Id: Guid
    Title: string
    Keywords: string
    Description: string
    CreateTime: DateTime
}


type IDynamicPost =
    static abstract member Meta: PostMeta
    static abstract member View: NodeRenderFragment


type PostViews =

    static member Keywords(keywords: string) =
        div.create [
            for keyword in keywords.Split [| ','; ';' |] do
                span {
                    class' "text-xs px-3 py-1 rounded-full mr-2 dark:bg-teal-100/20 bg-teal-200/20 dark:text-neutral-100/80 text-neutral-700/90"
                    keyword
                }
        ]

    static member ViewCount(count: int) = span {
        class' "rounded-lg dark:text-neutral-100/90 text-neutral-700/90 bg-teal-500/40 px-2 text-sm"
        $"View {count}"
    }

    static member LiksView(likes: int) = span {
        class' "rounded-lg dark:text-neutral-100/90 text-neutral-700/90 bg-teal-500/40 px-2 text-sm"
        $"Likes {likes}"
    }

    
    static member PostCard(post: Post) = div {
        class' "px-6 py-5 rounded-md bg-gray-600/10 my-5"
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
                class' "text-teal-500/60 text-xs my-2 flex items-center gap-2"
                childContent [
                    span { post.CreatedTime.ToString("yyyy-MM-dd") }
                    span {
                        class' "font-semibold"
                        post.Author
                    }
                    PostViews.ViewCount post.ViewCount
                    PostViews.LiksView post.Likes
                ]
            }
            PostViews.Keywords post.Keywords
            p {
                class' "dark:text-neutral-400/90 text-neutral-600/90 mt-2 text-sm"
                post.Description
            }
        ]
    }

    static member PostList() = 
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
                |> Seq.map PostViews.PostCard
                |> html.fragment
        )


[<FunBlazorCustomElement>]
type ViewCount() =
    inherit FunBlazorComponent()

    [<Parameter>]
    member val post_id = "" with get, set

    [<Parameter>]
    member val count = 0 with get, set

    override this.Render() =
        html.inject (fun (db: SlaveoftimeDb, hook: IComponentHook) ->
            hook.AddFirstAfterRenderTask(fun _ -> task {
                match Guid.TryParse(this.post_id) with
                | true, postId ->
                    do!
                        db.Posts
                            .Where(fun x -> x.Id = postId)
                            .ExecuteUpdateAsync(fun setter -> setter.SetProperty((fun x -> x.ViewCount), (fun x -> x.ViewCount + 1)))
                        |> Task.map ignore
                | _ -> ()
            })

            PostViews.ViewCount(this.count)
        )


[<FunBlazorCustomElement>]
type LikesSurvey() =
    inherit FunBlazorComponent()

    [<Parameter>]
    member val post_id = "" with get, set

    override this.Render() =
        html.inject (fun (db: SlaveoftimeDb) ->
            let isClicked = cval false

            adaptiview () {
                let! isClicked, setIsClicked = isClicked.WithSetter()
                match Guid.TryParse(this.post_id) with
                | true, postId when not isClicked -> section {
                    class' "flex items-center justify-center gap-3"
                    childContent [
                        p { "Do you like this post?" }
                        button {
                            onclick (fun _ ->
                                setIsClicked true
                                db.Posts
                                    .Where(fun x -> x.Id = postId)
                                    .ExecuteUpdateAsync(fun setter -> setter.SetProperty((fun x -> x.Likes), (fun x -> x.Likes + 1)))
                                |> Task.map ignore
                            )
                            class' "btn btn-sm btn-success"
                            "Yes"
                        }
                        button {
                            onclick (fun _ ->
                                setIsClicked true
                                db.Posts
                                    .Where(fun x -> x.Id = postId)
                                    .ExecuteUpdateAsync(fun setter -> setter.SetProperty((fun x -> x.DisLikes), (fun x -> x.DisLikes + 1)))
                                |> Task.map ignore
                            )
                            class' "btn btn-sm btn-outline"
                            "No"
                        }
                    ]
                  }

                | _ -> html.none
            }
        )
