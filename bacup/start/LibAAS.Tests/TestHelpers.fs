module LibAAS.Tests.TestHelpers
open LibAAS.Contracts.Types
open System

let newGuid() = Guid.NewGuid()
let newAggId() = AggregateId (newGuid())


