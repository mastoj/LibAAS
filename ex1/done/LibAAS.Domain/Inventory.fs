module internal LibAAS.Domain.Inventory

open LibAAS.Contracts
open LibAAS.Domain.DomainTypes
open System

let handleAtInit (id, (command:RegisterInventoryItem)) = 
    [ItemRegistered(command.Item, command.Quantity)] |> ok

let executeCommand state command =
    match state, command with
    | ItemInit, (id, RegisterInventoryItem cmd) -> handleAtInit (id, cmd)
    | _ -> InvalidState "Inventory" |> fail

let evolveAtInit = function
    | _ -> raise (exn "Implement me")

let evolveOne (event:EventData) state = 
    match state with
    | _ -> raise (exn "Implement me")

let evolveSeed = {Init = ItemInit; EvolveOne = evolveOne}
