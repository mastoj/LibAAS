[<AutoOpen>]
module EventStore
open System

type StreamId = StreamId of Guid
type StreamVersion = StreamVersion of int

type SaveResult = 
    | Ok
    | VersionConflict

type Messages<'T> = 
    | GetEvents of StreamId * AsyncReplyChannel<'T list option>
    | SaveEvents of StreamId * StreamVersion * 'T list * AsyncReplyChannel<SaveResult>

let saveEvents state (id, expectedVersion, events, (replyChannel:AsyncReplyChannel<SaveResult>)) = 
    match state |> Map.tryFind id with
    | None -> 
        replyChannel.Reply(Ok)
        state |> Map.add id events
    | Some existingEvents ->
        let currentVersion = existingEvents |> List.length |> StreamVersion
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

type EventStore<'TEvent, 'TError> = 
    {
        GetEvents: StreamId -> Result<StreamId*StreamVersion*'TEvent list, 'TError>
        SaveEvents: StreamId -> StreamVersion -> 'TEvent list -> Result<'TEvent list, 'TError>
    }
let createEventsourcingAgent<'T>() = Agent.Start(eventSourcingAgent<'T>)

let createEventStore<'TEvent, 'TError> (versionError:'TError) =
    let agent = createEventsourcingAgent<'TEvent>()
    let getEvents aggregateId = 
        let result = (fun r -> GetEvents (aggregateId, r)) |> postAsyncReply agent |> Async.RunSynchronously
        match result with
        | Some events -> (aggregateId, StreamVersion (events |> List.length), events) |> ok
        | None -> (aggregateId, StreamVersion 0, []) |> ok
    let saveEvents aggregateId expectedVersion events = 
        let result = (fun r -> SaveEvents(aggregateId, expectedVersion, events, r)) |> postAsyncReply agent |> Async.RunSynchronously
        match result with
        | Ok -> events |> ok
        | VersionConflict -> versionError |> fail

    { GetEvents = getEvents; SaveEvents = saveEvents}
