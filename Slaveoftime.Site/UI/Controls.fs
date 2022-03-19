// hot-reload
[<AutoOpen>]
module Slaveoftime.UI.Controls

open Fun.Blazor


let keywords (keywords: string) =
    div.create [
        for keyword in keywords.Split [| ','; ';' |] do
            span {
                class' "text-xs px-3 py-1 rounded-full mr-2 dark:bg-teal-100/20 bg-teal-200/20 dark:text-neutral-100/80 text-neutral-700/90"
                keyword
            }
    ]


let footerSection =
    footer {
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


let spinner =
    Static.html """
        <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-yellow-500/90" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
    """


let loader =
    div {
        class' "flex flex-row justify-center my-19"
        spinner
    }
