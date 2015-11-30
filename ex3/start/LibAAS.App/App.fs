module LibAAS.AppBuilder

open LibAAS.Contracts
open LibAAS.Domain.DomainEntry

let createApp() = 
    let eventStore = createEventStore<EventData, Error> (Error.VersionConflict "Version conflict")
    (eventStore, execute eventStore)

