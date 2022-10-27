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
            "Powered by ASP.NET Core 6 & Blazor"
        }
        p {
            class' "mt-2 text-sm text-neutral-400/90"
            "2022-02-14"
        }
    ]
}


let app =
    html.inject (fun (hook: IComponentHook, store: IShareStore, js: IJSRuntime) ->
        hook.OnFirstAfterRender.Add(fun _ ->
            hook.AddDisposes [
                store.Header.AddInstantCallback(js.changeTitle >> ignore)
                store.Keywords.AddInstantCallback(js.changeKeywords >> ignore)
            ]
        )

        div {
            navbar
            html.route [ 
                routeCif "/blog/%O" postDetail
                routeAny postList
            ]
            footerSection
            html.blazor<Blazor.Analytics.Components.NavigationTracker> ()
        }
    )
