module LibASS.Tests.TestHelpers
open LibASS.Contracts.Types
open System

let newGuid() = Guid.NewGuid()
let newAggId() = AggregateId (newGuid())


