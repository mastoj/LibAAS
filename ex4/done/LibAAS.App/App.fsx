#I "bin/Debug"

#r "LibAAS.Contracts"
#r "LibAAS.Infrastructure"
#r "LibAAS.Domain"

#load "App.fs"
open LibAAS.AppBuilder
open LibAAS.Contracts

let (eventStore,app) = createApp()

let itemId = ItemId 4
let item = ( itemId,
                Book { 
                    Title = Title "A book"
                    Author = Author "A author"})

let qty = Quantity.Create 10
let aggId = AggregateId 4
let command = aggId, RegisterInventoryItem (item, qty)
let result = command |> app
printfn "Result %A" result
