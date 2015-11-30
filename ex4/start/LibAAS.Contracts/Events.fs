[<AutoOpen>]
module LibAAS.Contracts.Events
open System

type EventData = 
    | ItemLoaned of loan:Loan*loanDate:LoanDate*dueDate:DueDate
    | ItemRegistered of item:Item * Quantity:Quantity

type Events = AggregateId * EventData list
