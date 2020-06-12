module Quote.Repository.BaseRepository

open Microsoft.Extensions.Logging
open Quote.Errors

let getById typeName logger byIdRetriever id =
    id |> sprintf "Attempting to get %s by id %A" typeName |> logger LogLevel.Trace
    match byIdRetriever id with
    | Error err ->
        match err with
        | SoftFailure (InvalidId id) ->
            id |> sprintf "Invalid %s id: %u" typeName |> logger LogLevel.Warning
            Error err
        | HardFailure (SystemFailure exn) ->
            exn |> sprintf "Critical failure when retrieving %s by id %A:\n %A" typeName id |> logger LogLevel.Critical
            Error err
    | Ok result ->
        id |> sprintf "Got %s by id %A" typeName |> logger LogLevel.Trace
        Ok result
