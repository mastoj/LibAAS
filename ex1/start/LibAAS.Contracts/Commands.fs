[<AutoOpen>]
module LibAAS.Contracts.Commands

type CommandData = 
    | LoanItem
    | ReturnItem
    | RegisterInventoryItem

and LoanItem = unit
and ReturnItem = unit
and RegisterInventoryItem = unit

type Command = AggregateId * CommandData


