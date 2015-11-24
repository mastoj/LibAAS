[<AutoOpen>]
module LibASS.Contracts.Commands

type CommandData = 
    | LoanItem
    | ReturnItem
    | RegisterInventoryItem of Item * Quantity

type Command = AggregateId * CommandData


