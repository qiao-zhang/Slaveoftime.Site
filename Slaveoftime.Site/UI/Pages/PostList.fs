namespace Slaveoftime.UI.Pages

open Microsoft.AspNetCore.Http
open Giraffe
open Fun.Blazor
open Fun.Htmx
open Slaveoftime.UI.Components


type PostList =

    static member Create() =
        html.inject (fun (ctx: IHttpContextAccessor) ->
            let ctx = ctx.HttpContext

            let node = div {
                class' "sm:w-5/6 md:w-3/4 max-w-[720px] m-auto min-h-[500px]"
                childContent [
                    div {
                        class' "flex justify-center mt-5"
                        div {
                            class' "input-group input-group-sm w-auto"
                            childContent [
                                input {
                                    type' InputTypes.text
                                    name "search"
                                    value (ctx.TryGetQueryStringValue("search") |> Option.defaultValue "")
                                    placeholder "Search by title, keywords and description"
                                    hxTrigger'(hxEvt.keyboard.keyup, changed = true, delayMs = 500)
                                    hxGet "/view/post-list"
                                    hxTarget "#post-list"
                                    hxIndicator ".htmx-indicator"
                                    class' "input input-bordered"
                                }
                                button {
                                    class' "btn btn-square"
                                    childContent [
                                        Static.html """
                                            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
                                        """
                                    ]
                                }
                            ]
                        }
                    }
                    progress { class' "progress progress-primary h-2 htmx-indicator" }
                    section {
                        id "post-list"
                        PostViews.PostList()
                    }
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
