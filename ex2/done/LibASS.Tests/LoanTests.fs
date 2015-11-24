namespace LibASS.Tests.LoanTests
open System
open LibASS.Contracts
open LibASS.Domain
open LibASS.Tests.Specification
open LibASS.Tests.TestHelpers
open Xunit

[<AutoOpen>]
module LoanTestsHelpers = ()
//    let createLoanTestData() = 
//        let loanGuid = newGuid()
//        let userId = UserId (newGuid())
//        let itemId = ItemId (newGuid())
//        let libraryId = LibraryId (newGuid())
//        let aggId = AggregateId loanGuid
//        aggId, { LoanId = LoanId loanGuid
//                 UserId = userId
//                 ItemId = itemId
//                 LibraryId = libraryId }
//
//    let today() = System.DateTime.Today

module ``When loaning an item`` =

    let implementME = ()