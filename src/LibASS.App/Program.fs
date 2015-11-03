// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System

[<EntryPoint>]
let main argv = 
    let eventStore = createEventStore<Event, Error> (Error.VersionConflict "Version conflict")
    let executer = execute eventStore

    let newGuid() = Guid.NewGuid()
    let loanItem = {LoanId = LoanId (newGuid()); UserId = UserId (newGuid()); ItemId = ItemId (newGuid()); LibraryId = LibraryId (newGuid())}
    let commandData = LoanItem loanItem
    let aggId = AggregateId (Guid.NewGuid())

    let result = (aggId, commandData) |> executer
    let returnResult = (aggId, ReturnItem { LoanId = loanItem.LoanId}) |> executer
    printfn "Result: %A" result
    printfn "Return result: %A" returnResult

    Console.ReadLine() |> ignore
    0 // return an integer exit code
