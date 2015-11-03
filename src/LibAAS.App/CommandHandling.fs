module CommandHandling
type AggregateDef<'TState, 'TCommand, 'TEvent> = {
    EvolveOne: 'TEvent -> 'TState -> Result<'TState, Error>
    ExecuteCommand: 'TState -> 'TCommand -> Result<'TEvent list, Error>
    Init: 'TState
}

let validateCommand command = command |> ok

let getEvents eventStore command = 
    eventStore.GetEvents (command |> fst)
    >>= (fun (aggId, ver, events) -> (aggId, ver, events, command) |> ok)

let buildState aggregateDef (aggregateId, version, events, command) =
    let evolver res e = bind (aggregateDef.EvolveOne e) res
    let state = events |> List.fold evolver (aggregateDef.Init |> ok)
    state >>= (fun s -> (aggregateId, version, s, command) |> ok)

let executeCommand aggregateDef (aggregateId, currentVersion, state, command) =
    command 
    |> (aggregateDef.ExecuteCommand state)
    >>= (fun es -> (aggregateId, currentVersion, es, command) |> ok)

let (|LoanCommand|) command =
    match command with
    | LoanItem _ -> LoanCommand
    | ReturnBook _ -> LoanCommand
    | PayFine _ -> LoanCommand

let getAggregateDef (aggregateId, command) = 
    match command with
    | LoanCommand -> 
        { EvolveOne = Loan.evolveOne
          ExecuteCommand = Loan.executeCommand
          Init = Loan.init }

let saveEvents eventStore (aggregateId, expectedVersion, events, command) = 
    eventStore.SaveEvents aggregateId expectedVersion events

let execute eventStore command = 
    let aggregateDef = getAggregateDef command

    command 
    |> validateCommand
    >>= getEvents eventStore
    >>= buildState aggregateDef
    >>= executeCommand aggregateDef
    >>= saveEvents eventStore
