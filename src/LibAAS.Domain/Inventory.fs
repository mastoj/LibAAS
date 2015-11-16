module Inventory

open LibASS.Contracts
open System

type InventoryState =
    | InventoryInit

let executeCommand state command =
    match state with
    | _ -> InvalidState |> fail

let evolveOne (event:EventData) state = 
    match state with
    | _ -> InvalidState |> fail

let init = InventoryInit