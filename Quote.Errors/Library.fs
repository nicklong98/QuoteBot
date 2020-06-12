namespace Quote.Errors

type SoftFailures =
    | InvalidId of id:int64
    
type HardFailures =
    | SystemFailure of innerError:exn
    
type Failures =
    | SoftFailure of SoftFailures
    | HardFailure of HardFailures
