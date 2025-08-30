using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace WebDiary.Frontend.Models.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient httpClient, NavigationManager navigationManager) {
        _localStorage = localStorage;
        _httpClient = httpClient;
        _navigationManager = navigationManager;
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
                /*
                if(jwtToken.ValidTo < DateTime.Now) {
                    await CheckTokenExpiration();
                } - it is a bad idea      */
            }
        } catch(Exception ex) {
            Console.WriteLine($"Token exception message: {ex}");
        }

        var user = new ClaimsPrincipal(identity);
        var state = new AuthenticationState(user);
        return state;
    }
    public async Task LoginUserAsync(string token, string refreshToken) {
        await _localStorage.SetItemAsync("token", token);
        await _localStorage.SetItemAsync("refreshToken", refreshToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    public async Task LogoutUserAsync() {
        try {
            await _localStorage.RemoveItemAsync("token");
            await _localStorage.RemoveItemAsync("refreshToken");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        } catch (Exception ex)
        {
            Console.WriteLine($"Logout error: {ex.Message}");
        }
    }

    public async Task<bool> CheckTokenExpiration() {
        var token = await _localStorage.GetItemAsync<string>("token");
        var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
        if(string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
            return false;
        
        var response = await _httpClient.PostAsJsonAsync($"/auth/refresh?culture={CultureInfo.CurrentCulture}",
            new { RefreshToken = refreshToken, AccessToken = token } );
        if(!response.IsSuccessStatusCode) {
            await LogoutUserAsync(); // token expired or invalid
            var msg = await response.Content.ReadAsStringAsync();
            _navigationManager.NavigateTo($"/Users/Login?ErrorMessage={msg}");
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<RefreshResponse>();
        if(result == null) return false;
        await _localStorage.SetItemAsync("token", result.token);
        await _localStorage.SetItemAsync("refreshToken", result.refreshToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return true;
    }
    public async Task<HttpResponseMessage> AuthorizedRequestAsync(Func<Task<HttpResponseMessage>> action) {
        var response = await action();
        if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
            bool refreshed = await CheckTokenExpiration();
            if(refreshed) {
                response = await action();
            }
        }

        return response;
    }
    private class RefreshResponse {
        public string token { get; set; } = "";
        public string refreshToken { get; set; } = "";
    }
}
