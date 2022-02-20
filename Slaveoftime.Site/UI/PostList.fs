[<AutoOpen>]
module Slaveoftime.UI.PostList

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
    let url = $"blog/{post.Id}?title={post.Title}"
    let title = post.Title
    let viewCount = post.ViewCount
    let author = post.Author
    let description = post.Description
    let createdTime = post.CreatedTime.ToString("yyyy-MM-dd")
    let keywords = keywords post.Keywords

    // To make the whole CE block can be inlined, we need to make sure all its reference is in local scope 
    div {
        class' "p-8 rounded-md bg-gray-600/10 my-5"
        h2 {
            class' "text-purple-500/80 first-letter:text-2xl first-letter:text-yellow-500 underline text-xl font-semibold"
            a {
                href url
                title
            }
        }
        p {
            class' "text-purple-500/50 text-2xs my-2"
            span { createdTime }
            span {
                class' "pl-3"
                viewCount
            }
            span {
                class' "pl-3 font-semibold"
                author
            }
        }
        keywords
        p {
            class' "text-neutral-400/90 mt-2 text-sm"
            description
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
