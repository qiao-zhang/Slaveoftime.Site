namespace Slaveoftime.UI.Components

open Fun.Blazor

type Footer =
    static member Create() = footer {
        class' "flex flex-col justify-center items-center py-10 gap-2"
        childContent [
            p {
                class' "text-sm text-teal-500/80 font-semibold"
                "Powered by ASP.NET Core 7"
            }
            p {
                class' "text-sm text-neutral-400"
                "albertwoo_slaveoftime@hotmail.com"
            }
            p {
                class' "text-sm text-neutral-400"
                "2023-01-31"
            }
        ]
    }
