namespace LibAAS.Domain
[<AutoOpen>]
module internal CommandHandling = 
    open LibAAS.Contracts
    open LibAAS.Domain.Types

    let validateCommand command = command |> ok

    let buildState2 evolveSeed (version, events) = 
        let evolver res e = bind (evolveSeed.EvolveOne e) res
        events |> List.fold evolver (evolveSeed.Init |> ok)

    let stateBuilder evolveSeed getEvents id = 
        getEvents id >>= buildState2 evolveSeed

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

    let commandRouteBuilder stateGetters commandData =
        match commandData with
        | LoanCommand -> 
            (buildState Loan.evolveSeed)
            >=> (fun (_,_,s,command) -> Loan.executeCommand s stateGetters command)
        | InventoryCommand ->
            (buildState Inventory.evolveSeed)
            >=> (fun (_,_,s,command) -> Inventory.executeCommand s command)

    let executeCommand stateGetters (aggregateId, currentVersion, events, command) =
        let (aggId, commandData) = command
        (aggregateId, currentVersion, events, command)
        |> commandRouteBuilder stateGetters commandData
        >>= (fun es -> (aggregateId, currentVersion, es, command) |> ok)

    let getEvents2 eventStore (id) =
        eventStore.GetEvents (StreamId id)

    let getEvents eventStore command = 
        let (AggregateId aggregateId, commandData) = command
        eventStore.GetEvents (StreamId aggregateId)
        >>= (fun (StreamVersion ver, events) -> (AggregateId aggregateId, ver, events, command) |> ok)

    let saveEvents eventStore (AggregateId aggregateId, expectedVersion, events, command) = 
        eventStore.SaveEvents (StreamId aggregateId) (StreamVersion expectedVersion) events

    let createGetters eventStore = 
        {
            GetLoan = (fun (LoanId id) ->  id |> stateBuilder Loan.evolveSeed (getEvents2 eventStore))
            GetInventoryItem = (fun (ItemId id) -> id |> stateBuilder Inventory.evolveSeed (getEvents2 eventStore))
        }


module Entrypoint = 
    let execute eventStore command = 
        let stateGetters = createGetters eventStore

        command 
        |> validateCommand
        >>= getEvents eventStore
        >>= executeCommand stateGetters
        >>= saveEvents eventStore
