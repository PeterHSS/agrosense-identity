using Application.DTOs.Authentication;
using Domain.Entities;

namespace Application.Mappers;

public static class UserMapper
{
    public static User ToEntity(this RegisterRequest request, string hashedPassword) 
        => new User(request.Name, request.Email, hashedPassword);
}
