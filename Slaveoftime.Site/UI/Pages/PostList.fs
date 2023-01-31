namespace Slaveoftime.UI.Pages

open System.Linq
open Fun.Blazor
open Slaveoftime.Db
open Slaveoftime.UI.Components


type PostList =

    static member private PostCard (post: Post) = div {
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
                PostViews.ViewCount post.ViewCount
            }
            PostViews.Keywords post.Keywords
            p {
                class' "dark:text-neutral-400/90 text-neutral-600/90 mt-2 text-sm"
                post.Description
            }
        ]
    }


    static member Create () =
        html.inject (fun (db: SlaveoftimeDb) ->
            let posts = db.Posts.OrderByDescending(fun x -> x.CreatedTime).ToList()

            let node =
                div {
                    class' "sm:w-5/6 md:w-3/4 max-w-[720px] m-auto min-h-[500px]"
                    childContent [
                        if posts.Count = 0 then
                            div {
                                class' "p-10 my-10 text-center text-danger-400/50 font-semibold text-2xl"
                                "No posts are found"
                            }
                        else
                            for post in posts do
                                PostList.PostCard post
                    ]
                }

            Layout.Create(
                headerNode = fragment {
                    title { "slaveOftime blogs" }
                    headerKeywords "slaveoftime,fsharp,csharp,typescript,dotnet,frontend,backend"
                    headerDescription "This is slaveOftime blog lists, have fun"
                },
                bodyNode = node
            )
        )
