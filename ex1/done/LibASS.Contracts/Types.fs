[<AutoOpen>]
module LibASS.Contracts.Types
open System
 
type AggregateId = AggregateId of int

type ItemId = ItemId of int
type Title = Title of string
type Author = Author of string
type Book = {Title: Title; Author: Author }
type Quantity = private Quantity of int
    with 
        static member Create x = 
            if x >= 0 then Quantity x
            else raise (exn "Invalid quantity")
type ItemData = 
    | Book of Book
type Item = ItemId*ItemData

type Version = int
type Error = 
    | NotImplemented of string
    | VersionConflict of string
    | InvalidStateTransition of string
    | InvalidState of string
    | InvalidItem

