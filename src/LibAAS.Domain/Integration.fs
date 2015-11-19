module LibASS.Domain.Integration

open LibASS.Contracts

type Dependencies = 
    {
        GetItem: ItemId -> Result<Item,Error>
    }
