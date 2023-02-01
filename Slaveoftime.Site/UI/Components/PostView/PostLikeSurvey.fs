namespace Slaveoftime.UI.Components.PostView

open System
open System.Linq
open FSharp.Data.Adaptive
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Components
open Fun.Result
open Fun.Blazor
open Slaveoftime.Db


[<FunBlazorCustomElement>]
type PostLikesSurvey() =
    inherit FunBlazorComponent()

    [<Parameter>]
    member val post_id = "" with get, set

    override this.Render() =
        html.inject (fun (db: SlaveoftimeDb) ->
            let isClicked = cval false

            adaptiview () {
                let! isClicked, setIsClicked = isClicked.WithSetter()
                match Guid.TryParse(this.post_id) with
                | true, postId when not isClicked -> section {
                    class' "flex items-center justify-center gap-3"
                    childContent [
                        p { "Do you like this post?" }
                        button {
                            onclick (fun _ ->
                                setIsClicked true
                                db.Posts
                                    .Where(fun x -> x.Id = postId)
                                    .ExecuteUpdateAsync(fun setter -> setter.SetProperty((fun x -> x.Likes), (fun x -> x.Likes + 1)))
                                |> Task.map ignore
                            )
                            class' "btn btn-sm btn-success"
                            "Yes"
                        }
                        button {
                            onclick (fun _ ->
                                setIsClicked true
                                db.Posts
                                    .Where(fun x -> x.Id = postId)
                                    .ExecuteUpdateAsync(fun setter -> setter.SetProperty((fun x -> x.DisLikes), (fun x -> x.DisLikes + 1)))
                                |> Task.map ignore
                            )
                            class' "btn btn-sm btn-outline"
                            "No"
                        }
                    ]
                  }

                | _ -> html.none
            }
        )
