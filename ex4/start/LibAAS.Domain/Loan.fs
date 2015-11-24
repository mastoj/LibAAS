module internal LibASS.Domain.Loan
open LibASS.Contracts
open LibASS.Domain.DomainTypes
open System

let handleAtInit stateGetters ((aggId:AggregateId), commandData) = 
    match commandData with
    | LoanItem (loanId, userId, itemId, libraryId) -> 
        itemId |> 
            (stateGetters.GetInventoryItem
             >=> function
                 | ItemInit -> InvalidItem |> fail
                 | _ ->
                    let loan = 
                        { LoanId = loanId
                          UserId = userId
                          ItemId = itemId
                          LibraryId = libraryId }
                    let now = DateTime.Today
                    [ItemLoaned (loan, LoanDate now, DueDate (now.AddDays(7.)))] |> ok)
    | _ -> raise (exn "Implement me")

let handleAtCreated data ((aggId:AggregateId), commandData) =
    match commandData with
    | _ -> raise (exn "Implement me")

let executeCommand state stateGetters command =
    match state with
    | LoanInit -> handleAtInit stateGetters command
    | _ -> InvalidState "Loan" |> fail

let evolveAtInit = function
    | _ -> raise (exn "Implement me")

let evolveAtCreated data = function
    | _ -> raise (exn "Implement me")

let evolveOne (event:EventData) state = 
    match state with
    | _ -> raise (exn "Implement me")

let evolveSeed = {Init = LoanInit; EvolveOne = evolveOne}
