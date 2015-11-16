// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open LibASS.Contracts
open System

[<EntryPoint>]
let main argv = 
    let eventStore = createEventStore<EventData, Error> (Error.VersionConflict "Version conflict")
    let executer = execute eventStore

    let newGuid() = Guid.NewGuid()
    let loanId = LoanId (newGuid())
    let commandData = LoanItem(loanId, UserId (newGuid()), ItemId (newGuid()), LibraryId (newGuid()))
    let aggId = AggregateId (Guid.NewGuid())

    let result = (aggId, commandData) |> executer
    let returnResult = (aggId, ReturnItem { LoanId = loanId}) |> executer
    printfn "Result: %A" result
    printfn "Return result: %A" returnResult

    Console.ReadLine() |> ignore
    0 // return an integer exit code
