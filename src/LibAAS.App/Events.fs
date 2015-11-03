[<AutoOpen>]
module Events
open System

// Loan
type ItemLoaned = { Loan: Loan; LoanDate: DateTime; DueDate: DateTime }
type ItemReturned = { Loan: Loan; }
type ItemLate = { Loan: Loan; ReturnDate: DateTime; NumberOfDaysLate: int }

// Fine
type FineCreated = { FineId: FineId; Loan: Loan; Amount: int; DueDate: DateTime }
type FinePaid = { Loan: Loan; Amount: int; Date: DateTime }

type EventData =
    | ItemLoaned of ItemLoaned
    | ItemReturned of ItemReturned
    | ItemLate of ItemLate
    | FineCreated of FineCreated
    | FinePaid of FinePaid

type Event = AggregateId * EventData
