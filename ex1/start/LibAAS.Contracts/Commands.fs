[<AutoOpen>]
module LibAAS.Contracts.Commands

type CommandData = 
    | LoanItem
    | ReturnItem
    | RegisterInventoryItem

type Command = AggregateId * CommandData


