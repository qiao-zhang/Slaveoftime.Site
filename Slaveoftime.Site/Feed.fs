module Slaveoftime.Feed

open System
open System.IO
open System.Xml
open System.Text
open System.Linq
open System.ServiceModel.Syndication
open Microsoft.EntityFrameworkCore
open FSharp.Data
open Giraffe
open Slaveoftime.Db

type FeedType = RSS | ATOM

let handle feedType: HttpHandler =
    fun nxt ctx -> task {
        let db = ctx.GetService<SlaveoftimeDb>()
        
        let host =
            #if DEBUG
            "https://localhost:6001"
            #else
            "https://www.slaveoftime.fun"
            #endif

        let! posts = db.Posts.OrderByDescending(fun x -> x.CreatedTime).ToListAsync()

        let items = 
            posts
            |> Seq.map (fun post -> 
                let item = SyndicationItem(
                    Id = Uri($"{host}/blog/{post.Id}").ToString(),
                    Title = TextSyndicationContent post.Title,
                    Summary = TextSyndicationContent post.Description,
                    PublishDate = post.CreatedTime,
                    LastUpdatedTime = post.UpdatedTime
                )
                item.Links.Add(SyndicationLink(Uri $"{host}/blog/{post.Slug}"))
                for keyword in post.Keywords.Split([|','; ';'|]) do
                    item.Categories.Add(SyndicationCategory keyword)
                item
            )
            |> Seq.toList

        let feed = SyndicationFeed(
            Title = TextSyndicationContent("slaveOftime blogs"),
            Description = TextSyndicationContent(""),
            Items = items
        )

        feed.Links.Add(SyndicationLink(Uri "https://localhost:6001"))
    
        use memoryStream = new MemoryStream()
        use xmlWriter = XmlWriter.Create(memoryStream, XmlWriterSettings(Encoding = Encoding.UTF8))
        match feedType with
        | RSS -> Rss20FeedFormatter(feed).WriteTo(xmlWriter)
        | ATOM -> Atom10FeedFormatter(feed).WriteTo(xmlWriter)
        xmlWriter.Flush()

        let bytes = memoryStream.ToArray()

        ctx.SetHttpHeader(HttpResponseHeaders.ContentType, "application/xml; charset=utf-8")
        ctx.SetHttpHeader(HttpResponseHeaders.ContentLength, bytes.Length)
        let! _ = ctx.Response.BodyWriter.WriteAsync(bytes)
        
        return! nxt ctx 
    }
