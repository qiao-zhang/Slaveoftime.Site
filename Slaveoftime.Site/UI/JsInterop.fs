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


let interopScript =
    fragment {
        changeHeadJs
        highlightCode
    }


type IJSRuntime with
    member js.changeTitle(title: string) = js.InvokeAsync("changeTitle", title)
    member js.changeKeywords(keywords: string) = js.InvokeAsync("changeKeywords", keywords)
    member js.highlightCode() = js.InvokeAsync("highlightCode")
