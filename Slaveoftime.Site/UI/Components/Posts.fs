#nowarn "3535"

namespace Slaveoftime.UI.Components

open System
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Components
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


[<RequireQualifiedAccess>]
type PostType =
    | Static of relativePath: string
    | Dynamic

[<CLIMutable>]
type PostMeta = {
    Id: Guid
    Title: string
    Keywords: string
    Description: string
    CreateTime: DateTime
}


type IDynamicPost =
    static abstract member Meta: PostMeta
    static abstract member View: NodeRenderFragment


type PostViews =

    static member Keywords (keywords: string) =
        div.create [
            for keyword in keywords.Split [| ','; ';' |] do
                span {
                    class' "text-xs px-3 py-1 rounded-full mr-2 dark:bg-teal-100/20 bg-teal-200/20 dark:text-neutral-100/80 text-neutral-700/90"
                    keyword
                }
        ]

    static member ViewCount (count: int) = span {
        class' "rounded-lg dark:text-neutral-100/90 text-neutral-700/90 bg-teal-500/40 mx-3 px-3 py-1 text-sm"
        $"View {count}"
    }


type ViewCount () =
    inherit FunBlazorComponent()

    [<Parameter>] member val count = 0 with get, set

    override this.Render() =
        html.inject (fun (db: SlaveoftimeDb, hook: IComponentHook) ->
            hook.AddFirstAfterRenderTask(fun _ -> task {
                do! 
                    db.Posts.ExecuteUpdateAsync(fun setter -> 
                        setter.SetProperty((fun x -> x.ViewCount), (fun x -> x.ViewCount + 1))
                    )
                    |> Task.map ignore
            })

            PostViews.ViewCount(this.count)
        )
