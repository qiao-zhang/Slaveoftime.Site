[<AutoOpen>]
module Slaveoftime.UI.Main

open Fun.Blazor
open Fun.Blazor.Router


let staticHead =
    // In VSCode with plugin: "Highlight HTML/SQL templates in F#" we can get hightlight and intellicense for below code
    // In Fun.Blazor V2 this is very efficient way to build static html fragments because there is only one call hanpens under the hood.
    // And even in csharp razor engine, the generated code will call exactly the same method.
    // So it is the best if we can keep static fragment in this way if you are using VSCode.
    Template.html $"""
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        <base href="/" />
        <link rel="stylesheet" media="(prefers-color-scheme:light)" href="https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.0.0-beta.68/dist/themes/light.css">
        <link rel="stylesheet" media="(prefers-color-scheme:dark)" href="https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.0.0-beta.68/dist/themes/dark.css"
            onload="document.documentElement.classList.add('sl-theme-dark');">
        <link href="css/app-generated.css" rel="stylesheet">
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
    div {
        navbar
        routes
        footerSection
    }
