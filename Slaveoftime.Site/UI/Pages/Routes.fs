module Slaveoftime.UI.Pages.Routes

open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open Giraffe
open Fun.Blazor


let cacheFor30Days: HttpHandler = publicResponseCaching (60 * 60 * 24 * 30) None


type View =

    static member inline Build(node: NodeRenderFragment) : HttpHandler =
        fun nxt ctx -> task {
            do! ctx.WriteFunDom(node)
            return! nxt ctx
        }

    static member inline Build(render: HttpContext -> NodeRenderFragment) : HttpHandler =
        fun nxt ctx -> task {
            let node = render ctx
            return! View.Build (node) nxt ctx
        }

    static member inline Build(render: unit -> NodeRenderFragment) : HttpHandler =
        fun nxt ctx -> task {
            let node = render ()
            return! View.Build (node) nxt ctx
        }


let uiRoutes: HttpHandler =
    choose [
        routeCif "/blog/%O" (fun (id: Guid) -> View.Build(PostDetail.Create id))
        routeCif "/blog/%s" (fun slug -> View.Build(PostDetail.Create slug))
        routeCi "/" >=> View.Build PostList.Create
    ]
