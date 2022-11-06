// hot-reload
[<AutoOpen>]
module Slaveoftime.UI.PostDetail

open System
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db
open Slaveoftime.Services


let private postSummary (post: Post) =
    html.fragment [
        div {
            class' "flex flex-row justify-center mt-10"
            keywords post.Keywords
        }
        h1 {
            class' "text-2xl font-bold text-teal-500/90 mt-5 mx-13 text-center"
            post.Title
        }
        p {
            class' "text-teal-500/80 text-xs mt-5 text-center"
            post.CreatedTime.ToString("yyyy-MM-dd")
            CustomElement.create (fun (hook: IComponentHook, postService: PostService) ->
                hook.AddFirstAfterRenderTask(fun _ -> postService.IncreaseViewCount post.Id)
                viewCount post.ViewCount
            )
        }
    ]

let private postContent (postHtml: string) = section {
    class' "my-13 px-5 text-slate-900 dark:text-slate-100 scrollbar"
    article {
        class' "min-h-[500px] prose prose-slate dark:prose-invert prose-headings:text-teal-500/70 prose-img:rounded-md prose-img:shadow-lg prose-img:mx-auto prose-img:max-h-[400px] prose-pre:shadow-lg prose-a:text-blue-500/70 prose-blockquote:first-letter:text-3xl prose-blockquote:first-letter:text-yellow-500 max-w-max"
        html.raw postHtml
    }
}

let private postNotFound = div {
    class' "p-10 my-10 text-center text-danger-400/50 font-semibold text-2xl"
    "Not Found"
    p {
        class' "mt-3 text-danger-400/40 font-normal text-base"
        "This post may already be deleted!"
    }
}


let postDetail (postId: Guid) =
    html.inject (fun (postService: PostService, store: IShareStore) ->
        let post = postService.GetPost postId |> Task.runSynchronously

        div {
            class' "sm:w-5/6 md:w-3/4 max-w-[720px] mx-auto"
            childContent [
                match post with
                | Ok data ->
                    store.Header.Publish data.Post.Title
                    store.Keywords.Publish data.Post.Keywords

                    postSummary data.Post
                    postContent data.PostContent

                    js "
                        if (!!Prism) {
                            Prism.highlightAll();
                        } else {
                            setTimeout(Prism.highlightAll, 5000)
                        }
                    "

                | Error _ -> postNotFound
            ]
        }
    )
