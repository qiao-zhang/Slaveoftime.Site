module Slaveoftime.Feed

open System
open System.IO
open System.Xml
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

        let! posts = db.Posts.OrderByDescending(fun x -> x.CreatedTime).ToListAsync()

        let items = 
            posts
            |> Seq.map (fun post -> 
                let item = SyndicationItem(
                    Id = post.Id.ToString(),
                    Title = TextSyndicationContent post.Title,
                    Summary = TextSyndicationContent post.Description,
                    BaseUri = 
                        #if DEBUG
                        Uri($"https://localhost:6001/blog/{post.Slug}")
                        #else
                        Uri($"https://www.slaveoftime.fun/blog/{post.Slug}")
                        #endif
                    ,
                    PublishDate = post.CreatedTime,
                    LastUpdatedTime = post.UpdatedTime
                )
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

        let feedXml = 
            use stringWriter = new StringWriter()
            use xmlWriter = XmlWriter.Create(stringWriter)
            match feedType with
            | RSS -> Rss20FeedFormatter(feed).WriteTo(xmlWriter)
            | ATOM -> Atom10FeedFormatter(feed).WriteTo(xmlWriter)
            xmlWriter.Flush()
            stringWriter.ToString()

        let! _ = ctx.WriteStringAsync(feedXml)
        ctx.SetHttpHeader(HttpResponseHeaders.ContentType, "application/xml")
        
        return! nxt ctx 
    }
