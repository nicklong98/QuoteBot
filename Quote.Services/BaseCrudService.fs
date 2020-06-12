namespace Quote.Services

open Microsoft.Extensions.Logging

module internal BaseCrudService =
    open Quote.Errors

    let private buildValidator rules =
        rules
        |> Seq.reduce (fun firstRule secondRule input ->
            match firstRule input with
            | None -> secondRule input
            | Some err -> Some err)


    let getById logger typeName mapper byIdRetriever id =
        id
        |> sprintf "Attempting to retrieve %s with id %A" typeName
        |> logger LogLevel.Trace
        match byIdRetriever id with
        | Ok retrieved ->
            retrieved
            |> sprintf "Got %s with id %A: %A" typeName id
            |> logger LogLevel.Trace
            retrieved
            |> mapper
            |> Ok
        | Error err ->
            let logError level err =
                err
                |> sprintf "Unable to retrieve %s with id %A %A" typeName id
                |> logger level
            match err with
            | SoftFailure err ->
                logError LogLevel.Warning err
                err
                |> SoftFailure
                |> Error
            | HardFailure err ->
                logError LogLevel.Error err
                err
                |> HardFailure
                |> Error

    let create logger typeName mapper creator byIdRetriever validationRules creationRequest =
        let validationEngine = buildValidator validationRules

        let logError level err =
            err
            |> sprintf "Unable to create %s: %A" typeName
            |> logger level
        typeName
        |> sprintf "Attempting to validate %s creation request"
        |> logger LogLevel.Trace
        match validationEngine creationRequest with
        | None ->
            typeName
            |> sprintf "Successfully validated %s creation request"
            |> logger LogLevel.Trace
            typeName
            |> sprintf "Attempting to create %s"
            |> logger LogLevel.Information
            match creator creationRequest with
            | Ok createdId ->
                createdId
                |> sprintf "Successfully created %s with id %A" typeName
                |> logger LogLevel.Trace
                getById logger typeName mapper byIdRetriever createdId
            | Error (SoftFailure err) ->
                err |> logError LogLevel.Warning
                err
                |> SoftFailure
                |> Error
            | Error (HardFailure err) ->
                err |> logError LogLevel.Error
                err
                |> HardFailure
                |> Error
        | Some err ->
            err
            |> sprintf "Unable to validate %s: %A" typeName
            |> logger LogLevel.Warning
            creationRequest
            |> sprintf "Unable to validate %s: %A\n%A" typeName err
            |> logger LogLevel.Trace
            Error err

    let update logger typeName mapper updater byIdRetriever validationRules id updateRequest =
        let validationEngine =
            id
            |> validationRules
            |> buildValidator

        let logError level err =
            err
            |> sprintf "Unable to update %s with id %A: %A" typeName id
            |> logger level

        typeName
        |> sprintf "Attempting to validate %s update request (id %A)" id
        |> logger LogLevel.Trace
        match validationEngine updateRequest with
        | None ->
            typeName
            |> sprintf "Successfully validated %s update request (id %A)" id
            |> logger LogLevel.Trace
            typeName
            |> sprintf "Attempting to update %s (id %A)" id
            |> logger LogLevel.Trace
            match updater id updateRequest with
            | Ok _ ->
                typeName
                |> sprintf "Successfully updated %s (id %A)" id
                |> logger LogLevel.Trace
                getById logger typeName mapper byIdRetriever id
            | Error (HardFailure err) ->
                err |> logError LogLevel.Error
                err
                |> HardFailure
                |> Error
            | Error (SoftFailure err) ->
                err |> logError LogLevel.Warning
                err
                |> SoftFailure
                |> Error
        | Some err ->
            err |> logError LogLevel.Warning
            err |> Error

    let delete logger typeName mapper deleter byIdRetriever validationRules id =
        let validationEngine = buildValidator validationRules

        let logFailure level err =
            err
            |> sprintf "Unable to delete %s with id %A: %A" typeName id
            |> logger level

        let logHardFailure err =
            err |> logFailure LogLevel.Error
        let logSoftFailure err =
            err |> logFailure LogLevel.Warning
        id
        |> sprintf "Attempting to delete %s with id %A" typeName
        |> logger LogLevel.Information
        match getById logger typeName mapper byIdRetriever id with
        | Ok toDelete ->
            id
            |> sprintf "%s id (%A) valid" typeName
            |> logger LogLevel.Trace
            match validationEngine id with
            | None ->
                match deleter id with
                | Ok _ ->
                    id
                    |> sprintf "Successfully deleted %s with id %A" typeName
                    |> logger LogLevel.Trace
                    Ok toDelete
                | Error (SoftFailure err) ->
                    err |> logSoftFailure
                    err
                    |> SoftFailure
                    |> Error
                | Error (HardFailure err) ->
                    err |> logHardFailure
                    err
                    |> HardFailure
                    |> Error
            | Some err ->
                logSoftFailure err
                Error err
        | Error (SoftFailure err) ->
            err |> logSoftFailure
            err
            |> SoftFailure
            |> Error
        | Error (HardFailure err) ->
            err |> logHardFailure
            err
            |> HardFailure
            |> Error
