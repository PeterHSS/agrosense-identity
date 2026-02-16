using Api.Domain.Entities;
using Api.Features.Users.Register;

namespace Api.Features.Users;

public static class UserMapper
{
    public static User ToEntity(this RegisterRequest request, string hashedPassword) 
        => new User(request.Name, request.Email, hashedPassword);
}
