[<AutoOpen>]
module Slaveoftime.UI.Components.PostView.PostComment

open System
open System.Linq
open System.Security.Claims
open FSharp.Data.Adaptive
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Components
open Microsoft.Extensions.Logging
open Fun.Htmx
open Fun.Result
open Fun.Blazor
open Fun.Blazor.Operators
open Slaveoftime.Db


let private replyIcon =
    Static.html
        """
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" class="w-5 h-5 fill-slate-400 hover:fill-primary">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 20.25c4.97 0 9-3.694 9-8.25s-4.03-8.25-9-8.25S3 7.444 3 12c0 2.104.859 4.023 2.273 5.48.432.447.74 1.04.586 1.641a4.483 4.483 0 01-.923 1.785A5.969 5.969 0 006 21c1.282 0 2.47-.402 3.445-1.087.81.22 1.668.337 2.555.337z" />
        </svg>
        """

let private commentIndent = 50

let rec private renderComments level (comments: Comment seq) canComment =
    html.fragment [
        for comment in comments do
            div {
                class' "rounded bg-slate-200 max-w-[400px] min-w-[200px] p-2 my-1 relative"
                style { marginLeft (level * commentIndent) }
                childContent [
                    p {
                        class' "flex items-center gap-2 opacity-75 text-sm"
                        span { comment.Author.Split("@")[0] }
                        span { comment.CreatedTime.ToString() }
                    }
                    p { html.raw comment.Content }
                    if canComment then
                        button {
                            class' "btn btn-circle btn-sm btn-ghost absolute right-1 top-1"
                            hxTarget ("#child-comments-" + comment.Id.ToString())
                            hxTrigger hxEvt.mouse.click
                            hxSwap_beforeend
                            hxPost $"view/post/{comment.PostId}/comment?parentComment={comment.Id}"
                            replyIcon
                        }
                ]
            }
            div {
                if comment.Children <> null then
                    renderComments (level + 1) comment.Children canComment
                div {
                    style { marginLeft ((level + 1) * commentIndent) }
                    id ("child-comments-" + comment.Id.ToString())
                }
            }
    ]


[<FunBlazorCustomElement>]
type NewPostComment() =
    inherit FunBlazorComponent()

    [<Parameter>]
    member val post_id = "" with get, set

    [<Parameter>]
    member val parent_comment = "" with get, set

    override this.Render() =
        html.inject (fun (db: SlaveoftimeDb, ctx: IHttpContextAccessor, logger: ILogger<NewPostComment>) ->
            let postId = Guid.Parse this.post_id
            let parentComment =
                match Guid.TryParse this.parent_comment with
                | true, x -> Some x
                | _ -> None

            let author =
                ctx.HttpContext.User.Claims
                |> Seq.tryFind (fun x -> x.Type = ClaimTypes.Email)
                |> Option.map (fun x -> x.Value)
                |> Option.get

            let mutable comment = ""
            let newComment = cval None

            let addComment () = task {
                try
                    let comment =
                        Comment(
                            Content = comment,
                            CreatedTime = DateTime.Now,
                            Author = author,
                            PostId = postId,
                            ParentId = Option.toNullable parentComment
                        )
                    db.Comments.Add comment |> ignore
                    do! db.SaveChangesAsync() |> Task.map ignore

                    newComment.Publish(Some(Ok comment))

                with ex ->
                    newComment.Publish(Some(Error "Save comment failed, please retry later!"))
                    logger.LogError(ex, "Save comment failed: {postId} {parentComment}", postId, parentComment)
            }

            adaptiview () {
                match! newComment with
                | None -> section {
                    textarea {
                        class' "rounded-sm bg-slate-200 textarea w-full my-2"
                        autofocus true
                        onchange (fun e -> comment <- e.Value.ToString())
                    }
                    div {
                        class' "flex items-center justify-end my-2 gap-2"
                        button {
                            class' "btn btn-outline"
                            onclick (fun _ -> addComment ())
                            "Cancel"
                        }
                        button {
                            class' "btn btn-primary"
                            onclick (fun _ -> addComment ())
                            "Submit"
                        }
                    }
                  }
                | Some(Ok comment) -> renderComments 0 [ comment ] false
                | Some(Error msg) -> div {
                    class' "my-2 alert alert-error"
                    msg
                  }
            }
        )


type PostComment =
    static member Create(postId: Guid) =
        html.inject (fun (db: SlaveoftimeDb, ctx: IHttpContextAccessor, logger: ILogger<PostComment>) ->
            try
                let ctx = ctx.HttpContext
                let isAuthed = ctx.User.Identity.IsAuthenticated

                let rec groupComments (parentId: Nullable<Guid>) (comments: Comment seq) =
                    comments
                    |> Seq.filter (fun c -> c.ParentId = parentId)
                    |> Seq.sortBy (fun c -> c.CreatedTime)
                    |> Seq.map (fun comment ->
                        comment.Children <- (groupComments (Nullable comment.Id) comments).ToList()
                        comment
                    )

                let post = db.Posts.FirstOrDefault(fun x -> x.Id = postId)
                let comments = db.Comments.Where(fun x -> x.PostId = postId).ToListAsync().Result |> groupComments (Nullable())
                let returnUrl =
                    if String.IsNullOrEmpty post.Slug then
                        $"/blog/{post.Id}"
                    else
                        $"/blog/{post.Slug}"

                section {
                    class' "my-3"
                    childContent [
                        div {
                            class' "flex items-center justify-center my-3"
                            if isAuthed then
                                button {
                                    class' "btn btn-primary mx-auto"
                                    hxTarget "#new-comment-placeholder"
                                    hxTrigger hxEvt.mouse.click
                                    hxSwap_beforeend
                                    hxPost $"view/post/{postId}/comment"
                                    "Add comment"
                                }
                            else
                                a {
                                    class' "link link-primary mx-auto"
                                    href $"signin?returnUrl={returnUrl}"
                                    "Signin to comment (GitHub)"
                                }
                        }
                        div {
                            id "new-comment-placeholder"
                            class' "overflow-auto max-h-[720px]"
                            renderComments 0 comments isAuthed
                        }
                    ]
                }

            with ex ->
                logger.LogError(ex, "Render post {id} comments failed", postId)
                html.none
        )


    static member NewCommentCustomElement(postId: Guid, parentComment: Nullable<Guid>) =
        html.customElement<NewPostComment> (
            attrs =
                ((nameof Unchecked.defaultof<NewPostComment>.post_id => postId.ToString())
                 ==> (if parentComment.HasValue then
                          nameof Unchecked.defaultof<NewPostComment>.parent_comment => parentComment.Value
                      else
                          html.emptyAttr))
        )
