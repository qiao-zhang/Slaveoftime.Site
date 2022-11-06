[<AutoOpen>]
module Slaveoftime.UI.Stores

open Fun.Blazor


type IShareStore with

    member store.Header = store.CreateCVal(nameof store.Header, "")
    member store.Keywords = store.CreateCVal(nameof store.Keywords, "")
