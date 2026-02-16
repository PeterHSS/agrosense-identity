using Api.Domain.Entities;
using Api.Features.Users.Register;

namespace Api.Features.Users;

public static class UserMapper
{
    public static User ToUser(this RegisterRequest request, string hashedPassword) 
        => new(request.Name, request.Email, hashedPassword);
}
