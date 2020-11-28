[<AutoOpen>]
module Slaveoftime.UI.PostList

open FSharp.Data.Adaptive
open Microsoft.JSInterop
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


[<Literal>]
let private TitleStr = "Blogs"

[<Literal>]
let private KeywordsStr = "slaveoftime,fsharp.dotnet"


let getPostListMeta =
    fragment {
        title { TitleStr }
        meta {
            name "keywords"
            value KeywordsStr
        }
    }


let private postCard (post: Post) =
    div {
        class' "p-8 rounded-md bg-gray-600/10 my-5"
        h2 {
            class' "text-purple-500/80 first-letter:text-2xl first-letter:text-yellow-500 underline text-xl font-semibold"
            a {
                href $"blog/{post.Id}?title={post.Title}"
                post.Title
            }
        }
        p {
            class' "text-purple-500/50 text-2xs my-2"
            span {
                post.UpdatedTime.ToString("yyyy-MM-dd HH:mm:ss")
            }
            span {
                class' "pl-3"
                post.ViewCount
            }
            span {
                class' "pl-3 font-semibold"
                post.Author
            }
        }
        keywords post.Keywords
        p {
            class' "text-neutral-400/90 mt-2 text-sm"
            post.Description
        }
    }


let postList =
    html.inject (fun (store: IShareStore, hook: IComponentHook, js: IJSRuntime) ->
        let posts = cval DeferredState.Loading

        let loadPosts () =
            task {
                let! result = hook.LoadPosts()
                posts.Publish(DeferredState.ofResult result)
            }

        if store.IsPrerendering.Value then loadPosts().Wait()


        hook.OnFirstAfterRender.Add(fun () ->
            js.changeTitle TitleStr |> ignore
            js.changeKeywords KeywordsStr |> ignore

            match posts.Value with
            | DeferredState.Loaded x when x.Length > 0 -> ()
            | _ -> loadPosts () |> ignore
        )


        let cards =
            adaptiview () {
                match! posts with
                | DeferredState.Loading -> loader
                | DeferredState.Loaded ps ->
                    for post in ps do
                        postCard post
                | _ -> html.none
            }


        div {
            class' "sm:w-5/6 md:w-3/4 max-w-[720px] m-auto min-h-[500px]"
            cards
        }
    )
