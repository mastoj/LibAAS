module LibASS.Domain.DomainEntry
open LibASS.Domain.CommandHandling

let execute eventStore command = 
    let stateGetters = createGetters eventStore

    command 
    |> validateCommand
    >>= getEvents eventStore
    >>= executeCommand stateGetters
    >>= saveEvents eventStore
