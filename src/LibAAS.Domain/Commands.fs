[<AutoOpen>]
module Commands
type ReturnItem = { LoanId: LoanId }
type PayFine = { LoanId: LoanId; Amount: int }

type CommandData = 
    | LoanItem of LoanId * UserId * ItemId * LibraryId
    | ReturnItem of ReturnItem
    | PayFine of PayFine

type Command = AggregateId * CommandData


