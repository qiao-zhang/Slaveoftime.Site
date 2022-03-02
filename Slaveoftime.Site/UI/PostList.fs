// hot-reload
[<AutoOpen>]
module Slaveoftime.UI.PostList

open Microsoft.JSInterop
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


[<Literal>]
let private TitleStr = "slaveOftime blogs"

[<Literal>]
let private KeywordsStr = "slaveoftime,fsharp,csharp,typescript,dotnet,frontend,backend"


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
        class' "px-6 py-5 rounded-md bg-gray-600/10 my-5"
        h2 {
            class' "text-teal-600/80 hover:text-teal-400 first-letter:text-2xl first-letter:text-teal-400 underline text-xl font-semibold"
            a {
                href $"blog/{post.Id}?title={post.Title}"
                post.Title
            }
        }
        p {
            class' "text-teal-500/60 text-2xs my-2"
            span { post.CreatedTime.ToString("yyyy-MM-dd") }
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
            class' "dark:text-neutral-400/90 text-neutral-600/90 mt-2 text-sm"
            post.Description
        }
    }


let postList =
    html.inject (fun (store: IShareStore, globalStore: IGlobalStore, hook: IComponentHook, js: IJSRuntime) ->
        // If it is for prerendering then we will do a sync call so we can get the data and fill the store now.
        if store.IsPrerendering.Value then hook.TryLoadPosts(0).Wait()

        // Below callback will happen when the browser rendered the content and SignalR connection is live.
        // Even prerender already got the title and keywords information, but after user navigate to other location those information may be changed so we will need to update again just for better user experience.
        hook.OnFirstAfterRender.Add(fun () ->
            js.changeTitle TitleStr |> ignore
            js.changeKeywords KeywordsStr |> ignore
            hook.TryLoadPosts 0 |> ignore
        )

        // Declare more fragment for better readability
        let cards =
            adaptiview () {
                match! globalStore.UsePosts 0 with
                | DeferredState.Loading -> loader
                | DeferredState.Loaded ps ->
                    for post in ps.Posts do
                        postCard post
                | _ -> html.none
            }

        div {
            class' "sm:w-5/6 md:w-3/4 max-w-[720px] m-auto min-h-[500px]"
            cards
        }
    )
