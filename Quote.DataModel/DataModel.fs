namespace Quote.DataModel

open System

type UserId = private UserId of int with
    static member ofInt i = i |> UserId
    member __.asInt = match __ with UserId u -> u
    static member zero = 0 |> UserId
    
type QuoteId = private QuoteId of int with
    static member ofInt i = i |> QuoteId
    member __.asInt = match __ with QuoteId q -> q
    static member zero = 0 |> QuoteId


type User = {
    Id: UserId
    Name: string
}

type Quote = {
    Id: QuoteId
    UserId: UserId
    Quote: string
    AddedAt: DateTime
}
