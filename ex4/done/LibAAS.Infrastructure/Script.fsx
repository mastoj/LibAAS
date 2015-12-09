#load "AgentHelper.fs"
#load "ErrorHandling.fs"
#load "EventStore.fs"

open EventStore

let inMemoryEventStore = createInMemoryEventStore<string,string> "This is a version error"
inMemoryEventStore.AddSubscriber "FirstSubscriber" (printfn "%A")
let res0 = inMemoryEventStore.SaveEvents (StreamId 1) (StreamVersion 0) ["Hello";"World"]
let res1 = inMemoryEventStore.SaveEvents (StreamId 1) (StreamVersion 1) ["Hello2";"World2"]
let res2 = inMemoryEventStore.SaveEvents (StreamId 1) (StreamVersion 2) ["Hello2";"World2"]

[res0;res1;res2] |> List.mapi (fun i v -> printfn "%i: %A" i v)