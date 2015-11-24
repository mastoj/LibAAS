[<AutoOpen>]
module LibASS.Contracts.Events
open System

type EventData = 
    | ItemLoaned of loan:Loan*loanDate:LoanDate*dueDate:DueDate
    | ItemRegistered of item:Item * Quantity:Quantity
    | ItemReturned of loan:Loan*returnDate:ReturnDate
    | ItemLate of loan:Loan*returnDate:ReturnDate*numberOfDaysLate:int*fine:Fine

type Events = AggregateId * EventData list
