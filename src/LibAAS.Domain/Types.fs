module LibASS.Domain.Types

open LibASS.Contracts

type InventoryState =
    | InventoryInit
    | InventorItemCreated of itemId:ItemId*itemData:ItemData*Quantity*Quantity

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
