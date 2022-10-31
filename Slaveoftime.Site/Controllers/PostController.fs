namespace Slaveoftime.Site

open Microsoft.AspNetCore.Mvc
open Slaveoftime.Services


[<Route "/api/post">]
type PostController(postService: PostService) as this =
    inherit ControllerBase()

    [<HttpPost "{id}/viewcount">]
    member _.IncreaseViewCount(id) = task {
        do! postService.IncreaseViewCount id
        return this.Ok()
    }
