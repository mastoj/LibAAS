[<AutoOpen>]
module Commands
type LoanItem = { LoanId: LoanId; UserId: UserId; ItemId: ItemId; LibraryId: LibraryId }
type ReturnItem = { LoanId: LoanId }
type PayFine = { FineId: FineId; Amount: int }

type CommandData = 
    | LoanItem of LoanItem
    | ReturnBook of ReturnItem
    | PayFine of PayFine

type Command = AggregateId * CommandData


