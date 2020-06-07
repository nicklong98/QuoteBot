module QuoteBot.Commands

type Quote = Quote of string with
    member __.asString =
        match __ with
        | Quote q -> q
        
type Username = Username of string with
    member __.asString =
        match __ with
        | Username u -> u 

type Command =
    | AddQuote of Username*Quote
    | RetrieveRandomQuote
    | RetrieveRandomQuoteFromUser of Username

let tryParseCommandAndArgs commandIndicator (inputStr:string) =
    if inputStr.StartsWith commandIndicator then
        let parts = inputStr.Split(' ')
        let commandName = parts.[0].ToLower()
        match commandName with
        | "stash" ->
            let username = parts.[1] |> Username
            let quoteStartIndex =
                (username.asString |> inputStr.IndexOf) + username.asString.Length + 1
            let quote = inputStr.Substring quoteStartIndex |> Quote
            (username, quote) |> AddQuote |> Some
        | "get" ->
            if parts.Length < 2 then RetrieveRandomQuote |> Some
            else
                let username = parts.[1] |> Username
                username |> RetrieveRandomQuoteFromUser |> Some
        | _ -> None
    else
        None
        
