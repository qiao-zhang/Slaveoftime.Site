namespace Slaveoftime.UI

open System.IO
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc.Rendering
open Microsoft.Extensions.Hosting
open Fun.Blazor


type Index() =
    inherit FunBlazorComponent()

    override _.Render() =
#if DEBUG
        html.hotReloadComp (app, "Slaveoftime.UI.Main.app")
#else
        app
#endif


    static member page(ctx: HttpContext) =
        let store, env = ctx.RequestServices.GetMultipleServices<IShareStore * IHostEnvironment>()

        let inlineStyle filePath =
#if DEBUG
            stylesheet filePath
#else
            styleElt { childContentRaw (File.ReadAllText(env.ContentRootPath </> "wwwroot" </> filePath)) }
#endif

        store.IsPrerendering.Publish true

        // Just get title and keywords for prerender and SEO
        let metas =
            adaptiview () {
                let! header' = store.Header
                title { header' }

                let! keywords' = store.Keywords
                meta {
                    name "keywords"
                    content keywords'
                }
            }

        let appEntry = rootComp<Index> ctx RenderMode.ServerPrerendered

        fragment {
            doctype "html"
            html' {
                lang "en"
                class' "bg-slate-100 dark:bg-slate-900 scrollbar"
                head {
                    baseUrl "/"
                    meta { charset "utf-8" }
                    meta {
                        name "viewport"
                        content "width=device-width, initial-scale=1.0"
                    }
                    meta {
                        name "description"
                        content "This site is my personal blogs, I will try some technology on this site when needed."
                    }
                    link {
                        rel "shortcut icon"
                        href "/favicon.ico"
                    }
                    link {
                        rel "stylesheet"
                        media "(prefers-color-scheme:light)"
                        href "https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.0.0-beta.68/dist/themes/light.css"
                    }
                    link {
                        rel "stylesheet"
                        media "(prefers-color-scheme:dark)"
                        href "https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.0.0-beta.68/dist/themes/dark.css"
                        "onload", "document.documentElement.classList.add('sl-theme-dark');"
                    }
                    inlineStyle "css/app-generated.css"
                    inlineStyle "css/prism-night-owl.css"
                    metas
                }
                body {
                    appEntry

#if HTMX
                    script { src "https://unpkg.com/htmx.org@1.8.0" }
#else
                    script { src "_framework/blazor.server.js" }
#endif

                    script { async' true; src "_content/Blazor-Analytics/blazor-analytics.js" }
                    script { async' true; src "https://cdnjs.cloudflare.com/ajax/libs/prism/1.23.0/components/prism-core.min.js" }
                    script { async' true; src "https://cdnjs.cloudflare.com/ajax/libs/prism/1.23.0/plugins/autoloader/prism-autoloader.min.js" }
                    //Comment this out, because not really used any of its control so far. But plan to use it in the future.
                    //script { async' true; type' "module"; src "https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.0.0-beta.68/dist/shoelace.js" }
                    interopScript
                    reconnectView
#if DEBUG
                    html.hotReloadJSInterop
#endif
                }
            }
        }
