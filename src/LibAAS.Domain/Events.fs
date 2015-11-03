[<AutoOpen>]
module Events
open System

// Loan
type ItemLoaned = { Loan: Loan; LoanDate: DateTime; DueDate: DateTime }
type ItemReturned = { Loan: Loan; ReturnDate: DateTime }
type ItemLate = { Loan: Loan; ReturnDate: DateTime; NumberOfDaysLate: int; Fine: Fine }
type FineCreated = { Loan: Loan; Amount: int; DueDate: DateTime }
type FinePaid = { Loan: Loan; Amount: int; Date: DateTime }

type EventData =
    | ItemLoaned of Loan*LoanDate*DueDate
    | ItemReturned of ItemReturned
    | ItemLate of ItemLate
    | FineCreated of FineCreated
    | FinePaid of FinePaid

type Event = AggregateId * EventData
