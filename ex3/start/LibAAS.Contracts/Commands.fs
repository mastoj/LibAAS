[<AutoOpen>]
module LibAAS.Contracts.Commands

type CommandData = 
    | LoanItem of LoanItem
    | ReturnItem of ReturnItem
    | RegisterInventoryItem of RegisterInventoryItem

and LoanItem = unit

and ReturnItem = unit

and RegisterInventoryItem = {
    Item:Item
    Quantity:Quantity }

type Command = AggregateId * CommandData