[<AutoOpen>]
module LibAAS.Contracts.Types
open System
 
type AggregateId = AggregateId of int
type ItemId = ItemId of int

type Version = int

type Error = 
    | NotImplemented of string
    | VersionConflict of string
    | InvalidStateTransition of string
    | InvalidState of string
    | InvalidItem

