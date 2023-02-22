namespace Slaveoftime.UI.Components.PostView

open System
open System.IO
open System.Linq
open Microsoft.Extensions.Caching.Memory
open Fun.Htmx
open Fun.Result
open Fun.Blazor
open Fun.Blazor.Operators
open Slaveoftime
open Slaveoftime.Db
open Slaveoftime.UI.Components

type PostDetail =

    static member private PostSummary(post: Post) =
        html.fragment [
            div {
                class' "flex flex-wrap items-center gap-2 justify-center mt-10"
                PostView.Keywords post.Keywords
            }
            h1 {
                class' "text-2xl font-bold text-teal-500/90 mt-5 mx-13 text-center"
                post.Title
            }
            section {
                class' "text-teal-6 text-xs mt-5 text-center flex items-center justify-center gap-2"
                childContent [
                    html.text (post.CreatedTime.ToString("yyyy-MM-dd"))
                    html.customElement<PostViewCount> (
                        delayMs = 5_000,
                        attrs =
                            ((nameof Unchecked.defaultof<PostViewCount>.post_id => post.Id.ToString())
                             ==> (nameof Unchecked.defaultof<PostViewCount>.count => post.ViewCount))
                    )
                    if post.Likes > 0 then PostView.LiksView post.Likes
                ]
            }
        //if String.IsNullOrEmpty post.MainImage |> not then
        //    img {
        //        class' "mx-auto object-fill object-center mt-5"
        //        src (host <//> "blog" <//> post.MainImage)
        //    }
        ]

    static member private PostContent(post: Post) =
        html.inject (fun (memoryCache: IMemoryCache) ->
            let node =
                try
                    if post.IsDynamic then
                        match memoryCache.TryGetValue<NodeRenderFragment> $"post-dynamic-{post.Id}" with
                        | true, node -> node
                        | _ -> PostDetail.PostNotFound
                    else
                        html.raw (File.ReadAllText post.ContentPath)
                with _ ->
                    PostDetail.PostNotFound

            section {
                class' "my-10 px-5 text-slate-900 dark:text-slate-100"
                article {
                    class'
                        "min-h-[500px] prose prose-slate dark:prose-invert prose-headings:text-teal-500/70 prose-img:rounded-md prose-img:shadow-lg prose-img:mx-auto prose-img:max-h-[400px] prose-pre:shadow-lg prose-a:text-blue-500/70 prose-blockquote:first-letter:text-3xl prose-blockquote:first-letter:text-yellow-500 prose-code:text-sm prose-code:leading-none max-w-full"
                    node
                }
            }
        )

    static member private Scripts = fragment {
        script { src $"{host}/zoom.js" }
        script { src "https://cdnjs.cloudflare.com/ajax/libs/prism/1.23.0/components/prism-core.min.js" }
        script { src "https://cdnjs.cloudflare.com/ajax/libs/prism/1.23.0/plugins/autoloader/prism-autoloader.min.js" }
        script { src "https://cdn.jsdelivr.net/npm/vanilla-lazyload@17.8.3/dist/lazyload.min.js" }
        js
            """
            Prism.highlightAll();
            Zoom('.post-detail img')

            document.querySelectorAll("#post-detail img").forEach(item => {
                if (item.className.indexOf("lazy") < 0) {
                    const dotIndex = item.src.lastIndexOf(".")
                    item.setAttribute("old-src", item.src)
                    item.setAttribute("data-src", item.src.substr(0, dotIndex) + "-original" + item.src.substr(dotIndex))
                    item.classList.add("lazy")
                }
            })
            new LazyLoad({
                callback_error: img => {
                    img.setAttribute("src", img.attributes["old-src"].value);
                }
            }).update()
            """
    }

    static member PostNotFound = div {
        class' "p-10 my-10 text-center text-danger-400/50 font-semibold text-2xl"
        "Not Found"
        p {
            class' "mt-3 text-danger-400/40 font-normal text-base"
            "This post may already be deleted!"
        }
    }

    static member Create(post: Post) = div {
        id "post-detail"
        class' "sm:w-5/6 md:w-3/4 max-w-[720px] mx-auto post-detail"
        childContent [
            PostDetail.PostSummary post
            PostDetail.PostContent post
            div { class' "divider" }
            html.customElement<PostLikesSurvey> (delayMs = 5_000, attrs = (nameof Unchecked.defaultof<PostLikesSurvey>.post_id => post.Id.ToString()))
            div { class' "divider" }
            div {
                class' "my-2"
                hxTrigger' (hxEvt.load, delayMs = 2000)
                hxSwap_outerHTML
                hxGet $"view/post/{post.Id}/comment"
            }
            PostDetail.Scripts
        ]
    }


    static member Create(postId: Guid) =
        html.inject (fun (db: SlaveoftimeDb) ->
            let post = db.Posts.FirstOrDefault(fun x -> x.IsActive && x.Id = postId) |> Option.ofObj

            match post with
            | Some post -> PostDetail.Create post
            | None -> PostDetail.PostNotFound
        )


    static member CreateForFeed(postId: Guid) =
        html.inject (fun (db: SlaveoftimeDb) ->
            let post = db.Posts.FirstOrDefault(fun x -> x.IsActive && x.Id = postId) |> Option.ofObj

            match post with
            | Some post -> div {
                id "post-detail"
                class' "sm:w-5/6 md:w-3/4 max-w-[720px] mx-auto post-detail"
                childContent [ PostDetail.PostContent post; stylesheet $"{host}/css/tailwind-generated.css"; PostDetail.Scripts ]
              }

            | None -> PostDetail.PostNotFound
        )
