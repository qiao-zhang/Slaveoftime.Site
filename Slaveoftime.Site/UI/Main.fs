[<AutoOpen>]
module Slaveoftime.UI.Main

open Fun.Blazor
open Fun.Blazor.Router


let staticHead =
    Template.html $"""
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <base href="/" />
        <link rel="stylesheet" media="(prefers-color-scheme:light)" href="https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.0.0-beta.68/dist/themes/light.css">
        <link rel="stylesheet" media="(prefers-color-scheme:dark)" href="https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.0.0-beta.68/dist/themes/dark.css"
            onload="document.documentElement.classList.add('sl-theme-dark');">
        <link href="css/app-generated.css" rel="stylesheet">
        <link href="css/github-markdown.css" rel="stylesheet">
        <link href="css/prism-night-owl.css" rel="stylesheet">
    """


let staticScript =
    Template.html $"""
        <script type="module" src="https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.0.0-beta.68/dist/shoelace.js"></script>
        <script src="_framework/blazor.server.js"></script>
        <script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.23.0/components/prism-core.min.js"></script>
        <script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.23.0/plugins/autoloader/prism-autoloader.min.js"></script>
    """


let routes = 
    html.route [
        routeCif "/blog/%O" postDetail
        routeAny postList
    ]


let app =
    html.inject (fun (store: IShareStore, hook: IComponentHook) ->
        hook.OnFirstAfterRender.Add(fun () -> store.IsPrerendering.Publish(false))

        div {
            navbar
            routes
            footerSection
        }
    )
