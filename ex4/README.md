# Exercise 4

The return of an item quite not that hard to implement. We also only have one simple business rule, and that is that for each day an item is late a fine of 100 should be applied.

## Adding the tests

Since we should be quite familiar with the procedure by now we add the tests right away:

```fsharp
module ``When returning an item`` =
    let aggId,loan = createLoanTestData()

    let itemLoaned dueDate =
        [ ItemLoaned
            ( loan,
                LoanDate System.DateTime.Today,
                DueDate (dueDate))]

    [<Fact>]
    let ``the item is returned``() =
        Given { defaultPreconditions with
                    presets = [aggId, (itemLoaned (today().AddDays(1 |> float)))]}
        |> When (aggId, ReturnItem loan.LoanId)
        |> Then ([ItemReturned (loan, (today() |> ReturnDate))] |> ok)

    [<Fact>]
    let ``if the item is late it should be returned AND notified``() =
        [1 .. 100]
        |> List.map (fun x ->
            Given { defaultPreconditions with
                        presets = [aggId, (itemLoaned (today().AddDays(-x |> float)))]}
            |> When (aggId, ReturnItem loan.LoanId)
            |> Then ([
                        ItemLate (loan, (today() |> ReturnDate), x, Fine (x*100))
                        ] |> ok))
```

## Adding the relevant types

This time you'll have to figure out the type errors by yourself.

## Adding the implementation

I'll give you the implementation of the `evolveXXX` functions here:

```fsharp
let evolveAtInit = function
    | ItemLoaned (loan, loanDate, dueDate) ->
        LoanCreated {Loan = loan; DueDate = dueDate; LoanDate = loanDate} |> ok
    | _ -> InvalidStateTransition "Loan at init" |> fail

let evolveAtCreated data = function
    | _ -> raise (exn "Implement me")

let evolveOne (event:EventData) state =
    match state with
    | LoanInit -> evolveAtInit event
    | _ -> raise (exn "Implement me")
```

To get the test green you need to add case to the match statement in the `executeCommand` method, and you must also implement the feature in the `handleAtCreated` method. In `handleAtCreated` you should handle the command if it is of type `ReturnItem`, if not you should return `InvalidState "Loan at created" |> fail`.

That finishes this little tutorial. I hope you see the value in the tutorial and raise issues if you find any errors. Ask me if you want to know more about the infrastructure implementation.
