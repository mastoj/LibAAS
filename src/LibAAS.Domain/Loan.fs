﻿module LibASS.Domain.Loan
open LibASS.Contracts
open LibASS.Domain.Integration
open System

type LoanData = {Loan: Loan; LoanDate: LoanDate; DueDate: DueDate}
type LoanState = 
    | LoanCreated of LoanData
    | LateReturn of LoanData * Fine * ReturnDate
    | LoanPaid of LoanData * Fine
    | Returned of LoanData * ReturnDate
    | LoanInit

let handleAtInit dependencies ((aggId:AggregateId), commandData) = 
    match commandData with
    | LoanItem (loanId, userId, itemId, libraryId) -> 
        dependencies.GetItem itemId
        >>= 
            fun _ ->
                let item = itemId,(Book { Title = Title "A book"; Author = Author "A author"})
                let loan = 
                    { LoanId = loanId
                      UserId = userId
                      ItemId = itemId
                      LibraryId = libraryId }
                let now = DateTime.Today
                [ItemLoaned (loan, LoanDate now, DueDate (now.AddDays(7.)))] |> ok
    | _ -> InvalidState "Loan at init" |> fail

let handleAtCreated data ((aggId:AggregateId), commandData) =
    match commandData with
    | ReturnItem cData -> 
        let now = DateTime.Now
        let (DueDate duedate) = data.DueDate
        let daysLate = (now - duedate).Days
        let fine = 10 * daysLate
        if now > duedate then 
            [ItemLate (data.Loan, ReturnDate now, daysLate, Fine fine )] |> ok
        else 
            [ItemReturned (data.Loan, ReturnDate now )] |> ok
    | _ -> InvalidState "Loan at created" |> fail

let executeCommand  state dependencies command =
    match state with
    | LoanInit -> handleAtInit dependencies command
    | LoanCreated data -> command |> handleAtCreated data
    | _ -> InvalidState "Loan" |> fail

let evolveAtInit = function
    | ItemLoaned (loan, loanDate, dueDate) -> 
        LoanCreated {Loan = loan; DueDate = dueDate; LoanDate = loanDate} |> ok
    | _ -> InvalidStateTransition "Loan at init" |> fail

let evolveAtCreated data = function
    | ItemReturned (loan, returnDate) -> Returned (data, returnDate) |> ok
    | ItemLate (loan, returnDate, daysLate, fine) -> LateReturn (data, fine, returnDate) |> ok
    | _ -> InvalidState "Loan at created" |> fail

let evolveOne (event:EventData) state = 
    match state with
    | LoanInit -> evolveAtInit event
    | LoanCreated data -> event |> evolveAtCreated data
    | _ -> InvalidStateTransition "Loan" |> fail

let init = LoanInit
