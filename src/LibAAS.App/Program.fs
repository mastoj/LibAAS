// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System

[<AutoOpen>]
module Types = 
    type LoanId = LoanId of Guid
    type UserId = UserId of Guid
    type BookId = BookId of Guid
    type LibraryId = LibraryId of Guid
    type Title = Title of string

    type Book = {BookId: BookId; Title: Title}

[<AutoOpen>]
module Commands =
    type LoanBook = { LoanId: LoanId; UserId: UserId; BookId: BookId; LibraryId: LibraryId }
    type ReturnBook = { LoanId: LoanId }

    type Command = 
        | LoanBook of LoanBook
        | ReturnBook of ReturnBook

module Events = 
    type BookLoaned = { LoanId: LoanId; UserId: UserId; Book: Book; LibraryId: LibraryId }
    type BookReturned = { LoanId: LoanId }

    type Event =
        | BookLoaned of BookLoaned
        | BookReturned of BookReturned




[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    0 // return an integer exit code
