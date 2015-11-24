module internal LibASS.Domain.Loan
open LibASS.Contracts
open LibASS.Domain.DomainTypes
open System

let handleAtInit stateGetters ((aggId:AggregateId), commandData) = 
    match commandData with
    | _ -> raise (exn "Implement me")

let handleAtCreated data ((aggId:AggregateId), commandData) =
    match commandData with
    | _ -> raise (exn "Implement me")

let executeCommand  state stateGetters command =
    match state with
    | _ -> raise (exn "Implement me")

let evolveAtInit = function
    | _ -> raise (exn "Implement me")

let evolveAtCreated data = function
    | _ -> raise (exn "Implement me")

let evolveOne (event:EventData) state = 
    match state with
    | _ -> raise (exn "Implement me")

let evolveSeed = {Init = LoanInit; EvolveOne = evolveOne}
