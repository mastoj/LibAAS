[<AutoOpen>]
module EventStore
open System

type StreamId = StreamId of int
type StreamVersion = StreamVersion of int

type SaveResult = 
    | Ok
    | VersionConflict

type Messages<'T> = 
    | GetEvents of StreamId * AsyncReplyChannel<'T list option>
    | SaveEvents of StreamId * StreamVersion * 'T list * AsyncReplyChannel<SaveResult>
    | AddSubscriber of string * (StreamId * 'T list -> unit)
    | RemoveSubscriber of string

type internal EventStoreState<'TEvent,'THandler> = 
    {
        EventHandler: 'THandler
        GetEvents: 'THandler -> StreamId -> ('TEvent list option * 'THandler) 
        SaveEvents: 'THandler -> StreamId -> StreamVersion -> 'TEvent list -> (SaveResult * 'THandler)
        Subscribers: Map<string, (StreamId * 'TEvent list -> unit)>
    }

let eventStoreAgent<'T, 'TEventHandler> (eventHandler:'TEventHandler) getEvents saveEvents (inbox:Agent<Messages<'T>>) = 
    let initState = 
        {
            EventHandler = eventHandler
            Subscribers = Map.empty
            GetEvents = getEvents
            SaveEvents = saveEvents
        }
    let rec loop state = 
        async {
            let! msg = inbox.Receive()
            match msg with
            | GetEvents (id, replyChannel) ->
                let (events, newHandler) = state.GetEvents state.EventHandler id
                replyChannel.Reply(events)
                return! loop {state with EventHandler = newHandler}
            | SaveEvents (id, expectedVersion, events, replyChannel) ->
                let (result, newHandler) = state.SaveEvents state.EventHandler id expectedVersion events
                if result = Ok then state.Subscribers |> Map.iter (fun _ sub -> sub(id, events)) else ()
                replyChannel.Reply(result)
                return! loop {state with EventHandler = newHandler}
            | AddSubscriber (subId, subFunction) ->
                let newState = {state with Subscribers = (state.Subscribers |> Map.add subId subFunction)}
                return! loop newState
            | RemoveSubscriber subId ->
                let newState = {state with Subscribers = (state.Subscribers |> Map.remove subId )}
                return! loop newState
        }
    loop initState

let createEventStoreAgent<'TEvent, 'TEventHandler> eventHandler getEvents saveEvents = 
    Agent.Start(eventStoreAgent<'TEvent, 'TEventHandler> eventHandler getEvents saveEvents)

type EventStore<'TEvent, 'TError> = 
    {
        GetEvents: StreamId -> Result<StreamVersion*'TEvent list, 'TError>
        SaveEvents: StreamId -> StreamVersion -> 'TEvent list -> Result<'TEvent list, 'TError>
        AddSubscriber: string -> (StreamId * 'TEvent list -> unit) -> unit
        RemoveSubscriber: string -> unit
    }

let createEventStore<'TEvent, 'TError> (versionError:'TError) agent =
    let getEvents streamId : Result<StreamVersion*'TEvent list, 'TError> = 
        let result = (fun r -> GetEvents (streamId, r)) |> postAsyncReply agent |> Async.RunSynchronously
        match result with
        | Some events -> (StreamVersion (events |> List.length), events) |> ok
        | None -> (StreamVersion 0, []) |> ok

    let saveEvents streamId expectedVersion events : Result<'TEvent list, 'TError> = 
        let result = (fun r -> SaveEvents(streamId, expectedVersion, events, r)) |> postAsyncReply agent |> Async.RunSynchronously
        match result with
        | Ok -> events |> ok
        | VersionConflict -> versionError |> fail

    let addSubscriber subId subscriber = 
        (subId,subscriber) |> AddSubscriber |> post agent

    let removeSubscriber subId = 
        subId |> RemoveSubscriber |> post agent

    { GetEvents = getEvents; SaveEvents = saveEvents; AddSubscriber = addSubscriber; RemoveSubscriber = removeSubscriber}

let createInMemoryEventStore<'TEvent, 'TError> (versionError:'TError) =
    let initState : Map<StreamId, 'TEvent list> = Map.empty

    let saveEventsInMap map id expectedVersion events = 
        match map |> Map.tryFind id with
        | None -> 
            (Ok, map |> Map.add id events)
        | Some existingEvents ->
            let currentVersion = existingEvents |> List.length |> StreamVersion
            match currentVersion = expectedVersion with
            | true -> 
                (Ok, map |> Map.add id (existingEvents@events))
            | false -> 
                (VersionConflict, map)

    let getEventsInMap map id = Map.tryFind id map, map

    let agent = createEventStoreAgent initState getEventsInMap saveEventsInMap
    createEventStore<'TEvent, 'TError> versionError agent