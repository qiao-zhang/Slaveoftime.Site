namespace Slaveoftime.UI.Components

open Fun.Blazor

type Navbar =
    static member Create() =
        header {
            class' "py-6 dark:bg-slate-500/20 bg-slate-700 flex flex-row items-center justify-center shadow-md"
            a {
                class' "font-semibold text-yellow-500 text-center"
                href "/"
                "slaveOftime"
            }
        }
