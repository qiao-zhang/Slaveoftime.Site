module Slaveoftime.Feed

open System
open System.IO
open System.Xml
open System.Linq
open System.Text
open System.Net.Http
open System.ServiceModel.Syndication
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore
open FSharp.Data
open Giraffe
open Slaveoftime.Db
open Slaveoftime.UI.Components


type FeedType = RSS | ATOM


let private feedCacheFile (feedType: FeedType) = $"feed-{feedType}.xml"


let generateFeedFile (sp: IServiceProvider) = task {
    let db = sp.GetService<SlaveoftimeDb>()
    let logger = sp.GetService<ILoggerFactory>().CreateLogger("FeedGenerator")
        
    logger.LogInformation "Prepare posts for feed generating"

    use http = new HttpClient()
    http.BaseAddress <- Uri host

    let! posts = db.Posts.OrderByDescending(fun x -> x.CreatedTime).ToListAsync()

    let items = Collections.Generic.List()
    for post in posts do
        let item = SyndicationItem(
            Id = post.Id.ToString(),
            BaseUri = Uri host,
            Title = TextSyndicationContent post.Title,
            PublishDate = post.CreatedTime,
            LastUpdatedTime = post.UpdatedTime
        )

        item.Links.Add(SyndicationLink.CreateAlternateLink(Uri $"{host}/blog/{post.Slug}"))

        let mainImageFile = FileInfo(postsDir </> post.MainImage)
        if String.IsNullOrEmpty post.MainImage |> not && mainImageFile.Exists then
            item.Links.Add(SyndicationLink.CreateMediaEnclosureLink(Uri $"{host}/blog/{post.MainImage}", "image", mainImageFile.Length))
        
        if String.IsNullOrEmpty post.Keywords |> not then
            for keyword in post.Keywords.Split([|','; ';'|]) do
                item.Categories.Add(SyndicationCategory keyword)

        try
            logger.LogInformation("Fetch content for post {id}", post.Id)
            let! content = http.GetStringAsync($"view/post/feed/{post.Id}")
            item.Summary <- SyndicationContent.CreateHtmlContent(content)
        with ex ->
            logger.LogError(ex, "Fetch content for post {id} failed", post.Id)

        items.Add item

    let feed = SyndicationFeed(
        Title = TextSyndicationContent("slaveOftime blogs"),
        Description = TextSyndicationContent("This site is my personal blogs, I will try some technology on this site when needed."),
        Items = items
    )

    let categories = [
        "blog"
        "vlog"
        "developing"
        "dotnet"
        "csharp"
        "fsharp"
    ]

    for cat in categories do
        feed.Categories.Add(SyndicationCategory cat)

    feed.Links.Add(SyndicationLink.CreateAlternateLink(Uri host))
    feed.Links.Add(SyndicationLink.CreateSelfLink(Uri $"{host}/feed", "application/rss+xml"))

    for feedType in [RSS; ATOM] do
        try
            logger.LogInformation("Generating feed file for {feedType}", feedType)
            use memoryStream = new MemoryStream()
            use xmlWriter = XmlWriter.Create(memoryStream, XmlWriterSettings(Encoding = Encoding.UTF8))
            match feedType with
            | RSS -> Rss20FeedFormatter(feed).WriteTo(xmlWriter)
            | ATOM -> Atom10FeedFormatter(feed).WriteTo(xmlWriter)
            xmlWriter.Flush()

            let bytes = memoryStream.ToArray()
            let fileName = feedCacheFile feedType
            File.WriteAllBytes(fileName, bytes)
            logger.LogInformation("Generated feed {file}", fileName)
        
        with ex ->
            logger.LogError(ex, "Generating feed file for {feedType} failed", feedType)
}


let handle feedType: HttpHandler =
    fun nxt ctx -> task {
        let fileName = FileInfo(feedCacheFile feedType)
        if fileName.Exists then
            return! (
                setHttpHeader HttpResponseHeaders.ContentType "application/xml; charset=utf-8"
                >=> streamFile true fileName.FullName None (Some fileName.LastWriteTime) 
            ) nxt ctx
        else
            return! RequestErrors.NOT_FOUND "Feed is not ready yet" nxt ctx
    }
