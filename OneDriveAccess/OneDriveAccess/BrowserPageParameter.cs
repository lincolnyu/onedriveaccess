namespace OneDriveAccess
{
    public class BrowserPageParameter
    {
        public BrowserPageParameter(string clientId, string returnUrl)
        {
            ClientId = clientId;
            ReturnUrl = returnUrl;
        }

        public string ClientId { get; }

        public string ReturnUrl { get; }

        public string StartDir { get; } = "/";
    }
}
