[<AutoOpen>]
module LibASS.Domain.CommandHandling
open LibASS.Contracts
open LibASS.Domain.Types

let validateCommand command = command |> ok

let buildState evolveSeed (aggregateId, version, events, command) =
    let evolver res e = bind (evolveSeed.EvolveOne e) res
    let state = events |> List.fold evolver (evolveSeed.Init |> ok)
    state >>= (fun s -> (aggregateId, version, s, command) |> ok)

let (|LoanCommand|InventoryCommand|) command =
    match command with
    | LoanItem _ -> LoanCommand
    | ReturnItem _ -> LoanCommand
    | PayFine _ -> LoanCommand
    | RegisterInventoryItem _ -> InventoryCommand

let commandRouteBuilder dependencies commandData =
    match commandData with
    | LoanCommand -> 
        (buildState Loan.evolveSeed)
        >=> (fun (_,_,s,command) -> Loan.executeCommand s dependencies command)
    | InventoryCommand ->
        (buildState Inventory.evolveSeed)
        >=> (fun (_,_,s,command) -> Inventory.executeCommand s command)

let executeCommand dependencies (aggregateId, currentVersion, events, command) =
    let (aggId, commandData) = command
    (aggregateId, currentVersion, events, command)
    |> commandRouteBuilder dependencies commandData
    >>= (fun es -> (aggregateId, currentVersion, es, command) |> ok)

let getEvents eventStore command = 
    let (AggregateId aggregateId, commandData) = command
    eventStore.GetEvents (StreamId aggregateId)
    >>= (fun (StreamVersion ver, events) -> (AggregateId aggregateId, ver, events, command) |> ok)

let saveEvents eventStore (AggregateId aggregateId, expectedVersion, events, command) = 
    eventStore.SaveEvents (StreamId aggregateId) (StreamVersion expectedVersion) events

let execute eventStore dependencies command = 
    command 
    |> validateCommand
    >>= getEvents eventStore
    >>= executeCommand dependencies
    >>= saveEvents eventStore
