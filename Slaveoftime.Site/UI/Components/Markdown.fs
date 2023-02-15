namespace Slaveoftime.UI.Components

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

        renderer.LinkRewriter <- composeUrl baseUrl

        pipeline.Setup(renderer)

        let document = MarkdownParser.Parse(markdown, pipeline)
        renderer.Render(document) |> ignore

        writer.Flush()
        writer.ToString()


    static member RenderForBlog(relativeSlug: string, markdown: string) =
        let baseUrl = host <//> "blog" <//> relativeSlug
        html.raw (Markdown.ConvertToHtml(baseUrl, markdown))


    static member RenderCodeBlockForBlog(relativeSlug: string, file: string) =
        try
            html.raw (File.ReadAllText(postsDir </> relativeSlug.Replace('/', Path.PathSeparator) </> file + ".html"))
        with ex ->
            html.none
