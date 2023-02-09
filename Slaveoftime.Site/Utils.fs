[<AutoOpen>]
module Slaveoftime.Utils

open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Reflection
open System.Globalization


let inline (</>) x y = 
    let x = if String.IsNullOrEmpty x then "" else x
    let y = if String.IsNullOrEmpty y then "" else y
    Path.Combine(x, y)


let inline (<?>) (x: 'T voption) (def: 'T) =
    match x with
    | ValueSome x -> x
    | _ -> def


let postsDir = DirectoryInfo("UI/Pages/Posts").FullName

let versionStampFile = "version-stamp"
let versionStamp = FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString()

    
let host =
    #if DEBUG
    "https://localhost:6001"
    #else
    "https://www.slaveoftime.fun"
    #endif


let toSlug (title: string) =
    let invalidChars = Regex.Escape(string (Path.GetInvalidFileNameChars()))
    let invalidRegStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars)

    let slug =
        Regex.Replace(title, invalidRegStr, "_").Normalize(NormalizationForm.FormD)
        |> Seq.filter (fun c -> CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark)
        |> String.Concat
        |> fun x -> x.Normalize(NormalizationForm.FormC)

    let slug = slug.Replace(" ", "-")
    slug.ToLowerInvariant()


let siteTitle = "slaveOftime blogs"
let siteDescription = "This site is my personal blogs, I will try some technology on this site when needed."
let siteKeywords = [
    "blog"
    "vlog"
    "developing"
    "dotnet"
    "csharp"
    "fsharp"
]
