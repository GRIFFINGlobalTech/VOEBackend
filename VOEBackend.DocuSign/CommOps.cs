using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.DocuSign
{
    public class CommOps : BaseClass
    {

        private ApiClient _apiClient = null;
        private string _accountId = null;

        public CommOps() {
            setApiClient();
        }

        public ApiClient apiClient { 
            get 
            {
                if (_apiClient == null) 
                {
                    setApiClient(); 
                }

                return _apiClient;

            }
        }

        public string accountId
        {
            get
            {
                return _accountId;
            }
        }   

        private void setApiClient()
        {

            byte[] privateKeyStream = File.ReadAllBytes(PrivateKeyFilename);
            ApiClient authClient = new ApiClient(Host);

            OAuth.OAuthToken tokenInfo = authClient.RequestJWTUserToken(IntegratorKey, UserId, OAuthBasePath, privateKeyStream, ExpiresInHours);
            OAuth.UserInfo userInfo = authClient.GetUserInfo(tokenInfo.access_token);

            foreach (var item in userInfo.Accounts)
            {
                if (item.IsDefault == "true")
                {
                    //string authValue = tokenInfo.token_type + tokenInfo.access_token

                    _accountId = item.AccountId;
                    _apiClient = new ApiClient(item.BaseUri + "/restapi");
                    
                    break;
                }
            }
        }
            




    }
}
