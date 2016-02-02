using Microsoft.OneDrive.Sdk;
using System.Threading.Tasks;

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

        public async Task<bool> Connect(string clientId, string returnUrl)
        {
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
    }
}
