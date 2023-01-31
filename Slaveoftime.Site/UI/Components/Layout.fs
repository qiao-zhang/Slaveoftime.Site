﻿namespace Slaveoftime.UI.Components

open System.IO
open Fun.Blazor
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open Slaveoftime.UI.Components


type Layout =

    static member Create(?headerNode: NodeRenderFragment, ?bodyNode: NodeRenderFragment) =
        html.inject (fun (httpContextAccessor: IHttpContextAccessor) ->
            let ctx = httpContextAccessor.HttpContext
            let env = ctx.RequestServices.GetMultipleServices<IHostEnvironment>()

            let inlineStyle filePath = styleElt { childContentRaw (File.ReadAllText(env.ContentRootPath </> "wwwroot" </> filePath)) }

            let headerNode =
                defaultArg
                    headerNode
                    (fragment {
                        title { "GiraffeHtmxBlazor" }
                        headerKeywords "slaveoftime,blog,dotnet,fsharp"
                        headerDescription "This site is my personal blogs, I will try some technology on this site when needed."
                    })

            html.fragment [
                doctype "html"
                html' {
                    class' "bg-slate-100 dark:bg-slate-900"
                    head {
                        chartsetUTF8
                        baseUrl "/"
                        viewport "width=device-width, initial-scale=1.0"
                        link {
                            rel "shortcut icon"
                            href "/favicon.ico"
                        }
                        inlineStyle "css/tailwind-generated.css"
                        inlineStyle "css/prism-night-owl.css"
                        CustomElement.lazyBlazorJs ()
                        headerNode
                    }
                    body {
                        Navbar.Create()
                        defaultArg bodyNode html.none
                        Footer.Create()
                        // script { src "https://unpkg.com/htmx.org@1.8.5" }
                    }
                }
            ]
        )