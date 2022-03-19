// hot-reload
[<AutoOpen>]
module Slaveoftime.UI.PostDetail

open System
open System.Linq
open Microsoft.Extensions.DependencyInjection
open Microsoft.JSInterop
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


let getPostDetailMeta (sp: IServiceProvider) (postId: Guid) =
    let db = sp.GetService<SlaveoftimeDb>()
    let post = db.Posts.FirstOrDefault(fun x -> x.Id = postId)

    if post = null then
        title { "slaveOftime blogs" }
    else
        let titleStr = post.Title
        let keywords = post.Keywords
        html.fragment [
            title { titleStr }
            meta {
                name "keywords"
                value keywords
            }
        ]


let private postSummary (post: Post) =
    let title = post.Title
    let keywords = keywords post.Keywords
    let createdTime = post.CreatedTime.ToString("yyyy-MM-dd")

    html.fragment [
        div {
            class' "flex flex-row justify-center mt-10"
            keywords
        }
        h1 {
            class' "text-2xl font-bold text-teal-500/90 mt-5 mx-13 text-center"
            title
        }
        p {
            class' "text-teal-500/80 text-xs mt-5 text-center"
            createdTime
        }
    ]

let private postContent (postHtml: string) =
    section {
        class' "my-13 px-5 text-slate-900 dark:text-slate-100 scrollbar"
        article {
            class' "min-h-[500px] prose prose-slate dark:prose-invert prose-headings:text-teal-500/70 prose-img:rounded-md prose-img:shadow-lg prose-img:mx-auto prose-img:max-h-[400px] prose-pre:shadow-lg prose-a:text-blue-500/70 prose-blockquote:first-letter:text-3xl prose-blockquote:first-letter:text-yellow-500 "
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
    html.inject (fun (hook: IComponentHook, globalStore: IGlobalStore, store: IShareStore, js: IJSRuntime) ->
        let postStore = globalStore.UsePost postId


        if store.IsPrerendering.Value then hook.TryLoadPost(postId).Wait()

        hook.OnFirstAfterRender.Add(fun () ->
            hook.IncreaseViewCount postId |> ignore
            hook.TryLoadPost postId |> ignore

            hook.AddDisposes [
                // Use InstantCallback so we can trigger a call immediatly, because the postStore may already cached so it will not load again, and lazy callback will not be triggered.
                postStore.AddInstantCallback(
                    function
                    | DeferredState.Loaded data ->
                        js.changeTitle data.Post.Title |> ignore
                        js.changeKeywords data.Post.Keywords |> ignore
                        js.highlightCode () |> ignore
                    | _ -> ()
                )
            ]
        )


        let detail =
            adaptiview () {
                match! postStore with
                | DeferredState.NotStartYet -> html.none
                | DeferredState.Loaded data ->
                    postSummary data.Post
                    postContent data.PostContent
                | DeferredState.Loading -> loader
                | _ -> postNotFound
            }


        div {
            class' "sm:w-5/6 md:w-3/4 max-w-[720px] mx-auto"
            detail
        }
    )
