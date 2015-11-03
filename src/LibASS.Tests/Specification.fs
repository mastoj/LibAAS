module LibASS.Tests.Specification
open EventStore

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

let When spec command = {spec with Command = Some command}

let Then spec (postCondition: Result<Event list, Error>) =
    let finalSpec = {spec with PostCondition = Some postCondition}
    let eventStore = createEventStore<Event>()
    let executer = CommandHandling.execute eventStore

    let savePreConditions preCondition = 
        preCondition 
        |> List.groupBy (fun (aggId, event) -> aggId)
        |> List.iter (fun (aggId, events) -> eventStore.SaveEvents aggId 0 (events |> List.map snd) |> ignore)

    finalSpec.PreCondition |> savePreConditions
    let actual = finalSpec.Command |> Option.get |> executer
    let expected = finalSpec.PostCondition |> Option.get
    actual = expected
