namespace Slaveoftime.UI.Pages.Posts.Demo2

open FSharp.Data.Adaptive
open System
open Fun.Blazor
open Slaveoftime.UI.Components
open Slaveoftime.UI.Components.PostView

// code-block Demo22Counter
[<FunBlazorCustomElement>]
type Demo22Counter() =
    inherit FunBlazorComponent()

    override _.Render() =
        let count = cval 0
        div {
            adaptiview () {
                let! c = count
                html.text $"count = {c}"
            }
            button {
                onclick (fun _ -> count.Publish((+) 1))
                "Increase"
            }
        }
// code-block-end

type Demo22 =

    interface IDynamicPost with

        static member Meta = {
            Id = Guid.Parse "2d0af6fc-7aca-4796-bb08-528db16be32f"
            Title = "demo22 中文赐福 阿松大 * awd& == _"
            Keywords = "k1,k2"
            Description = "demo2 description"
            CreateTime = DateTime.Parse "2023-01-24"
            MainImage = "Demo2/demo2.png"
            IsHidden = false
        }

        static member View = 
            let relativePath = "Demo2"
            let markdown x = Markdown.RenderForBlog(relativePath, x)
            let codeblock x = Markdown.RenderCodeBlockForBlog(relativePath, x)
            div {
                p { "Demo22" }
                html.customElement<Demo22Counter> ()
                markdown "Below is the source code for it:"
                codeblock "Demo22Counter"
            }
