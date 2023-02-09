[<AutoOpen>]
module Slaveoftime.Utils

open System.IO
open System.Reflection


let inline (</>) x y = Path.Combine(x, y)


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
