# Exercise 2

You can continue working from the solution you have or start from the `start` folder here, they should look almost the same. From now there will be a little bit less details so you need to think more what to do. If you get stuck the solution is in the `done` folder, but try first.

The next step is to add functionality to prevent us from adding an `Item` if the id is taken.

## Adding the test

I'll give you the test as starting point this time as well.

```fsharp
[<Fact>]
let ``The item should not be added if the id is not unique``() =
    let itemIntId = newRandomInt()
    let aggId = AggregateId itemIntId
    let itemId = ItemId itemIntId
    let book = Book {Title = Title "Magic Book"; Author = Author "JRR Tolkien"}
    let item = itemId,book
    let qty = Quantity.Create 10

    Given {
        defaultPreconditions
            with
            presets = [aggId, [ItemRegistered(item, qty)]] }
    |> When (aggId, RegisterInventoryItem { Item = item; Quantity =  qty })
    |> Then (InvalidState "Inventory" |> fail)
```

As you can see, this test is a little bit different. Instead of expecting a list of events, we expect `InvalidState "Inventory" |> fail`. The `fail` helper method creates a `Failure` value of type `Result` which we use to not have side effects in our functions. In regular C# programming you would most likely use an exception instead, but that is actually not a good way since that means your functions returns two different things, a return value or an exception. By using this wrapper type, we are certain that our result will always have the same type, `Result`. If you look at the previous test we piped the events to `ok` which creates a `Success` value of type `Result`.

The definition of the `Result` type and some helper functions for it can be found [here](start/LibAAS.Infrastructure/ErrorHandling.fs).

Everything compiles, that is because I've already added the `InvalidState` type, check if you can find it. Since we don't need any new types we can go straight to the implementation.

## Adding the implementation

We got almost the same starting point as last time, but this time it is in the `evolveOne` function in the `Inventory` module, so let's go there and fix it. Before we do that let's think about the test. If you read the test you see that we have some pre conditions, specified in the `Given` clause. That event should take our `Item` from `ItemInit` state to some other state, let's call it `ItemInStock`. Then, when we execute the command and we're not in the `ItemInit` state we should get the `InvalidState` result. It only make sense to register an item in if we are in the `ItemInit` state.

Update the functions in the `Inventory` module so they look like this:

```fsharp
let executeCommand state command =
    match state, command with
    | ItemInit, (id, RegisterInventoryItem cmd) -> handleAtInit (id, cmd)
    | _ -> InvalidState "Inventory" |> fail

let evolveAtInit = function
    | ItemRegistered(item, quantity) -> ItemInStock (item, quantity) |> ok

let evolveOne (event:EventData) state =
    match state with
    | ItemInit -> evolveAtInit event
```

This doesn't compile since the `ItemInStock` case doesn't exist on the `InventoryState` (I know it should probably be renames). Find the `InventoryState` and add the `ItemInStock` case.

When you have done so exercise 2 is done and you are ready for [exercise 3](../ex3/README.md).
