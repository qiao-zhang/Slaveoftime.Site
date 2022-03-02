namespace Slaveoftime.UI

open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc.Rendering
open Fun.Blazor
open Fun.Blazor.Router


type Index() =
    inherit FunBlazorComponent()

    override _.Render() =
#if DEBUG
        html.hotReloadComp (app, "Slaveoftime.UI.Main.app")
#else
        app
#endif


    static member page(ctx: HttpContext) =
        let store = ctx.RequestServices.GetService<IShareStore>()
        store.IsPrerendering.Publish true

        // Just get title and keywords for prerender and SEO
        let metas =
            html.route [
                routeCif "blog/%O" (getPostDetailMeta ctx.RequestServices)
                routeAny getPostListMeta
            ]

        let root = rootComp<Index> ctx RenderMode.ServerPrerendered

        fragment {
            doctype "html"
            html' {
                class' "bg-slate-100 dark:bg-slate-900 scrollbar"
                head {
                    staticHead
                    metas
                }
                body {
                    root
                    staticScript
                    interopScript
#if DEBUG
                    html.hotReloadJSInterop
#endif                
                }
            }
        }
