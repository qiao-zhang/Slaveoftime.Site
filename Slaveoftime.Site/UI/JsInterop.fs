[<AutoOpen>]
module Slaveoftime.UI.JsInterop

open System
open Microsoft.AspNetCore.Components
open Microsoft.JSInterop
open Fun.Blazor


type SlChangeEventArgs() =
    inherit EventArgs()

    member val Value = null with get, set


[<EventHandler("onsl-change", typeof<SlChangeEventArgs>, enableStopPropagation = true, enablePreventDefault = true)>]
[<AbstractClass; Sealed>]
type EventHandlers = class end


let private registerCustomEventsJs =
    js """
        Blazor.registerCustomEventType('sl-change', {
            browserEventName: 'sl-change',
            createEventArgs: event => {
                return {
                    value: event.target.value
                };
            }
        });
    """


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


let dynamicBlazorJs =
    js """
        if (!window.isBlazorLoaded) {
            const customEltScript = document.createElement("script")
            customEltScript.src = "_content/Microsoft.AspNetCore.Components.CustomElements/BlazorCustomElements.js"
            document.body.appendChild(customEltScript)

            const blazorScript = document.createElement("script")
            blazorScript.src = "_framework/blazor.server.js"
            document.body.appendChild(blazorScript)

            window.isBlazorLoaded = true
        }
    """


let interopScript =
    fragment {
        registerCustomEventsJs
        changeHeadJs
        highlightCode
    }


type IJSRuntime with
    member js.changeTitle(title: string) = js.InvokeAsync("changeTitle", title)
    member js.changeKeywords(keywords: string) = js.InvokeAsync("changeKeywords", keywords)
    member js.highlightCode() = js.InvokeAsync("highlightCode")
