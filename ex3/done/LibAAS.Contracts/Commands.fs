[<AutoOpen>]
module LibAAS.Contracts.Commands

type CommandData = 
    | LoanItem of LoanItem
    | ReturnItem of ReturnItem
    | RegisterInventoryItem of RegisterInventoryItem

and LoanItem = {
    Id:LoanId
    UserId:UserId
    ItemId:ItemId
    LibraryId:LibraryId }

and ReturnItem = {
    Id:LoanId }

and RegisterInventoryItem = {
    Item:Item
    Quantity:Quantity }

type Command = AggregateId * CommandData


