[<AutoOpen>]
module LibASS.Contracts.Events
open System

type EventData = int

type Events = AggregateId * EventData list
