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
    | _ -> InvalidState "Inventory" |> fail

let evolveAtInit = function
    | ItemRegistered(item, quantity) -> ItemInStock (item, quantity) |> ok

let evolveOne (event:EventData) state = 
    match state with
    | ItemInit -> evolveAtInit event

let evolveSeed = {Init = ItemInit; EvolveOne = evolveOne}