module LibASS.AppBuilder

open LibASS.Contracts
open LibASS.Domain.DomainEntry

let createApp() = 
    let eventStore = createEventStore<EventData, Error> (Error.VersionConflict "Version conflict")
    (eventStore, execute eventStore)

