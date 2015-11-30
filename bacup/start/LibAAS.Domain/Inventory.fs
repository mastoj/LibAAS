module internal LibAAS.Domain.Inventory

open LibAAS.Contracts
open LibAAS.Domain.Types
open System

let handleAtInit (id, command) = 
    match command with
    | RegisterInventoryItem(item, quantity) -> [ItemRegistered(item, quantity)] |> ok
    | _ -> InvalidState "Inventory at init" |> fail

let executeCommand state command =
    match state with
    | ItemInit -> handleAtInit command
    | _ -> InvalidState "Inventory" |> fail

let evolveAtInit = function
    | ItemRegistered (item,quantity) ->
        ItemInStock (item, quantity) |> ok
    | _ -> InvalidStateTransition "Item at init" |> fail

let evolveOne (event:EventData) state = 
    match state with
    | ItemInit -> evolveAtInit event
    | _ -> InvalidStateTransition "Inventory" |> fail

let evolveSeed = {Init = ItemInit; EvolveOne = evolveOne}