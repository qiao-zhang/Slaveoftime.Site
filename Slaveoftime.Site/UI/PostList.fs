// hot-reload
[<AutoOpen>]
module Slaveoftime.UI.PostList

open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


let keywords (keywords: string) =
    div.create [
        for keyword in keywords.Split [| ','; ';' |] do
            span {
                class' "text-xs px-3 py-1 rounded-full mr-2 dark:bg-teal-100/20 bg-teal-200/20 dark:text-neutral-100/80 text-neutral-700/90"
                keyword
            }
    ]


let private postCard (post: Post) = div {
    class' "px-6 py-5 rounded-md bg-gray-600/10 my-5"
    childContent [
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
                class' "pl-3 font-semibold"
                post.Author
            }
            viewCount post.ViewCount
        }
        keywords post.Keywords
        p {
            class' "dark:text-neutral-400/90 text-neutral-600/90 mt-2 text-sm"
            post.Description
        }
    ]
}


let postList =
    html.inject (fun (store: IShareStore, hook: IComponentHook) ->
        store.Header.Publish "slaveOftime blogs"
        store.Keywords.Publish "slaveoftime,fsharp,csharp,typescript,dotnet,frontend,backend"

        // Declare more fragment for better readability
        let cards =
            adaptiview () {
                match! hook.GetPosts() with
                | LoadingState.Reloading posts
                | LoadingState.Loaded posts ->
                    if posts.Length = 0 then
                        div { 
                            class' "p-10 my-10 text-center text-danger-400/50 font-semibold text-2xl"
                            "No posts are found"
                        }
                    else
                        for post in posts do
                            postCard post
                | _ -> loader
            }

        div {
            class' "sm:w-5/6 md:w-3/4 max-w-[720px] m-auto min-h-[500px]"
            cards
        }
    )
