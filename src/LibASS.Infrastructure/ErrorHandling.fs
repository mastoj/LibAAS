[<AutoOpen>]
module ErrorHandling
 
type Result<'TResult, 'TError> = 
    | Success of 'TResult
    | Failure of 'TError

let bind f = function 
    | Success y -> f y
    | Failure err -> Failure err

let ok x = Success x
let fail x = Failure x

let (>>=) result func = bind func result

let (>=>) f1 f2 = f1 >> (bind f2)