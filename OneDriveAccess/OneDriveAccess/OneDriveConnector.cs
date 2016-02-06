using Microsoft.OneDrive.Sdk;
using System;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace OneDriveAccess
{
    public class OneDriveConnector
    {
        public AccountSession AccountSession { get; private set; }

        public IOneDriveClient Client { get; private set; }

        public bool Connected
        {
            get
            {
                return Client != null && AccountSession != null;
            }
        }

        public async Task<bool> Connect()
        {
            if (Connected)
            {
                return true;
            }
            var scopes = new string[]
            {
                "wl.signin",
                "wl.offline_access",
                "onedrive.readwrite"
            };
            Client = OneDriveClientExtensions.GetUniversalClient(scopes);
            if (Client != null)
            {
                AccountSession = await Client.AuthenticateAsync();
            }
            if (!Connected)
            {
                Disconnect();
            }
            return Connected;
        }

        public void Disconnect()
        {
            Client = null;
            AccountSession = null;
        }

        public async Task<bool> SignOut()
        {
            var connected = await Connect();
            if (connected)
            {
                if (AccountSession.CanSignOut)
                {
                    await Client.SignOutAsync();
                    return await Connect();
                }
                else
                {
                    var wc = new HttpClient();
                    var getUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={AccountSession.ClientId}");
                    var responseGet = await wc.GetAsync(getUri);
                    do
                    {
                        AccountSession.ExpiresOnUtc = DateTimeOffset.UtcNow;
                    }
                    while (!AccountSession.IsExpiring()) ;
                    AccountSession.AccessToken = null;
                    AccountSession.RefreshToken = null;
                    await Client.SignOutAsync();
                }   
                Disconnect();
                return await Connect();
            }
            return false;
        }
    }
}
