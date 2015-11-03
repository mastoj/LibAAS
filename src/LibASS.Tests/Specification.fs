[<AutoOpen>]
module LibASS.Tests.Specification
open EventStore
open Swensen.Unquote

type Specification = 
    {
        PreCondition: (AggregateId*Event) list
        Command: Command option
        PostCondition: (Result<Event list, Error>) option
    }

let Given preCondition = 
    { PreCondition = preCondition 
      Command = None
      PostCondition = None }

let When command spec = {spec with Command = Some command}

let Then (postCondition: Result<Event list, Error>) spec =
    let finalSpec = {spec with PostCondition = Some postCondition}
    let eventStore = createEventStore<Event,Error> (Error.VersionConflict "Invalid version when saving")
    let executer = CommandHandling.execute eventStore

    let savePreConditions preCondition = 
        preCondition 
        |> List.groupBy fst
        |> List.iter (fun (AggregateId aggId, events) -> eventStore.SaveEvents (StreamId aggId) (StreamVersion 0) (events |> List.map snd) |> ignore)

    finalSpec.PreCondition |> savePreConditions
    let actual = finalSpec.Command |> Option.get |> executer
    let expected = finalSpec.PostCondition |> Option.get

    test <@ actual = expected @>
