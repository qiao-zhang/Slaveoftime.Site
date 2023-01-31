namespace Slaveoftime.UI.Components

open Fun.Blazor

type Footer =
    static member Create() = footer {
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
