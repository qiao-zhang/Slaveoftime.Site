#nowarn "3535"

namespace Slaveoftime.UI.Components.PostView

open System
open Fun.Blazor
open Slaveoftime


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
    MainImage: string
    IsHidden: bool
}


type IDynamicPost =
    static abstract member Meta: PostMeta
    static abstract member View: NodeRenderFragment


type PostView =

    static member Keywords(keywords: string) = html.fragment [
        for keyword in keywords.Split keywordsSpliter do
            span {
                class' "badge badge-primary"
                keyword
            }
    ]

    static member ViewCount(count: int) = span {
        class' "badge badge-accent badge-sm opacity-90"
        $"Views {count}"
    }

    static member LiksView(likes: int) = span {
        class' "badge badge-accent badge-sm opacity-90"
        $"Likes {likes}"
    }
