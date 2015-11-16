namespace LibASS.Tests.LoanTests
open System
open LibASS.Contracts
open LibASS.Tests.Specification
open Xunit

module ``When loaning an item`` =
    let newGuid() = Guid.NewGuid()
    let newAggId() = AggregateId (newGuid())

    [<Fact>]
    let ``the loan should be created and due date set`` () =
        let loanGuid = newGuid()
        let userId = UserId (newGuid())
        let itemId = ItemId (newGuid())
        let libraryId = LibraryId (newGuid())
        let aggId = AggregateId loanGuid
        let loan = { LoanId = LoanId loanGuid
                     UserId = userId
                     Item =
                        ( itemId,
                          Book { Title = Title "A book"
                                 Author = Author "A author"})
                     LibraryId = libraryId }

        Given []
        |> When (aggId, LoanItem (LoanId loanGuid, userId, itemId, libraryId))
        |> Then ([ItemLoaned 
                    ( loan,
                      LoanDate System.DateTime.Today,
                      DueDate (System.DateTime.Today.AddDays(7.)))] |> ok)