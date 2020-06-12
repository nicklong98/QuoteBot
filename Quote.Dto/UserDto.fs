module Quote.Dto.Users

type UserId = private UserId of int64 with
    member __.asLong = match __ with | UserId l -> l
    static member ofLong l = l |> UserId
    static member zero = 0L |> UserId

type User = {
    Id: UserId
    Name: string
}