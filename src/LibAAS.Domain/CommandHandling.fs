[<AutoOpen>]
module CommandHandling

let validateCommand command = command |> ok

let buildState evolveOne init (aggregateId, version, events, command) =
    let evolver res e = bind (evolveOne e) res
    let state = events |> List.fold evolver (init |> ok)
    state >>= (fun s -> (aggregateId, version, s, command) |> ok)

let (|LoanCommand|) command =
    match command with
    | LoanItem _ -> LoanCommand
    | ReturnItem _ -> LoanCommand
    | PayFine _ -> LoanCommand

let getCommandHandler commandData =
    match commandData with
    | LoanCommand -> 
        (buildState Loan.evolveOne Loan.init)
        >=> (fun (_,_,s,command) -> Loan.executeCommand s command)

let executeCommand (aggregateId, currentVersion, events, command) =
    let (aggId, commandData) = command
    (aggregateId, currentVersion, events, command)
    |> getCommandHandler commandData
    >>= (fun es -> (aggregateId, currentVersion, es, command) |> ok)

let getEvents eventStore command = 
    let (AggregateId aggregateId, commandData) = command
    eventStore.GetEvents (StreamId aggregateId)
    >>= (fun (StreamVersion ver, events) -> (AggregateId aggregateId, ver, events, command) |> ok)

let saveEvents eventStore (AggregateId aggregateId, expectedVersion, events, command) = 
    eventStore.SaveEvents (StreamId aggregateId) (StreamVersion expectedVersion) events

let execute eventStore command = 
    command 
    |> validateCommand
    >>= getEvents eventStore
    >>= executeCommand
    >>= saveEvents eventStore
