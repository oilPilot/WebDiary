using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace WebDiary.Frontend.Models.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient httpClient) {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        
        try {
            if(_localStorage is null)
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            
            var token = await _localStorage.GetItemAsync<string>("token");
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if(token != null) {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        } catch(Exception ex) {
            Console.WriteLine($"Token exception message: {ex}");
        }

        var user = new ClaimsPrincipal(identity);
        var state = new AuthenticationState(user);
        return state;
    }
    public async Task LoginUserAsync(string token) {
        await _localStorage.SetItemAsync("token", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    public async Task LogoutUserAsync() {
        await _localStorage.RemoveItemAsync("token");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
