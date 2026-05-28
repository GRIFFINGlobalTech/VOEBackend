using FHMC.Interfaces.emdb;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.Graph
{
    public class User : BaseClass
    {
        public User(object logger) : base(logger) { }

        public class EmdbUserInfoView : IEmdbUserInfoView
        {
            public string Email { get; set; }
            public bool IsOOO { get; set; }
        }

        public List<Interfaces.emdb.IEmdbUserInfoView> getOOOStatus()
        {

            List<Interfaces.emdb.IEmdbUserInfoView> retVal = new List<Interfaces.emdb.IEmdbUserInfoView>() { };

            IGraphServiceUsersCollectionPage users = graphClient.Users.Request().GetAsync().Result;

            while (users != null)
            {

                //make request
                foreach (Microsoft.Graph.User user in users)
                {

                    bool isOOO = false;

                    try
                    {
                        //first get mailsettings object
                        MailboxSettings mailSettings = getMailboxSettings(user.Id);

                        //then extract status
                        string oooStatus = getAutoReplyStatus(mailSettings);

                        if (oooStatus == null)
                        {
                            //there is not currently an ooo
                        }
                        else if (oooStatus == "disabled")
                        {
                            //there is not currently an ooo
                        }
                        else
                        {
                            //there is curently an ooo
                            isOOO = true;
                        }

                        Log.Trace(user.Mail + ":" + isOOO.ToString());

                        if (user.Mail != null)
                        {
                            retVal.Add(new EmdbUserInfoView {
                                Email = user.Mail.Replace("'", ""),
                                IsOOO = isOOO
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("GraphAPI Error", ex);
                    }


                }

                if (users != null)
                {
                    if (users.NextPageRequest != null)
                    {
                        users = users.NextPageRequest.GetAsync().Result;
                    }
                    else
                    {
                        users = null;
                    }
                }

            }

            return retVal;

        }

        private string getAutoReplyStatus(MailboxSettings mailSettings)
        {
            string retVal = null;

            if (mailSettings != null)
            {
                if (mailSettings.AutomaticRepliesSetting != null)
                {
                    if (mailSettings.AutomaticRepliesSetting.Status != null)
                    {

                        retVal = mailSettings.AutomaticRepliesSetting.Status.Value.ToString().ToLower();

                    }

                }
            }

            return retVal;

        }

        private MailboxSettings getMailboxSettings(string userId)
        {
            MailboxSettings retVal = null;

            try
            {
                string requestUrl = String.Format(@"https://graph.microsoft.com/v1.0/users/{0}/mailboxSettings", userId);

                // Create the request message
                HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                // hrm.Content = new StringContent(htmlBody, System.Text.Encoding.UTF8, "text/html");

                // Authenticate (add access token) our HttpRequestMessage
                graphClient.AuthenticationProvider.AuthenticateRequestAsync(hrm);

                // Send the request and get the response.
                HttpResponseMessage response = graphClient.HttpProvider.SendAsync(hrm).Result;

                if (response.IsSuccessStatusCode)
                {

                    string contentJSON = response.Content.ReadAsStringAsync().Result;

                    MailboxSettings mailSettings = graphClient.HttpProvider.Serializer.DeserializeObject<MailboxSettings>(contentJSON);

                    retVal = mailSettings;

                }
                else
                {

                    string httpCode = response.StatusCode.ToString();
                    string httpMessage = response.Content.ReadAsStringAsync().Result;

                    throw new GraphAPICustomException("GraphAPI Failed to Retrieve MailboxSettings for " + userId, httpCode, httpMessage);
                }
            }
            catch (GraphAPICustomException gex)
            {
                Log.Error(gex.Message, gex);
            }
            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Retrieve MailboxSettings for " + userId, ex);
                }

            }


            return retVal;
        }

    }
}
