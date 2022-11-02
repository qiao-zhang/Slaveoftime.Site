[<AutoOpen>]
module Slaveoftime.UI.JsInterop

open Microsoft.JSInterop
open Fun.Blazor


let private changeHeadJs =
    js """
        window.changeTitle = (title) => {
            document.title = title
        };
        window.changeKeywords = (keywords) => {
            var metas = document.getElementsByTagName("meta")
            for (let i = 0; i < metas.length; i++) {
                const element = metas[i];
                if (element.name === 'keywords') {
                    element.content = keywords;
                    return;
                }
            }
        };
    """

let private highlightCode =
    js """
        window.highlightCode = () => {
            if (!!Prism) {
                Prism.highlightAll();
            } else {
                setTimeout(Prism.highlightAll, 5000)
            }
        }
    """


let invokeDynamicBlazorJs = js "window.initBlazor()"

let dynamicBlazorJs immediate =
    fragment {
        js """
            window.initBlazor = () => {
                if (!window.isBlazorLoaded) {
                    const customEltScript = document.createElement("script")
                    customEltScript.src = "_content/Microsoft.AspNetCore.Components.CustomElements/BlazorCustomElements.js"
                    document.body.appendChild(customEltScript)

                    const blazorScript = document.createElement("script")
                    blazorScript.src = "_framework/blazor.server.js"
                    document.body.appendChild(blazorScript)

                    window.isBlazorLoaded = true
                }
            }
        """
        if immediate then
            invokeDynamicBlazorJs
    }




let interopScript =
    fragment {
        changeHeadJs
        highlightCode
    }


type IJSRuntime with
    member js.changeTitle(title: string) = js.InvokeAsync("changeTitle", title)
    member js.changeKeywords(keywords: string) = js.InvokeAsync("changeKeywords", keywords)
    member js.highlightCode() = js.InvokeAsync("highlightCode")
