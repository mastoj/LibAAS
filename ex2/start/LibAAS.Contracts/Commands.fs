[<AutoOpen>]
module LibAAS.Contracts.Commands

type CommandData = 
    | LoanItem
    | ReturnItem
    | RegisterInventoryItem of Item * Quantity

type Command = AggregateId * CommandData


