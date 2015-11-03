module Loan
open System

type LoanCreated = {Loan: Loan; LoanDate: DateTime; DueDate: DateTime}
type LoanState = 
    | LoanCreated of LoanCreated
    | LoanInit

let handleAtInit (aggId, commandData) = 
    match commandData with
    | LoanItem item -> 
        let i = item.ItemId,(Book { Title = Title "A book"; Author = Author "A author"})
        let loan = 
            { LoanId = item.LoanId
              UserId = item.UserId
              Item = i
              LibraryId = item.LibraryId }
        let now = DateTime.Now
        [ItemLoaned { Loan = loan; LoanDate = now; DueDate = now.AddDays(7.) }] |> ok
    | _ -> InvalidState |> fail

let executeCommand state command = 
    match state with
    | LoanInit -> handleAtInit command
    | _ -> InvalidState |> fail

let evolveAtInit = function
    | ItemLoaned item -> 
        LoanCreated {Loan = item.Loan; DueDate = item.DueDate; LoanDate = item.LoanDate} |> ok
    | _ -> InvalidState |> fail

let evolveOne event state = 
    match state with
    | LoanInit -> evolveAtInit event
    | _ -> InvalidState |> fail

let init = LoanInit
