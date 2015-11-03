module Loan
open System

type LoanData = {Loan: Loan; LoanDate: LoanDate; DueDate: DueDate}
type LoanState = 
    | LoanCreated of LoanData
    | LateReturn of LoanData * Fine * DateTime
    | LoanPaid of LoanData * Fine
    | Returned of LoanData * DateTime
    | LoanInit

let handleAtInit ((aggId:AggregateId), commandData) = 
    match commandData with
    | LoanItem (loanId, userId, itemId, libraryId) -> 
        let i = itemId,(Book { Title = Title "A book"; Author = Author "A author"})
        let loan = 
            { LoanId = loanId
              UserId = userId
              Item = i
              LibraryId = libraryId }
        let now = DateTime.Today
        [aggId,ItemLoaned (loan, LoanDate now, DueDate (now.AddDays(7.)))] |> ok
    | _ -> InvalidState |> fail

let handleAtCreated data ((aggId:AggregateId), commandData) =
    match commandData with
    | ReturnItem cData -> 
        let now = DateTime.Now
        let (DueDate duedate) = data.DueDate
        let daysLate = (now - duedate).Days
        let fine = 10 * daysLate
        match now > duedate with
        | true -> 
            [aggId, ItemLate { Loan = data.Loan; ReturnDate = now; NumberOfDaysLate = daysLate; Fine = Fine fine }] |> ok
        | false ->
            [aggId, ItemReturned { Loan = data.Loan; ReturnDate = now }] |> ok
    | _ -> InvalidState |> fail

let executeCommand state command =
    match state with
    | LoanInit -> handleAtInit command
    | LoanCreated data -> command |> handleAtCreated data
    | _ -> InvalidState |> fail

let evolveAtInit = function
    | aggId, ItemLoaned (loan, loanDate, dueDate) -> 
        LoanCreated {Loan = loan; DueDate = dueDate; LoanDate = loanDate} |> ok
    | _ -> InvalidState |> fail

let evolveAtCreated data = function
    | aggId, ItemReturned eData -> Returned (data, eData.ReturnDate) |> ok
    | aggId, ItemLate eData -> LateReturn (data, eData.Fine, eData.ReturnDate) |> ok
    | _ -> InvalidState |> fail

let evolveOne (event:Event) state = 
    match state with
    | LoanInit -> evolveAtInit event
    | LoanCreated data -> event |> evolveAtCreated data
    | _ -> InvalidState |> fail

let init = LoanInit
