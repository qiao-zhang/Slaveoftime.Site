namespace Slaveoftime.UI.Components

open System
open System.IO
open Markdig
open Markdig.Parsers
open Markdig.Renderers
open Fun.Blazor
open Slaveoftime


type Markdown =

    static member ConvertToHtml(baseUrl, markdown: string) =
        let pipeline = MarkdownPipelineBuilder().Build()
        use writer = new StringWriter()
        let renderer = new HtmlRenderer(writer)

        renderer.LinkRewriter <-
            fun url ->
                if url.StartsWith "/" then
                    baseUrl + url.Substring(1)
                elif url.StartsWith "./" then
                    baseUrl + url.Substring(2)
                elif url.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) then
                    url
                elif url.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase) then
                    url
                else
                    baseUrl + url

        pipeline.Setup(renderer)

        let document = MarkdownParser.Parse(markdown, pipeline)
        renderer.Render(document) |> ignore

        writer.Flush()
        writer.ToString()


    static member RenderForBlog(relativeSlug: string, markdown: string) =
        let ensureEnd (str: string) = if str.EndsWith "/" then relativeSlug else str + "/"

        let baseUrl =
            host + "/blog/"
            + (if relativeSlug = "/" then ""
               else if relativeSlug.StartsWith "/" then relativeSlug.Substring(1) |> ensureEnd
               else relativeSlug |> ensureEnd)

        html.raw (Markdown.ConvertToHtml(baseUrl, markdown))
