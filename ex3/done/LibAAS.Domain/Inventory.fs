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
    | ItemRegistered(item, quantity) -> ItemInStock (item, quantity) |> ok

let evolveOne (event:EventData) state = 
    match state with
    | ItemInit -> evolveAtInit event

let evolveSeed = {Init = ItemInit; EvolveOne = evolveOne}