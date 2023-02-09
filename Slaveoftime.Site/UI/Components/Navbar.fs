namespace Slaveoftime.UI.Components

open Fun.Blazor

type Navbar =
    static member Create() =
        header {
            class' "py-6 dark:bg-slate-500/20 bg-slate-700 flex flex-row gap-2 items-center justify-center shadow-md"
            childContent [
                a {
                    class' "font-semibold text-yellow-500 text-center"
                    href "/"
                    "slaveOftime"
                }
                a {
                    class' "link"
                    href "/feed"
                    target "_blank"
                    Static.html """
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-5 h-5 text-slate-300">
                            <path stroke-linecap="round" stroke-linejoin="round" d="M12.75 19.5v-.75a7.5 7.5 0 00-7.5-7.5H4.5m0-6.75h.75c7.87 0 14.25 6.38 14.25 14.25v.75M6 18.75a.75.75 0 11-1.5 0 .75.75 0 011.5 0z" />
                        </svg>
                    """
                }
            ]
        }
