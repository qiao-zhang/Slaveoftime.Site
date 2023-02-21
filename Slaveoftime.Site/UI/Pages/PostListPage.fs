namespace Slaveoftime.UI.Pages

open System
open Microsoft.AspNetCore.Http
open Fun.Blazor
open Fun.Htmx
open Slaveoftime
open Slaveoftime.UI.Components
open Slaveoftime.UI.Components.PostView

type PostListPage =

    static member Create() =
        html.inject (fun (ctx: IHttpContextAccessor) ->
            let ctx = ctx.HttpContext
            let searchQuery =
                match ctx.Request.Query.TryGetValue("search") with
                | true, q -> q.ToString()
                | _ -> ""

            let node = div {
                class' "sm:w-5/6 md:w-3/4 max-w-[720px] m-auto min-h-[500px]"
                childContent [
                    div {
                        class' "input-group input-group-xs mt-5 px-5"
                        childContent [
                            input {
                                type' InputTypes.text
                                name "search"
                                value searchQuery
                                placeholder "Search by title, keywords and description"
                                hxTrigger' (hxEvt.keyboard.keyup, changed = true, delayMs = 500)
                                hxGet "/view/post-list"
                                hxTarget "#post-list"
                                hxIndicator ".htmx-indicator"
                                class' "input input-bordered w-full input-sm"
                            }
                            button {
                                class' "btn btn-square btn-sm"
                                childContent [
                                    Static.html
                                        """
                                        <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"></path></svg>
                                        """
                                ]
                            }
                        ]
                    }
                    progress { class' "progress progress-primary h-1 htmx-indicator" }
                    section {
                        id "post-list"
                        PostList.Create()
                    }
                ]
            }

            Layout.Create(
                headerNode = fragment {
                    headerTitle siteTitle
                    headerKeywords (String.Join(",", siteKeywords))
                    headerDescription siteDescription
                    meta {
                        name "twitter:card"
                        content "summary"
                    }
                },
                bodyNode = node
            )
        )
