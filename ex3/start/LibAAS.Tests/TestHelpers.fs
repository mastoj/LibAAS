module LibAAS.Tests.TestHelpers
open LibAAS.Contracts.Types
open System

let random = new Random()
let newRandomInt() = random.Next()
let newAggId() = AggregateId (newRandomInt())
