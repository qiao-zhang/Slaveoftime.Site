namespace Slaveoftime.UI.Pages.Posts.Demo2

open System
open Fun.Blazor
open Slaveoftime.UI.Components

type Demo21 =
    // We will use reflection to find the interface and register to database
    interface IDynamicPost with

        static member Meta = {
            Id = Guid.Parse "6d8d2c1c-c45f-4600-8ade-8fd7f8f24f1b"
            Title = "demo21"
            Keywords = "k1,k2"
            Description = "demo21 description"
            CreateTime = DateTime.Parse "2023-01-23"
        }

        static member View = div {
            p { "Demo21" }
            Markdown.RenderForBlog(
                "Demo2",
                $"""
## Sub title

Some information

- list1
- list2

![](./demo2.png)

{DateTime.Now}
                """
            )
        }
