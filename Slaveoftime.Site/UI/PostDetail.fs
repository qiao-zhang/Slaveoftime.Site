[<AutoOpen>]
module Slaveoftime.UI.PostDetail

open System
open System.Linq
open FSharp.Data.Adaptive
open Microsoft.Extensions.DependencyInjection
open Microsoft.JSInterop
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


let getPostDetailMeta (sp: IServiceProvider) (postId: Guid) =
    let db = sp.GetService<SlaveoftimeDb>()
    let post = db.Posts.FirstOrDefault(fun x -> x.Id = postId)

    if box post = null then
        html.none
    else
        fragment {
            title { post.Title }
            meta {
                name "keywords"
                value post.Keywords
            }
        }


let private postSummary (post: Post) =
    fragment {
        h1 {
            class' "text-2xl font-bold text-purple-500/80 mt-10 mx-13 text-center"
            post.Title
        }
        p {
            class' "text-purple-500/50 text-xs mt-5 text-center"
            post.UpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")
        }
        div {
            class' "flex flex-row justify-center mt-2"
            keywords post.Keywords
        }
    }

let private postContent (postHtml: string) =
    section {
        class' "my-13 px-5"
        article {
            class' "markdown-body min-h-[500px]"
            html.raw postHtml
        }
    }

let private postNotFound =
    div {
        class' "p-10 my-10 text-center text-danger-400/50 font-semibold text-2xl"
        "Not Found"
        p {
            class' "mt-3 text-danger-400/40 font-normal text-base"
            "This post may already be deleted!"
        }
    }


let postDetail (postId: Guid) =
    html.inject (fun (hook: IComponentHook, store: IShareStore, js: IJSRuntime) ->
        let post = cval DeferredState.Loading

        let loadPost () =
            task {
                match! hook.LoadPost postId with
                | Ok (p, html) ->
                    post.Publish(DeferredState.Loaded(p, html))
                    js.changeTitle p.Title |> ignore
                    js.changeKeywords p.Keywords |> ignore
                    js.highlightCode () |> ignore
                | _ ->
                    post.Publish(DeferredState.LoadFailed "Cannot find post")
            }


        if store.IsPrerendering.Value then loadPost().Wait()

        hook.OnFirstAfterRender.Add(fun () ->
            hook.IncreaseViewCount postId |> ignore
            if post.Value.Value.IsNone then loadPost () |> ignore
        )


        let detail =
            adaptiview () {
                match! post with
                | DeferredState.Loaded (post, postHtml) ->
                    postSummary post
                    postContent postHtml
                | DeferredState.Loading -> loader
                | _ -> postNotFound
            }


        div {
            class' "sm:w-5/6 md:w-3/4 max-w-[720px] mx-auto"
            detail
        }
    )
