namespace LibAAS.Tests.LoanTests
open System
open LibAAS.Contracts
open LibAAS.Domain
open LibAAS.Tests.Specification
open LibAAS.Tests.TestHelpers
open Xunit

[<AutoOpen>]
module LoanTestsHelpers =
    let createLoanTestData() =
        let loanIntId = newRandomInt()
        let userId = UserId (newRandomInt())
        let itemId = ItemId (newRandomInt())
        let libraryId = LibraryId (newRandomInt())
        let aggId = AggregateId loanIntId
        aggId, { LoanId = LoanId loanIntId
                 UserId = userId
                 ItemId = itemId
                 LibraryId = libraryId }

    let today() = System.DateTime.Today

module ``When loaning an item`` =

    [<Fact>]
    let ``the loan should be created and due date set``() =
        let aggId,loan = createLoanTestData()

        let item = ( loan.ItemId,
                     Book
                        { Title = Title "A book"
                          Author = Author "A author"})

        let qty = Quantity.Create 10
        let (ItemId id) = loan.ItemId
        let itemAggId = AggregateId id

        Given {defaultPreconditions
                with
                    presets = [itemAggId, [ItemRegistered(item, qty)]]}
        |> When (aggId, LoanItem (loan.LoanId, loan.UserId, loan.ItemId, loan.LibraryId))
        |> Then ([ItemLoaned
                    ( loan,
                      LoanDate System.DateTime.Today,
                      DueDate (System.DateTime.Today.AddDays(7.)))] |> ok)

    [<Fact>]
    let ``the user shoul be notified if item doesn't exist``() =
        let aggId,loan = createLoanTestData()

        Given defaultPreconditions
        |> When (aggId, LoanItem (loan.LoanId, loan.UserId, loan.ItemId, loan.LibraryId))
        |> Then (InvalidItem |> fail)
