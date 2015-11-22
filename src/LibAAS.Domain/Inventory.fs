module LibASS.Domain.Inventory

open LibASS.Contracts
open LibASS.Domain.Types
open System

type InventoryState =
    | InventoryInit

let handleAtInit (id, command) = 
    match command with
    | RegisterInventoryItem(item, quantity) -> [InventoryItemRegistered(item, quantity)] |> ok
    | _ -> InvalidState "Inventory at init" |> fail

let executeCommand state command =
    match state with
    | InventoryInit -> handleAtInit command
    | _ -> InvalidState "Inventory" |> fail

let evolveOne (event:EventData) state = 
    match state with
    | _ -> InvalidStateTransition "Inventory" |> fail

let evolveSeed = {Init = InventoryInit; EvolveOne = evolveOne}