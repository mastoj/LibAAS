[<AutoOpen>]
module Events
open System

type EventData =
    | ItemLoaned of loan:Loan*loanDate:LoanDate*dueDate:DueDate
    | ItemReturned of loan:Loan*returnDate:ReturnDate
    | ItemLate of loan:Loan*returnDate:ReturnDate*numberOfDaysLate:int*fine:Fine
    | FineCreated of loan:Loan*amount:int*dueDate:DueDate
    | FinePaid of loan:Loan*amount:int*date:DateTime

type Events = AggregateId * EventData list
