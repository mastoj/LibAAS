namespace LibASS.Tests.LoanTests
open System
open LibASS.Contracts
open LibASS.Domain
open LibASS.Domain.Integration
open LibASS.Tests.Specification
open LibASS.Tests.TestHelpers
open Xunit

[<AutoOpen>]
module LoanTestsHelpers = 
    let createLoanTestData() = 
        let loanGuid = newGuid()
        let userId = UserId (newGuid())
        let itemId = ItemId (newGuid())
        let libraryId = LibraryId (newGuid())
        let aggId = AggregateId loanGuid
        aggId, { LoanId = LoanId loanGuid
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

        let getItem _ = item |> ok
        let dependenciesBuilder d = {d with GetItem = getItem}

        Given {defaultPreconditions with dependencies = dependenciesBuilder }
        |> When (aggId, LoanItem (loan.LoanId, loan.UserId, loan.ItemId, loan.LibraryId))
        |> Then ([ItemLoaned 
                    ( loan,
                      LoanDate System.DateTime.Today,
                      DueDate (System.DateTime.Today.AddDays(7.)))] |> ok)

    [<Fact>]
    let ``the user shoul be notified if item doesn't exist``() = 
        let aggId,loan = createLoanTestData()

        let getItem _ = InvalidItem |> fail
        let dependenciesBuilder d = {d with GetItem = getItem}

        Given {defaultPreconditions with dependencies = dependenciesBuilder }
        |> When (aggId, LoanItem (loan.LoanId, loan.UserId, loan.ItemId, loan.LibraryId))
        |> Then (InvalidItem |> fail)

module ``When returning an item`` =
    let aggId,loan = createLoanTestData()

    let itemLoaned dueDate = 
        [ ItemLoaned 
            ( loan,
                LoanDate System.DateTime.Today,
                DueDate (dueDate))]

    [<Fact>]
    let ``the item is returned``() =
        Given { defaultPreconditions with 
                    presets = [aggId, (itemLoaned (today().AddDays(1 |> float)))]}
        |> When (aggId, ReturnItem loan.LoanId)
        |> Then ([ItemReturned (loan, (today() |> ReturnDate))] |> ok)

    [<Fact>]
    let ``if the item is late it should be returned AND notified``() = 
        Given { defaultPreconditions with 
                    presets = [aggId, (itemLoaned (today().AddDays(-1 |> float)))]}
        |> When (aggId, ReturnItem loan.LoanId)
        |> Then ([
                    ItemLate (loan, (today() |> ReturnDate), 1, Fine 100)
                    ] |> ok)
        