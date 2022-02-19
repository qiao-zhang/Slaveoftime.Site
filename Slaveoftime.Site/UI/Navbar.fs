[<AutoOpen>]
module Slaveoftime.UI.Navbar

open Fun.Blazor


let navbar =
    header {
        class' "py-6 bg-slate-500/20 flex flex-row items-center justify-center"
        a {
            class' "font-semibold text-yellow-500 text-center"
            href "/"
            "slaveOftime"
        }
    }
