[<AutoOpen>]
module Types
open System
 
type AggregateId = AggregateId of Guid
type LoanId = LoanId of Guid
type UserId = UserId of Guid
type ItemId = ItemId of Guid
type LibraryId = LibraryId of Guid
type FineId = FineId of string
type Title = Title of string
type Author = Author of string
type Director = Director of string

type Book = {Title: Title; Author: Author }
type Movie = {Title: Title; Director: Director }

type ItemData = 
    | Book of Book
    | Movie of Movie

type Error = 
    | NotImplemented of string
    | VersionConflict of string
    | InvalidState

type Item = ItemId*ItemData

type Loan = { LoanId: LoanId; UserId: UserId; Item: Item; LibraryId: LibraryId }

type Version = int

