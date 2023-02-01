#nowarn "3535"

namespace Slaveoftime.UI.Components.PostView

open System
open Fun.Blazor


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


type PostView =

    static member Keywords(keywords: string) =
        div.create [
            for keyword in keywords.Split [| ','; ';' |] do
                span {
                    class' "text-xs px-3 py-1 rounded-full mr-2 dark:bg-teal-100/20 bg-teal-200/20 dark:text-neutral-100/80 text-neutral-700/90"
                    keyword
                }
        ]

    static member ViewCount(count: int) = span {
        class' "rounded-lg dark:text-neutral-100/90 text-neutral-700/90 bg-teal-500/40 px-2 text-sm"
        $"View {count}"
    }

    static member LiksView(likes: int) = span {
        class' "rounded-lg dark:text-neutral-100/90 text-neutral-700/90 bg-teal-500/40 px-2 text-sm"
        $"Likes {likes}"
    }
