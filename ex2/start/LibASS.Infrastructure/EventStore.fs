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
    | AddSubscriber of string * (StreamId * 'T list -> unit)
    | RemoveSubscriber of string

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

type internal EventStoreState<'T> = {Events: Map<StreamId, 'T list>; Subscribers: Map<string, (StreamId * 'T list -> unit)>}
let eventSourcingAgent<'T> (inbox:Agent<Messages<'T>>) = 
    let initState = {Events = Map.empty; Subscribers = Map.empty}
    let rec loop state = 
        async {
            let! msg = inbox.Receive()
            match msg with
            | GetEvents (id, replyChannel) ->
                let events = state.Events |> Map.tryFind id
                replyChannel.Reply(events)
                return! loop state
            | SaveEvents (id, expectedVersion, events, replyChannel) ->
                let newEventMap = saveEvents state.Events (id, expectedVersion, events, replyChannel)
                let newState = {state with Events = newEventMap}

                state.Subscribers |> Map.iter (fun _ sub -> sub(id, events))

                return! loop newState
            | AddSubscriber (subId, subFunction) ->
                let newState = {state with Subscribers = (state.Subscribers |> Map.add subId subFunction)}
                return! loop newState
            | RemoveSubscriber subId ->
                let newState = {state with Subscribers = (state.Subscribers |> Map.remove subId )}
                return! loop newState
        }
    loop initState

type EventStore<'TEvent, 'TError> = 
    {
        GetEvents: StreamId -> Result<StreamVersion*'TEvent list, 'TError>
        SaveEvents: StreamId -> StreamVersion -> 'TEvent list -> Result<'TEvent list, 'TError>
        AddSubscriber: string -> (StreamId * 'TEvent list -> unit) -> unit
        RemoveSubscriber: string -> unit
    }
let createEventsourcingAgent<'T>() = Agent.Start(eventSourcingAgent<'T>)

let createEventStore<'TEvent, 'TError> (versionError:'TError) =
    let agent = createEventsourcingAgent<'TEvent>()
    let getEvents streamId = 
        let result = (fun r -> GetEvents (streamId, r)) |> postAsyncReply agent |> Async.RunSynchronously
        match result with
        | Some events -> (StreamVersion (events |> List.length), events) |> ok
        | None -> (StreamVersion 0, []) |> ok

    let saveEvents streamId expectedVersion events = 
        let result = (fun r -> SaveEvents(streamId, expectedVersion, events, r)) |> postAsyncReply agent |> Async.RunSynchronously
        match result with
        | Ok -> events |> ok
        | VersionConflict -> versionError |> fail

    let addSubscriber subId subscriber = 
        (subId,subscriber) |> AddSubscriber |> post agent

    let removeSubscriber subId = 
        subId |> RemoveSubscriber |> post agent

    { GetEvents = getEvents; SaveEvents = saveEvents; AddSubscriber = addSubscriber; RemoveSubscriber = removeSubscriber}
