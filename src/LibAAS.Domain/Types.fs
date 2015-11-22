module LibASS.Domain.Types

open LibASS.Contracts

type InventoryState =
    | ItemInit
    | ItemInStock of item:Item*quantity:Quantity

type LoanData = {Loan: Loan; LoanDate: LoanDate; DueDate: DueDate}
type LoanState = 
    | LoanCreated of LoanData
    | LateReturn of LoanData * Fine * ReturnDate
    | LoanPaid of LoanData * Fine
    | Returned of LoanData * ReturnDate
    | LoanInit

type EvolveOne<'T> = EventData -> 'T -> Result<'T, Error>
type EvolveSeed<'T> = 
    {
        Init: 'T
        EvolveOne: EvolveOne<'T>
    }

type InternalDependencies = 
    {
        GetItem: ItemId -> Result<InventoryState,Error>
    }

type StateGetters = 
    {
        GetLoan: LoanId -> Result<LoanState, Error>
        GetInventoryItem: ItemId -> Result<InventoryState, Error>
    }

