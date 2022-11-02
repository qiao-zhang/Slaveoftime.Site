// hot-reload
namespace Slaveoftime.UI

open System
open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Components
open Microsoft.AspNetCore.Components.Web
open Microsoft.AspNetCore.Components.Server
open Slaveoftime.Services
open Fun.Blazor
open Fun.Blazor.Operators


// It will not just display the view count but also increase viewcount.
[<Extension>]
type DetailViewCount() as this =
    inherit FunBlazorComponent()

    static member CustomElementName = "detail-view-count"
    static member CustomElement = EltBuilder DetailViewCount.CustomElementName


    [<Extension>]
    static member RegisterViewCount(options: CircuitRootComponentOptions) =
        options.RegisterAsCustomElement<DetailViewCount>(DetailViewCount.CustomElementName)
        options


    [<Parameter>]
    member val PostId = "" with get, set

    [<Parameter>]
    member val ViewCount = 0 with get, set


    override _.Render() =
        html.inject (fun (hook: IComponentHook, postService: PostService) ->
            hook.AddAfterRenderTask(fun _ -> postService.IncreaseViewCount(Guid.Parse this.PostId))

            span {
                class' "rounded-lg dark:text-neutral-100/90 text-neutral-700/90 bg-teal-500/40 mx-3 px-3 py-1"
                $"View {this.ViewCount}"
            }
        )


type DetailViewCount'() =
    inherit CustomEltBuilder(DetailViewCount.CustomElementName)

    [<CustomOperation("PostId")>]
    member inline _.PostId([<InlineIfLambda>] render: AttrRenderFragment, v: string) = render ==> ("post-id" => v)

    [<CustomOperation("ViewCount")>]
    member inline _.ViewCount([<InlineIfLambda>] render: AttrRenderFragment, v: int) = render ==> ("view-count" => v)
