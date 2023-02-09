namespace Slaveoftime.UI.Components.PostView

open System
open System.Linq
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Components
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


[<FunBlazorCustomElement>]
type PostViewCount() =
    inherit FunBlazorComponent()

    [<Parameter>]
    member val post_id = "" with get, set

    [<Parameter>]
    member val count = 0 with get, set

    override this.Render() =
        html.inject (fun (db: SlaveoftimeDb, hook: IComponentHook) ->
            hook.AddFirstAfterRenderTask(fun _ -> task {
                match Guid.TryParse(this.post_id) with
                | true, postId ->
                    do!
                        db.Posts
                            .Where(fun x -> x.Id = postId)
                            .ExecuteUpdateAsync(fun setter -> setter.SetProperty((fun x -> x.ViewCount), (fun x -> x.ViewCount + 1)))
                        |> Task.map ignore
                | _ -> ()
            })

            if this.count > 0 then
                PostView.ViewCount(this.count)
            else
                html.none
        )