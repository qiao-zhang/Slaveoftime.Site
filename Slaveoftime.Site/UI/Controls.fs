// hot-reload
[<AutoOpen>]
module Slaveoftime.UI.Controls

open System.IO
open Fun.Blazor


let inline (</>) x y = Path.Combine(x, y)


let lazyStylesheet path = link {
    href path
    rel "stylesheet"
    media "print"
    "onload", "this.media='all'"
}


let spinner =
    Static.html """
        <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-yellow-500/90" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
    """


let loader = div {
    class' "flex flex-row justify-center my-19"
    spinner
}


let reconnectView = fragment {
    styleElt {
        childContentRaw """
            #components-reconnect-modal {
                display: none !important;
            }
            #components-reconnect-modal.components-reconnect-show, 
            #components-reconnect-modal.components-reconnect-failed, 
            #components-reconnect-modal.components-reconnect-rejected {
                display: block !important;
            }
        """
    }
    Static.html """
        <div id="components-reconnect-modal" 
            onclick="document.location.reload()" 
            class="components-reconnect-hide fixed top-0 h-full w-full bg-white opacity-[0.01]">
        </div>
    """
}
