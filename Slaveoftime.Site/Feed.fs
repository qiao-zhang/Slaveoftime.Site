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
open Slaveoftime
open Slaveoftime.Db
open Microsoft.AspNetCore.Http


type FeedType =
    | RSS
    | ATOM


let private feedCacheFile (feedType: FeedType) = $"feed-{feedType}.xml"


let generateFeedFile (sp: IServiceProvider) = task {
    let db = sp.GetService<SlaveoftimeDb>()
    let logger = sp.GetService<ILoggerFactory>().CreateLogger("FeedGenerator")

    logger.LogInformation "Prepare posts for feed generating"

    use http = new HttpClient()
    http.BaseAddress <- Uri host

    let! posts = db.Posts.Where(fun x -> x.IsActive).OrderByDescending(fun x -> x.CreatedTime).ToListAsync()

    let items = Collections.Generic.List()
    for post in posts do
        try
            let item =
                SyndicationItem(
                    Id = post.Id.ToString(),
                    BaseUri = Uri host,
                    Title = TextSyndicationContent post.Title,
                    PublishDate = post.CreatedTime,
                    LastUpdatedTime = post.UpdatedTime
                )

            item.Links.Add(SyndicationLink.CreateAlternateLink(Uri $"{host}/blog/{post.Slug}"))

            // let mainImageFile = FileInfo(postsDir </> post.MainImage)
            // if String.IsNullOrEmpty post.MainImage |> not && mainImageFile.Exists then
            //     item.Links.Add(SyndicationLink.CreateMediaEnclosureLink(Uri(host <//> "blog" <//> post.MainImage), "image", mainImageFile.Length))

            if String.IsNullOrEmpty post.Keywords |> not then
                for keyword in post.Keywords.Split([| ','; ';' |]) do
                    item.Categories.Add(SyndicationCategory keyword)

            logger.LogInformation("Fetch content for post {id}", post.Id)
            let! content = http.GetStringAsync($"view/post/feed/{post.Id}")
            item.Summary <- SyndicationContent.CreateHtmlContent(content)

            items.Add item

        with ex ->
            logger.LogError(ex, "Fetch content for post {id} failed", post.Id)


    let feed =
        SyndicationFeed(Title = TextSyndicationContent(siteTitle), Description = TextSyndicationContent(siteDescription), Items = items)

    for cat in siteKeywords do
        feed.Categories.Add(SyndicationCategory cat)

    feed.Links.Add(SyndicationLink.CreateAlternateLink(Uri host))
    feed.Links.Add(SyndicationLink.CreateSelfLink(Uri $"{host}/feed", "application/rss+xml"))

    for feedType in [ RSS; ATOM ] do
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


let create feedType =
    Func<_, _>(fun () -> task {
        let fileName = FileInfo(feedCacheFile feedType)
        if fileName.Exists then
            return Results.File(fileName.FullName, contentType = "application/xml", lastModified = fileName.LastWriteTime)
        else
            return Results.NotFound "Feed is not ready yet"
    })
