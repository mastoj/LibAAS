[<AutoOpen>]
module CommandHandling
type AggregateDef<'TState, 'TCommand, 'TEvent> = {
    EvolveOne: 'TEvent -> 'TState -> Result<'TState, Error>
    ExecuteCommand: 'TState -> 'TCommand -> Result<'TEvent list, Error>
    Init: 'TState
}

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
        >>+ (fun (_,_,s,command) -> Loan.executeCommand s command)

let executeCommand (aggregateId, currentVersion, events, command) =
    let (aggId, commandData) = command
    (aggregateId, currentVersion, events, command)
    |> getCommandHandler commandData
    >>= (fun es -> (aggregateId, currentVersion, es, command) |> ok)

let getEvents eventStore (command:Command) = 
    let (AggregateId aggregateId, commandData) = command
    eventStore.GetEvents (StreamId aggregateId)
    >>= (fun (StreamId aggId, StreamVersion ver, events) -> (AggregateId aggId, ver, events, command) |> ok)

let saveEvents eventStore (AggregateId aggregateId, expectedVersion, events, command) = 
    eventStore.SaveEvents (StreamId aggregateId) (StreamVersion expectedVersion) events

let execute (eventStore:EventStore<Event,Error>) (command:Command) = 
    command 
    |> validateCommand
    >>= getEvents eventStore
    >>= executeCommand
    >>= saveEvents eventStore
