namespace LibASS.Tests.LoanTests
open System
open LibASS.Contracts
open LibASS.Domain
open LibASS.Domain.Integration
open LibASS.Tests.Specification
open LibASS.Tests.TestHelpers
open Xunit

module ``When loaning an item`` =
    [<Fact>]
    let ``the loan should be created and due date set``() =
        let loanGuid = newGuid()
        let userId = UserId (newGuid())
        let itemId = ItemId (newGuid())
        let libraryId = LibraryId (newGuid())
        let aggId = AggregateId loanGuid
        let item = ( itemId,
                     Book 
                        { Title = Title "A book"
                          Author = Author "A author"})
        let loan = { LoanId = LoanId loanGuid
                     UserId = userId
                     ItemId = itemId
                     LibraryId = libraryId }

        let getItem _ = item |> ok
        let dependenciesBuilder d = {d with GetItem = getItem}

        Given {defaultPreconditions with dependencies = dependenciesBuilder }
        |> When (aggId, LoanItem (LoanId loanGuid, userId, itemId, libraryId))
        |> Then ([ItemLoaned 
                    ( loan,
                      LoanDate System.DateTime.Today,
                      DueDate (System.DateTime.Today.AddDays(7.)))] |> ok)

module ``When trying to loan an item that doesn't exist`` =

    [<Fact>]
    let ``the user shoul be notified``() = 
        let loanGuid = newGuid()
        let userId = UserId (newGuid())
        let itemId = ItemId (newGuid())
        let libraryId = LibraryId (newGuid())
        let aggId = AggregateId loanGuid

        let getItem _ = InvalidItem |> fail
        let dependenciesBuilder d = {d with GetItem = getItem}

        Given {defaultPreconditions with dependencies = dependenciesBuilder }
        |> When (aggId, LoanItem (LoanId loanGuid, userId, itemId, libraryId))
        |> Then (InvalidItem |> fail)