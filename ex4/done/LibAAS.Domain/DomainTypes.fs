module internal LibAAS.Domain.DomainTypes

open LibAAS.Contracts

type InventoryState =
    | ItemInit
    | ItemInStock of item:Item*quantity:Quantity

type LoanData = {Loan: Loan; LoanDate: LoanDate; DueDate: DueDate}
type LoanState = 
    | LoanInit
    | LoanCreated of LoanData

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
        GetInventoryItem: ItemId -> Result<InventoryState, Error>
    }

