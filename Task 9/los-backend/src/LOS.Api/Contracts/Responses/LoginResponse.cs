using LOS.Application.Auth.DTOs;

namespace LOS.Api.Contracts.Responses;

public record LoginResponse(
    DateTime AccessTokenExpiresAtUtc,
    UserProfileDto User,
    string LandingRoute
);
