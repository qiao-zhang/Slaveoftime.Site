namespace Slaveoftime.UI.Pages.Posts

open System
open Fun.Blazor
open Fun.Blazor.Operators
open Slaveoftime.UI.Components
open Slaveoftime.UI.Components.PostView


type ``A post for describe the post itself`` =

    interface IDynamicPost with

        static member Meta = {
            Id = Guid.Parse "142ce52b-6beb-4a88-a776-ddfed6d77844"
            Title = "A post for describe the post site itself"
            Keywords = "blog,fsharp"
            Description = "This blog site is rebuilt for many times, now, there is one more to go ..."
            CreateTime = DateTime.Parse "2023-02-01"
        }

        static member View =
            let markdown x = Markdown.RenderForBlog("A post for describe the post itself", x)

            html.fragment [
                markdown
                    "
Every time when I learned something, I want to try it out and my blog site is the place to let me play around.

In my last post I mixed giraffe, HTMX and blazor together. To verify that combination I upgraded this blog site again. The updated features are:

- Create static post under the source code repo
- Create dynamic post using fsharp
- Added search functions powered by HTMX
- Added likes survey function
- ...

And here, this post itself is a dynamic post. For example I can add a like survey component at below (After you clicked on Yes or No, it will disapear. Normally it is displaied at the end of each post):
                    "
                html.customElement<PostLikesSurvey>(
                    attrs = (nameof Unchecked.defaultof<PostLikesSurvey>.post_id => "142ce52b-6beb-4a88-a776-ddfed6d77844")
                )
                markdown
                    "
You can also easily copy paste html and display by using Static.html, for example below is an alert example from daisyUI:
                    "
                Static.html
                    """
<div class="alert shadow-lg">
  <div>
    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="stroke-info flex-shrink-0 w-6 h-6"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
    <span>12 unread messages. Tap to see.</span>
  </div>
</div>
                    """
                markdown
                    "
## Summary

It is nothing but the source code of a simple personal blog project, allow me to write normal markdown post and also allow me to write post with code. Here is using **fsharp**. The idea is a little bit like **Astro**.

I thought I should have more to talk about in this post, but ...

Just wasted my time again! slaveOftime
                    "
            ]
