module internal LibAAS.Domain.Loan
open LibAAS.Contracts
open LibAAS.Domain.DomainTypes
open System

let handleAtInit stateGetters ((aggId:AggregateId), (commandData:LoanItem)) = 
    commandData.ItemId |> 
        (stateGetters.GetInventoryItem
            >=> function
                | ItemInit -> InvalidItem |> fail
                | _ ->
                let loan = 
                    { LoanId = commandData.Id
                      UserId = commandData.UserId
                      ItemId = commandData.ItemId
                      LibraryId = commandData.LibraryId }
                let now = DateTime.Today
                [ItemLoaned (loan, LoanDate now, DueDate (now.AddDays(7.)))] |> ok)

let handleAtCreated data ((aggId:AggregateId), (commandData:ReturnItem)) =
    raise (exn "Implement me")

let executeCommand state stateGetters command =
    match state, command with
    | LoanInit, (id, LoanItem data) -> handleAtInit stateGetters (id, data)
    | _ -> InvalidState "Loan" |> fail

let evolveAtInit = function
    | _ -> raise (exn "Implement me")

let evolveAtCreated data = function
    | _ -> raise (exn "Implement me")

let evolveOne (event:EventData) state = 
    match state with
    | _ -> raise (exn "Implement me")

let evolveSeed = {Init = LoanInit; EvolveOne = evolveOne}
