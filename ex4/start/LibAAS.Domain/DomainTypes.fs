module internal LibASS.Domain.DomainTypes

open LibASS.Contracts

type InventoryState =
    | ItemInit
    | ItemInStock of item:Item*quantity:Quantity

type LoanState = 
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
        GetInventoryItem: ItemId -> Result<InventoryState, Error>
    }

