namespace Slaveoftime.UI

open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc.Rendering
open Fun.Blazor
open Fun.Blazor.Router


type Index() =
    inherit FunBlazorComponent()

    override _.Render() = app


    static member page(ctx: HttpContext) =
        let store = ctx.RequestServices.GetService<IShareStore>()
        store.IsPrerendering.Publish true

        let metas =
            html.route [
                routeCif "blog/%O" (getPostDetailMeta ctx.RequestServices)
                routeAny getPostListMeta
            ]

        let root = rootComp<Index> ctx RenderMode.ServerPrerendered

        fragment {
            doctype "html"
            html' {
                class' "bg-neutral-50 dark:bg-neutral-1000 scrollbar"
                head {
                    staticHead
                    metas
                }
                body {
                    root
                    staticScript
                    interopScript
                }
            }
        }
