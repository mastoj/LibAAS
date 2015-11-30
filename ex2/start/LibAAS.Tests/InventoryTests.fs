namespace LibAAS.Tests.InventoryTests
open System
open LibAAS.Contracts
open LibAAS.Tests.Specification
open LibAAS.Tests.TestHelpers
open Xunit

module ``When add an item to the inventory`` =

    [<Fact>]
    let ``the item should be added``() =
        let itemIntId = newRandomInt()
        let aggId = AggregateId itemIntId
        let itemId = ItemId itemIntId
        let book = Book {Title = Title "Magic Book"; Author = Author "JRR Tolkien"}
        let item = itemId,book
        let qty = Quantity.Create 10

        Given defaultPreconditions
        |> When (aggId, RegisterInventoryItem (item, qty))
        |> Then ([ItemRegistered(item, qty)] |> ok)
