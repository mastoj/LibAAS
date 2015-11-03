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

let combine f1 f2 = 
    fun x -> x |> f1 >>= f2

let (>>+) f1 f2 = combine f1 f2