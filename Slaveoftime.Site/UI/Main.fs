// hot-reload
[<AutoOpen>]
module Slaveoftime.UI.Main

open Fun.Blazor
open Fun.Blazor.Router
open Microsoft.JSInterop



let private footerSection = footer {
    class' "flex flex-col justify-center items-center py-10"
    childContent [
        p {
            class' "text-sm text-neutral-400/90"
            "albertwoo_slaveoftime@hotmail.com"
        }
        p {
            class' "mt-2 text-sm text-teal-500/80 font-semibold"
#if HTMX
            "Powered by ASP.NET Core 6 & HTMX"
#else
            "Powered by ASP.NET Core 6 & Blazor"
#endif
        }
        p {
            class' "mt-2 text-sm text-neutral-400/90"
            "2022-10-31"
        }
    ]
}


let app =
    html.inject (fun (hook: IComponentHook, store: IShareStore, jsRunTime: IJSRuntime) ->
        hook.OnFirstAfterRender.Add(fun _ ->
            hook.AddDisposes [
                store.Header.AddInstantCallback(jsRunTime.changeTitle >> ignore)
                store.Keywords.AddInstantCallback(jsRunTime.changeKeywords >> ignore)
            ]
        )

        div {
            navbar
            html.route [ 
                routeCif "/blog/%O" postDetail
                routeAny postList
            ]
            footerSection

#if HTMX
            html.raw
                """
                <!-- Google tag (gtag.js) -->
                <script async src="https://www.googletagmanager.com/gtag/js?id=UA-147730361-1"></script>
                <script>
                  window.dataLayer = window.dataLayer || [];
                  function gtag(){dataLayer.push(arguments);}
                  gtag('js', new Date());

                  gtag('config', 'UA-147730361-1');
                </script>
                """
#else
            html.blazor<Blazor.Analytics.Components.NavigationTracker> ()
#endif
        }
    )
