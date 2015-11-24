module internal LibASS.Domain.Inventory

open LibASS.Contracts
open LibASS.Domain.DomainTypes
open System

let handleAtInit (id, command) = 
    match command with
    | RegisterInventoryItem(item, quantity) -> [ItemRegistered(item, quantity)] |> ok
    | _ -> raise (exn "Implement me")

let executeCommand state command =
    match state with
    | ItemInit -> handleAtInit command
    | _ -> raise (exn "Implement me")

let evolveAtInit = function
    | _ -> raise (exn "Implement me")

let evolveOne (event:EventData) state = 
    match state with
    | _ -> raise (exn "Implement me")

let evolveSeed = {Init = ItemInit; EvolveOne = evolveOne}