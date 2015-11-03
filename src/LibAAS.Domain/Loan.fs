module Loan
open System

type LoanData = {Loan: Loan; LoanDate: DateTime; DueDate: DateTime}
type LoanState = 
    | LoanCreated of LoanData
    | LateReturn of LoanData * Fine * DateTime
    | LoanPaid of LoanData * Fine
    | Returned of LoanData * DateTime
    | LoanInit

let handleAtInit ((aggId:AggregateId), commandData) = 
    match commandData with
    | LoanItem item -> 
        let i = item.ItemId,(Book { Title = Title "A book"; Author = Author "A author"})
        let loan = 
            { LoanId = item.LoanId
              UserId = item.UserId
              Item = i
              LibraryId = item.LibraryId }
        let now = DateTime.Now
        [aggId,ItemLoaned { Loan = loan; LoanDate = now; DueDate = now.AddDays(7.) }] |> ok
    | _ -> InvalidState |> fail

let executeCommand state command = 
    match state with
    | LoanInit -> handleAtInit command
    | _ -> InvalidState |> fail

let evolveAtInit = function
    | aggId, ItemLoaned item -> 
        LoanCreated {Loan = item.Loan; DueDate = item.DueDate; LoanDate = item.LoanDate} |> ok
    | _ -> InvalidState |> fail

let evolveOne (event:Event) state = 
    match state with
    | LoanInit -> evolveAtInit event
    | _ -> InvalidState |> fail

let init = LoanInit
