[<AutoOpen>]
module LibASS.Contracts.Commands
type ReturnItem = { LoanId: LoanId }
type PayFine = { LoanId: LoanId; Amount: int }

type CommandData = 
    | LoanItem of LoanId * UserId * ItemId * LibraryId
    | ReturnItem of ReturnItem
    | PayFine of PayFine
    | RegisterInventoryItem of Item

type Command = AggregateId * CommandData


