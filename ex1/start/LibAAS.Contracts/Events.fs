[<AutoOpen>]
module LibAAS.Contracts.Events
open System

type EventData = int

type Events = AggregateId * EventData list
