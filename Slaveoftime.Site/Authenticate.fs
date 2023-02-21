module Slaveoftime.Authenticate

open System
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies

let signin =
    Func<IAuthenticationSchemeProvider, HttpContext, _>(fun schemeProvider ctx -> task {
        let! schemes = schemeProvider.GetAllSchemesAsync()

        let provider =
            match ctx.Request.Form.TryGetValue("provider") with
            | true, x -> x.ToString()
            | _ -> ""

        let returnUrl =
            match ctx.Request.Form.TryGetValue("returnUrl") with
            | true, x -> x.ToString()
            | _ -> ""

        let isProviderSupported =
            schemes |> Seq.exists (fun s -> String.Equals(s.Name, provider, StringComparison.OrdinalIgnoreCase))

        if isProviderSupported then
            return Results.Challenge(AuthenticationProperties(RedirectUri = returnUrl), Collections.Generic.List [ provider ])
        else
            return Results.BadRequest()
    })


let signout =
    Func<HttpContext, _>(fun ctx -> task {
        do! ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, AuthenticationProperties(RedirectUri = "/"))
    })
