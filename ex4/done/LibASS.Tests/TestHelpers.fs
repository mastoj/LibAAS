module LibASS.Tests.TestHelpers
open LibASS.Contracts.Types
open System

let random = new Random()
let newRandomInt() = random.Next()
let newAggId() = AggregateId (newRandomInt())


