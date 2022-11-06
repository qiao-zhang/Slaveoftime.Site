// hot-reload
[<AutoOpen>]
module Slaveoftime.UI.MainView

open Fun.Blazor
open Fun.Blazor.Router


let private navbar =
    html.fragment [
        div {
            class' "flex flex-row justify-center bg-green-100/30 text-green-400 py-2"
            "STAND FOR "
            span {
                class' "font-semibold mx-3"
                "PEACE"
            }
            "ONLY!"
        }
        header {
            class' "py-6 dark:bg-slate-500/20 bg-slate-700 flex flex-row items-center justify-center shadow-md"
            a {
                class' "font-semibold text-yellow-500 text-center"
                href "/"
                "slaveOftime"
            }
        }
    ]


let private footerSection = footer {
    class' "flex flex-col justify-center items-center py-10"
    childContent [
        p {
            class' "text-sm text-neutral-400/90"
            "albertwoo_slaveoftime@hotmail.com"
        }
        p {
            class' "mt-2 text-sm text-teal-500/80 font-semibold"
            "Powered by ASP.NET Core 6"
        }
        p {
            class' "mt-2 text-sm text-neutral-400/90"
            "2022-10-31"
        }
    ]
}


let private analytics =
    Static.html """
        <!-- Google tag (gtag.js) -->
        <script async src="https://www.googletagmanager.com/gtag/js?id=UA-147730361-1"></script>
        <script>
            window.dataLayer = window.dataLayer || [];
            function gtag(){dataLayer.push(arguments);}
            gtag('js', new Date());

            gtag('config', 'UA-147730361-1');
        </script>
    """


let mainView = div {
    navbar
    html.route [ 
        routeCif "/blog/%O" postDetail
        routeAny postList
    ]
    footerSection
    analytics
}
