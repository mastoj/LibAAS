[<AutoOpen>]
module LibASS.Contracts.Commands
type PayFine = { LoanId: LoanId; Amount: int }

type CommandData = 
    | LoanItem of LoanId * UserId * ItemId * LibraryId
    | ReturnItem of LoanId
    | PayFine of PayFine
    | RegisterInventoryItem of Item * Quantity

type Command = AggregateId * CommandData


