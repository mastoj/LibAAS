[<AutoOpen>]
module EventStore

type SaveResult = 
    | Ok
    | VersionConflict

type Messages<'T> = 
    | GetEvents of AggregateId * AsyncReplyChannel<'T list option>
    | SaveEvents of AggregateId * Version * 'T list * AsyncReplyChannel<SaveResult>

let saveEvents state (id, expectedVersion, events, (replyChannel:AsyncReplyChannel<SaveResult>)) = 
    match state |> Map.tryFind id with
    | None -> 
        replyChannel.Reply(Ok)
        state |> Map.add id events
    | Some existingEvents ->
        let currentVersion = existingEvents |> List.length
        match currentVersion = expectedVersion with
        | true -> 
            replyChannel.Reply(Ok)
            state |> Map.add id (existingEvents@events)
        | false -> 
            replyChannel.Reply(VersionConflict)
            state

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
                let newState = saveEvents state (id, expectedVersion, events, replyChannel)
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
