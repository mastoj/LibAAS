# Exercise 3

We have the functionality to add items, so let's start creating loans. We will go through the same procedure as in the last exercises. Defining the tests, and then throw in the types and implementation.

## Adding the test

The first thing we'll do is a simple helper method to create one set of sample data. In a real scenario you might want to do it in a more sophisticated than we are doing now. In the `LoanTests` module create this helper module:

```fsharp
[<AutoOpen>]
module LoanTestsHelpers =
    let createLoanTestData() =
        let loanIntId = newRandomInt()
        let userId = UserId (newRandomInt())
        let itemId = ItemId (newRandomInt())
        let libraryId = LibraryId (newRandomInt())
        let aggId = AggregateId loanIntId
        aggId, { LoanId = LoanId loanIntId
                 UserId = userId
                 ItemId = itemId
                 LibraryId = libraryId }

    let today() = System.DateTime.Today
```

Again the code doesn't compile due to some missing types. Add the types to the `Types` module in the `Contracts` project.

The id types, `UserId`, `LoanId` and `LibraryId` should be straightforward. But you also need to add a `Loan` type, and it should look like this:

```fsharp
type Loan = { LoanId: LoanId; UserId: UserId; ItemId: ItemId; LibraryId: LibraryId }
```

The type is used in the `createLoanTestData` function on the last row, but you don't see it because of type inference.

With that in place let's add the first **two** loan tests.

```fsharp
[<Fact>]
let ``the loan should be created and due date set``() =
    let aggId,loan = createLoanTestData()

    let item = ( loan.ItemId,
                 Book
                    { Title = Title "A book"
                      Author = Author "A author"})

    let qty = Quantity.Create 10
    let (ItemId id) = loan.ItemId
    let itemAggId = AggregateId id

    Given {defaultPreconditions
            with
                presets = [itemAggId, [ItemRegistered(item, qty)]]}
    |> When (aggId, LoanItem (loan.LoanId, loan.UserId, loan.ItemId, loan.LibraryId))
    |> Then ([ItemLoaned
                ( loan,
                  LoanDate System.DateTime.Today,
                  DueDate (System.DateTime.Today.AddDays(7.)))] |> ok)

[<Fact>]
let ``the user shoul be notified if item doesn't exist``() =
    let aggId,loan = createLoanTestData()
    
    Given defaultPreconditions
    |> When (aggId, LoanItem (loan.LoanId, loan.UserId, loan.ItemId, loan.LibraryId))
    |> Then (InvalidItem |> fail)
  ```

Again, it doesn't compile since we are missing the command and event definitions as well as the type for `LoanDate` and `DueDate`.

## Adding the relevant types

To get the solution to build again you need to do the following:

1. Add the command `LoanItem` to the `CommandData` discriminated union with the type arguments `LoanId * UserId * ItemId * LibraryId`.
2. Add the event `ItemLoaned` to the `EventData` discriminated union with the type arguments `Loan * LoanDate * DueDate`
3. Add the types `LoanDate` and `DueDate` to the `Types` module. They both should have type `xxxDate of DateTime`, again we create wrapper types to get more help from the compiler to stop us from doing stupid mistakes.

Everything builds and we are ready to implement the feature.

## Adding the implementation

If you look at the test we have two simple business rule, they are
1. We should get the `LoanDate` set to today and `DueDate` set to today plus 7 days.
2. We can't loan item that doesn't exist.

To handle this we don't do TDD, it would take to much time. We just past in the solution. Functions in the `Loan` module so they look like this:

```fsharp
let handleAtInit stateGetters ((aggId:AggregateId), commandData) =
    match commandData with
    | LoanItem (loanId, userId, itemId, libraryId) ->
        itemId |>
            (stateGetters.GetInventoryItem
             >=> function
                 | ItemInit -> InvalidItem |> fail
                 | _ ->
                    let loan =
                        { LoanId = loanId
                          UserId = userId
                          ItemId = itemId
                          LibraryId = libraryId }
                    let now = DateTime.Today
                    [ItemLoaned (loan, LoanDate now, DueDate (now.AddDays(7.)))] |> ok)
    | _ -> raise (exn "Implement me")

let executeCommand state stateGetters command =
    match state with
    | LoanInit -> handleAtInit stateGetters command
    | _ -> InvalidState "Loan" |> fail
```

Some points about the code above that might look strange. The operator `>=>` is not magic, it is a custom function that glues to other functions together. It is defined in [`ErrorHandling.fs`](start/LibASS.Infrastructure/ErrorHandling.fs). We need to use that one instead of the standard operator `>>` since we are using the wrapper type `Result`. There is also this `stateGetters` argument. That is a variable that holds a function to get items from the inventory, it is defined in [`CommandHandling.fs`](start/LibASS.Domain/CommandHandling.fs). Please ask if you're curious about the details.

The anonymous function

```fsharp
function
| ItemInit -> InvalidItem |> fail
| _ -> ...
```

is a short form of

```fsharp
fun x ->
    match x with
    | ItemInit -> InvalidItem |> fail
    | _ -> ...
```

So in the example code we have the input to `function` must match the output of `stateGetters.GetInventoryItem itemId`.

The last exercise ([Exercise 4](../ex4/README.md)) will deal with returning an item.
