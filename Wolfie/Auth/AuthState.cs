
using System;
using System.Collections.Generic;
using System.Text;
using Wolfie.Helpers;
using Wolfie.Services;

namespace Wolfie.Auth
{
    public class AuthState
    {
        //public string? LoggerString { get; set; }

        public string? AccessToken { get; set; }
        //private readonly SslClientService _client = ServiceClientHelper.GetService<SslClientService>();
        public bool IsAuthorized => AccessToken != null;

        //public async Task<string> GetLogsAsync(string access_token)
        //{
        //    try
        //    {
        //        string? _access = access_token ?? " no_token";
        //
        //        await _client.SendJsonAsync("get_access_token_data", new Dictionary<string, string>
        //        {
        //            ["token_access"] = _access
        //        });
        //        _client.MessageReceived += OnMessageReceived;
        //        await Task.Delay(500);
        //        return LoggerString ?? "no answer";
        //    }
        //    catch (Exception ex)
        //    {
        //        return ("Get loggs by access token error" + ex.Message);
        //    }
        //}
        //
        //private void OnMessageReceived(string msg)
        //{
        //    LoggerString = msg;
        //}
    }
}
