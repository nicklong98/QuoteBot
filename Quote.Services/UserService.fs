module Quote.Services.UserService

open System
open Quote
open Quote.Dto.Users

type UserCreationRequest = {
    Name: string
}

let private typeName = "User"

let userCreationValidationRules =
    [
        fun (user:UserCreationRequest) ->
            if user.Name |> String.IsNullOrWhiteSpace then
    ]

let private mapper (user: DataModel.User) =
    { User.Id = user.Id.asInt |> UserId.ofLong
      Name = user.Name }

let getUserById logger byIdRetriever (id:UserId) =
    BaseCrudService.getById logger typeName mapper byIdRetriever id
    

