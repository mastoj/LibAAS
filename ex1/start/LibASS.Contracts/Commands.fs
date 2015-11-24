[<AutoOpen>]
module LibASS.Contracts.Commands

type CommandData = 
    | LoanItem
    | ReturnItem
    | RegisterInventoryItem

type Command = AggregateId * CommandData


