[<AutoOpen>]
module Commands
type LoanItem = { LoanId: LoanId; UserId: UserId; ItemId: ItemId; LibraryId: LibraryId }
type ReturnItem = { LoanId: LoanId }
type PayFine = { LoanId: LoanId; Amount: int }

type CommandData = 
    | LoanItem of LoanItem
    | ReturnItem of ReturnItem
    | PayFine of PayFine

type Command = AggregateId * CommandData


