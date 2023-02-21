namespace Slaveoftime.UI.Pages

open Microsoft.AspNetCore.Http
open Fun.Blazor
open Slaveoftime.UI.Components

type SigninPage =

    static member Create() =
        html.inject (fun (ctx: IHttpContextAccessor) ->
            let ctx = ctx.HttpContext

            let returnUrl =
                match ctx.Request.Query.TryGetValue("returnUrl") with
                | true, x -> x.ToString()
                | _ -> "/"

            let node = section {
                class' "mx-auto"
                childContent [
                    p {
                        class' "text-center mt-12 mb-5"
                        "Signin with"
                    }
                    div {
                        class' "flex items-center justify-center"
                        Authenticate.AuthProviders(returnUrl)
                    }
                ]
            }

            Layout.Create(bodyNode = node)
        )
