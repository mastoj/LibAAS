[<AutoOpen>]
module LibAAS.Contracts.Types
open System
 
type AggregateId = AggregateId of Guid
type LoanId = LoanId of Guid
type UserId = UserId of Guid
type ItemId = ItemId of Guid
type LibraryId = LibraryId of Guid
type Title = Title of string
type Author = Author of string
type Director = Director of string
type Fine = Fine of int
type LoanDate = LoanDate of DateTime
type DueDate = DueDate of DateTime
type ReturnDate = ReturnDate of DateTime
type Quantity = internal Quantity of int
    with 
        static member Create x = 
            if x >= 0 then Quantity x
            else raise (exn "Invalid quantity")


type Book = {Title: Title; Author: Author }
type Movie = {Title: Title; Director: Director }

type ItemData = 
    | Book of Book
    | Movie of Movie

type Error = 
    | NotImplemented of string
    | VersionConflict of string
    | InvalidStateTransition of string
    | InvalidState of string
    | InvalidItem

type Item = ItemId*ItemData

type Loan = { LoanId: LoanId; UserId: UserId; ItemId: ItemId; LibraryId: LibraryId }

type Version = int

