[<AutoOpen>]
module LibAAS.Tests.Specification
open LibAAS.Contracts
open LibAAS.Domain.DomainEntry
open EventStore
open Swensen.Unquote

type Precondition = 
    { presets: Events list }

type Specification = 
    { PreCondition:Precondition
      Command: Command option
      PostCondition: (Result<EventData list, Error>) option }

let notImplemented = fun _ -> "Dependency not set for test" |> NotImplemented |> fail

let defaultPreconditions = 
    { presets = [] }

let Given preCondition = 
    { PreCondition = preCondition 
      Command = None
      PostCondition = None }

let When command spec = {spec with Command = Some command}

let Then (postCondition: Result<EventData list, Error>) spec =
    let finalSpec = {spec with PostCondition = Some postCondition}
    let eventStore = createEventStore<EventData, Error> (Error.VersionConflict "Invalid version when saving")
    let executer = execute eventStore

    let savePreConditions preCondition = 
        preCondition 
        |> List.iter (fun (AggregateId aggId, events) -> 
                            eventStore.SaveEvents (StreamId aggId) (StreamVersion 0) events |> ignore)

    finalSpec.PreCondition.presets |> savePreConditions
    let actual = finalSpec.Command |> Option.get |> executer
    let expected = finalSpec.PostCondition |> Option.get

    test <@ actual = expected @>
