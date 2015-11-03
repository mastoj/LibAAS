open System

[<AutoOpen>]
module Types = 
    type AggregateId = AggregateId of Guid
    type LoanId = LoanId of Guid
    type UserId = UserId of Guid
    type ItemId = ItemId of Guid
    type LibraryId = LibraryId of Guid
    type FineId = FineId of string
    type Title = Title of string

    type Book = {Title: Title; Author: string}
    type Movie = {Title: Title; Director: string}

    type ItemData = 
        | Book of Book
        | Movie of Movie

    type Item = ItemId*ItemData

    type Loan = { LoanId: LoanId; UserId: UserId; Item: Item; LibraryId: LibraryId }

    type Version = int

[<AutoOpen>]
module Commands =
    type LoanItem = { LoanId: LoanId; UserId: UserId; ItemId: ItemId; LibraryId: LibraryId }
    type ReturnItem = { LoanId: LoanId }
    type PayFine = { FineId: FineId; Amount: int }

    type CommandData = 
        | LoanItem of LoanItem
        | ReturnBook of ReturnItem
        | PayFine of PayFine

    type Command = AggregateId * CommandData

[<AutoOpen>]
module Events = 
    // Loan
    type ItemLoaned = { Loan: Loan; LoanDate: DateTime; DueDate: DateTime }
    type ItemReturned = { Loan: Loan; }
    type ItemLate = { Loan: Loan; ReturnDate: DateTime; NumberOfDaysLate: int }

    // Fine
    type FineCreated = { FineId: FineId; Loan: Loan; Amount: int; DueDate: DateTime }
    type FinePaid = { Loan: Loan; Amount: int; Date: DateTime }

    type EventData =
        | ItemLoaned of ItemLoaned
        | ItemReturned of ItemReturned
        | ItemLate of ItemLate
        | FineCreated of FineCreated
        | FinePaid of FinePaid

    type Event = AggregateId * EventData

[<AutoOpen>]
module ErrorHandling = 
    type Result<'TResult, 'TError> = 
        | Success of 'TResult
        | Failure of 'TError

    let bind f = function 
        | Success y -> f y
        | Failure err -> Failure err

    let ok x = Success x
    let fail x = Failure x

    let (>>=) result func = bind func result

    type Error = 
        | NotImplemented of string
        | VersionConflict of string

module Loan = 
    type LoanState = 
        | LoanCreated of Loan
        | LoanInit

    let executeCommand state command = [] |> ok
    let evolveOne event state = state |> ok
    let init = LoanInit

[<AutoOpen>]
module AgentHelper = 
    type Agent<'T> = MailboxProcessor<'T>
    let post (agent:Agent<'T>) message = agent.Post message
    let inline postAsyncReply (agent:Agent<'T>) messageConstr = agent.PostAndAsyncReply(messageConstr)

[<AutoOpen>]
module EventStore = 
    type SaveResult = 
        | Ok
        | VersionConflict

    type Messages<'T> = 
        | GetEvents of AggregateId * AsyncReplyChannel<'T list option>
        | SaveEvents of AggregateId * Version * 'T list * AsyncReplyChannel<SaveResult>

    let eventSourcingAgent<'T> (inbox:Agent<Messages<'T>>) = 
        let initState = Map.empty
        let rec loop state = 
            async {
                let! msg = inbox.Receive()
                match msg with
                | GetEvents (id, replyChannel) ->
                    let events = state |> Map.tryFind id
                    replyChannel.Reply(events)
                    return! loop state
                | SaveEvents (id, expectedVersion, events, replyChannel) ->
                    match state |> Map.tryFind id with
                    | None -> 
                        let newState = state |> Map.add id events
                        replyChannel.Reply(Ok)
                        return! loop newState
                    | Some existingEvents ->
                        let currentVersion = existingEvents |> List.length
                        let newState = 
                            match currentVersion = expectedVersion with
                            | true -> 
                                replyChannel.Reply(Ok)
                                state |> Map.add id (existingEvents@events)
                            | false -> 
                                replyChannel.Reply(VersionConflict)
                                state
                        return! loop newState
            }
        loop initState

    type EventStore<'TEvent> = 
        {
            GetEvents: AggregateId -> Result<AggregateId*Version*'TEvent list, Error>
            SaveEvents: AggregateId -> Version -> 'TEvent list -> Result<'TEvent list, Error>
        }
    let createEventsourcingAgent<'T>() = Agent.Start(eventSourcingAgent<'T>)

    let createEventStore<'TEvent>() =
        let agent = createEventsourcingAgent<'TEvent>()
        let getEvents aggregateId = 
            let result = (fun r -> GetEvents (aggregateId, r)) |> postAsyncReply agent |> Async.RunSynchronously
            match result with
            | Some events -> (aggregateId, (events |> List.length), events) |> ok
            | None -> (aggregateId, 0, []) |> ok
        let saveEvents aggregateId expectedVersion events = 
            let result = (fun r -> SaveEvents(aggregateId, expectedVersion, events, r)) |> postAsyncReply agent |> Async.RunSynchronously
            match result with
            | Ok -> events |> ok
            | VersionConflict -> (Error.VersionConflict "Unmet req. expected version") |> fail

        { GetEvents = getEvents; SaveEvents = saveEvents}


module CommandHandling = 
    type AggregateDef<'TState, 'TCommand, 'TEvent> = {
        EvolveOne: 'TEvent -> 'TState -> Result<'TState, Error>
        ExecuteCommand: 'TState -> 'TCommand -> Result<'TEvent list, Error>
        Init: 'TState
    }

    let validateCommand command = command |> ok

    let getEvents : (EventStore<Event> -> Command -> Result<AggregateId*Version*Event list*Command,Error>) = 
        fun eventStore command -> 
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


//let command = {LoanId = LoanId (Guid.NewGuid())}

//let agent = createEventStore<int>()
//let aggId = AggregateId (Guid.NewGuid())
//let res = (fun r -> SaveEvents (aggId,0,[1;2;3],r)) |> postAsyncReply agent
//res |> Async.RunSynchronously
//let res = (fun r -> GetEvents (aggId,r)) |> postAsyncReply agent
//res |> Async.RunSynchronously
//let res = (fun r -> SaveEvents (aggId,0,[4;5;6],r)) |> postAsyncReply agent
//res |> Async.RunSynchronously
//let res = (fun r -> SaveEvents (aggId,3,[4;5;6],r)) |> postAsyncReply agent
//res |> Async.RunSynchronously
//let res2 = (fun r -> SaveEvents (aggId,6,[4;5;6],r)) |> postAsyncReply agent
//res2 |> Async.RunSynchronously
//let res = (fun r -> GetEvents (aggId,r)) |> postAsyncReply agent
//let res1 = (fun r -> GetEvents (aggId,r)) |> postAsyncReply agent
//res1 |> Async.RunSynchronously

let newGuid() = Guid.NewGuid()
let loanItem = {LoanId = LoanId (newGuid()); UserId = UserId (newGuid()); ItemId = ItemId (newGuid()); LibraryId = LibraryId (newGuid())}
let commandData = LoanItem loanItem
let aggId = AggregateId (Guid.NewGuid())
let eventStore = createEventStore<Event>()
let executer = CommandHandling.execute eventStore
(aggId, commandData) |> executer