module internal LibAAS.Domain.Inventory

open LibAAS.Contracts
open LibAAS.Domain.DomainTypes
open System

let handleAtInit (id, (command:RegisterInventoryItem)) = 
    raise (exn "Implement me")

let executeCommand state command =
    match state, command with
    | _ -> raise (exn "Implement me")

let evolveAtInit = function
    | _ -> raise (exn "Implement me")

let evolveOne (event:EventData) state = 
    match state with
    | _ -> raise (exn "Implement me")

let evolveSeed = {Init = ItemInit; EvolveOne = evolveOne}