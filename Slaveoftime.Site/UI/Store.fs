[<AutoOpen>]
module Slaveoftime.UI.Store

open Fun.Blazor


type IShareStore with

    member store.IsPrerendering = store.CreateCVal(nameof store.IsPrerendering, false)
