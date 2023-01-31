namespace Slaveoftime.UI.Components

open Fun.Blazor

type Navbar =
    static member Create() =
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
