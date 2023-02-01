namespace Slaveoftime.UI.Pages

open System
open System.IO
open System.Linq
open Microsoft.Extensions.Caching.Memory
open Fun.Result
open Fun.Blazor
open Fun.Blazor.Operators
open Slaveoftime.Db
open Slaveoftime.UI.Components


type PostDetail =

    static member private PostSummary(post: Post) =
        html.fragment [
            div {
                class' "flex flex-row justify-center mt-10"
                PostViews.Keywords post.Keywords
            }
            h1 {
                class' "text-2xl font-bold text-teal-500/90 mt-5 mx-13 text-center"
                post.Title
            }
            section {
                class' "text-teal-500/80 text-xs mt-5 text-center flex items-center justify-center gap-2"
                childContent [
                    html.text (post.CreatedTime.ToString("yyyy-MM-dd"))
                    html.customElement<ViewCount> (
                        preRender = true,
                        attrs =
                            ((nameof Unchecked.defaultof<ViewCount>.post_id => post.Id.ToString())
                             ==> (nameof Unchecked.defaultof<ViewCount>.count => post.ViewCount))
                    )
                    PostViews.LiksView post.Likes
                ]
            }
        ]

    static member private PostContent(node: NodeRenderFragment) = section {
        class' "my-10 px-5 text-slate-900 dark:text-slate-100"
        article {
            class'
                "min-h-[500px] prose prose-slate dark:prose-invert prose-headings:text-teal-500/70 prose-img:rounded-md prose-img:shadow-lg prose-img:mx-auto prose-img:max-h-[400px] prose-pre:shadow-lg prose-a:text-blue-500/70 prose-blockquote:first-letter:text-3xl prose-blockquote:first-letter:text-yellow-500 max-w-max"
            node
        }
    }

    static member private PostNotFound = div {
        class' "p-10 my-10 text-center text-danger-400/50 font-semibold text-2xl"
        "Not Found"
        p {
            class' "mt-3 text-danger-400/40 font-normal text-base"
            "This post may already be deleted!"
        }
    }

    static member Create(post: Post) =
        html.inject (fun (memoryCache: IMemoryCache) ->
            Layout.Create(
                headerNode = fragment {
                    title { post.Title }
                    headerKeywords post.Keywords
                    headerDescription post.Description
                },
                bodyNode = div {
                    class' "sm:w-5/6 md:w-3/4 max-w-[720px] mx-auto post-detail"
                    childContent [
                        PostDetail.PostSummary post
                        PostDetail.PostContent(
                            try
                                if post.IsDynamic then
                                    match memoryCache.TryGetValue<NodeRenderFragment> $"post-dynamic-{post.Id}" with
                                    | true, node -> node
                                    | _ -> PostDetail.PostNotFound
                                else
                                    html.raw (File.ReadAllText post.ContentPath)
                            with _ ->
                                PostDetail.PostNotFound
                        )
                        html.customElement<LikesSurvey> (attrs = (nameof Unchecked.defaultof<LikesSurvey>.post_id => post.Id.ToString()))
                        script { src "zoom.js" }
                        script { src "https://cdnjs.cloudflare.com/ajax/libs/prism/1.23.0/components/prism-core.min.js" }
                        script { src "https://cdnjs.cloudflare.com/ajax/libs/prism/1.23.0/plugins/autoloader/prism-autoloader.min.js" }
                        js
                            "
                            Prism.highlightAll();
                            Zoom('.post-detail img')
                            "
                    ]
                }
            )
        )


    static member Create(postId: Guid) =
        html.inject (fun (db: SlaveoftimeDb) ->
            let post = db.Posts.FirstOrDefault(fun x -> x.Id = postId) |> Option.ofObj

            match post with
            | Some post -> PostDetail.Create post
            | None -> Layout.Create(bodyNode = PostDetail.PostNotFound)
        )


    static member Create(postSlug: string) =
        html.inject (fun (db: SlaveoftimeDb) ->
            let post = db.Posts.FirstOrDefault(fun x -> x.Slug = postSlug) |> Option.ofObj

            match post with
            | Some post -> PostDetail.Create post
            | None -> Layout.Create(bodyNode = PostDetail.PostNotFound)
        )
