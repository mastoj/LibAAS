[<AutoOpen>]
module LibAAS.Contracts.Commands

type CommandData = 
    | LoanItem of LoanId * UserId * ItemId * LibraryId
    | ReturnItem of LoanId
    | RegisterInventoryItem of Item * Quantity

type Command = AggregateId * CommandData


