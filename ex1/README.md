# Exercise 1

All the infrastructure is already in place, since the focus is on how it is to work with an application when you have set that up. If you are really curious of how it works you can browse around the code to have a look. There is probably a lot of improvements that can be done, but this will be good enough for these exercises.

The purpose of exercise 1 is to add the functionality to register items in the library.

## Adding the test

Open `InventoryTests.fs` in the test project and add the following body to the function `the item should be added`.


```fsharp
let itemIntId = newRandomInt()
let aggId = AggregateId itemIntId
let itemId = ItemId itemIntId
let book = Book {Title = Title "Magic Book"; Author = Author "JRR Tolkien"}
let item = itemId,book
let qty = Quantity.Create 10

Given defaultPreconditions
|> When (aggId, RegisterInventoryItem { Item = item; Quantity =  qty })
|> Then ([ItemRegistered(item, qty)] |> ok)
```

Of course the code doesn't compile, but we will come to that soon.

As you can see, by using F# we can name our function in a way that make the tests much more readable. Also, the "framework" with `Given`, `When` and `Then` is something implemented in `Specification.fs` for you to look at if interested.

## Adding the relevant types

The first part to get the test green is to make it compile, so let's add all the types. I try to separate between external and domain types. External types are types that the surrounding need to communicate with the domain and domain types are those that only the domain implementation needs to know about. External types are defined in the `Contracts` project.

To the `Types` module (`Types.fs`) add the following lines:

```fsharp
type Title = Title of string
type Author = Author of string
type Book = {Title: Title; Author: Author }
type Quantity = private Quantity of int
    with
        static member Create x =
            if x >= 0 then Quantity x
            else raise (exn "Invalid quantity")
type ItemData =
    | Book of Book
type Item = ItemId*ItemData
```

We use single [discriminated union](http://fsharpforfunandprofit.com/posts/discriminated-unions/) for `Title`, `Author` and `Quantity`, this will make the code more type safe since it is now impossible to switch those to values with each other as it would if they were just of type `string`. We do the same for `Quantity`, but here we take it one step further making the constructor of the case `private`, thus forcing the user to use the `Create` method to create a valid value. If you need to serialize and deserialize this value you need to add `CLIMutable` I think.

After we have the basic type we can add the command to the `Commands` module in the `Contracts` project. Replace the `RegisterInventoryItem` with the following

```fsharp
| RegisterInventoryItem of RegisterInventoryItem

and RegisterInventoryItem = {
    Item:Item
    Quantity:Quantity }
```

The last type we need to add is the event used in the test. So add that to the `Events` module. The `EventData` definition should look like this when done:

```fsharp
type EventData =
    | ItemRegistered of item:Item * Quantity:Quantity
```

Now everything should compile and we have a failing test, which mean we are ready for the implementation.

## Adding the implementation

If you run the test you'll notice that you got some kind of `Exception` with the descriptive message `Implement me`. This happened in the `executeCommand` function in `Inventory` module, so let's go there and check.

In the `Inventory` module we have skeleton implementation of what we need. The most important functions are, `evolveOne` and `executeCommand`, and these two functions will exist for each aggregate. The `evolveOne` function is the entry point for the function used when doing a `fold` over all the events. The `executeCommand` is the entry point for the action. At first we don't need to do anything with the `evolveOne` function since we don't have any events as a precondition, and that function is only called when events exists.

If you add a break point in the `executeCommand` function you'll see that the `State` is `ItemInit`. That state is defined in `DomainTypes`, and is the state we are interested in right now since nothing has happened and the `Item` should be in its initial stage. To get the test green we need to handle the command, and to address that you need to change the implementation to this:

```fsharp
let handleAtInit (id, (command:RegisterInventoryItem)) = 
    [ItemRegistered(command.Item, command.Quantity)] |> ok

let executeCommand state command =
    match state, command with
    | ItemInit, (id, RegisterInventoryItem cmd) -> handleAtInit (id, cmd)
    | _ -> raise (exn "Implement me")
```

Now we have our first function working and that finishes of exercise 1. And you can go on to [Exercise 2](../ex2/README.md).
