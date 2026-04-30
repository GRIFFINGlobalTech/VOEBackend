using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using VOESystem.Data.Business;
using VOESystem.Services;
using VOESystem.UnitTests.Business;

namespace VOESystem.UnitTests.Tests.ServiceTests
{
    public class ServiceTestBase : UnitTests.Business.BusinessBase
    {

        [SetUp]
        public void Initialize()
        {
            
        }

        [TearDown]
        public void EndTest()
        {
            var testResult = TestContext.CurrentContext.Result.Outcome;

            if (testResult == ResultState.Failure || testResult == ResultState.Error)
            {

                string testName = TestContext.CurrentContext.Test.Name;
                string message = TestContext.CurrentContext.Result.Message;
                string stackTrace = TestContext.CurrentContext.Result.StackTrace;

                logger.Error(testName + ": " + message + "\n" + stackTrace,
                    new Exception("Test Result Error"));

            }

            
        }

        public T GetServiceInstance<T>(bool AddFile = false, List<string> usingRoles = null, string usingUser = null, string urlReferrer = null,
            string formDataJSON = null, string EquifaxTestResultMessage = null)
            where T : BaseService, new()
        {

            //mock session object
            CustomUserSession mockCustomSession = new CustomUserSession
            {
                //primary string fields
                UserAuthId = usingUser ?? UserName,
                FullName = usingUser ?? UserFullName,
                //CompanyName = "FirstHome",
                EncompassPassword = Password,
                Email = UserEmail,
                                
                //other
                Roles = UserRoles,
                IsAuthenticated = true,

                //date calcs
                NextBusinessDay = DateTime.Today.AddDays(1),
                NextBusiness5thDay = DateTime.Today.AddDays(5),
                PrevBusinessDay = DateTime.Today.AddDays(-1),
            };


            if (usingRoles != null)
            {
                //make sure session contains role
                mockCustomSession.Roles.Clear();
                mockCustomSession.Roles.AddRange(usingRoles);
                
            }

            //set permission flags
            CustomCredentialsAuthProvider.CustomUserRepository rep = new CustomCredentialsAuthProvider.CustomUserRepository();
            rep.setUserOrgPersonaForUser(ref mockCustomSession);
            rep.setUserPermissions(mockCustomSession);
            
            //mock http request
            var mockedRequestContext = new Mock<IRequestContext>();
            var mockedHttpRequest = new Mock<IHttpRequest>();
            var mockedOriginalRequest = new Mock<HttpRequestBase>();
            var mockedOriginalRequestContext = new Mock<RequestContext>();

            Uri url = new Uri(baseUrl);
            string ApplicationPath = baseUrl.Replace(url.Scheme + "://" + url.Authority, "");

            mockedOriginalRequest.SetupGet(r => r.Url).Returns(url);
            //mockedOriginalRequest.SetupGet(r => r.ApplicationPath).Returns("/");
            mockedOriginalRequest.SetupGet(r => r.ApplicationPath).Returns(ApplicationPath);
            if (urlReferrer != null)
            {
                mockedHttpRequest.SetupGet(r => r.UrlReferrer).Returns(new Uri(urlReferrer));
            }

            mockedOriginalRequest.SetupGet(x => x.RequestContext).Returns(mockedOriginalRequestContext.Object);
            mockedHttpRequest.SetupGet(x => x.OriginalRequest).Returns(mockedOriginalRequest.Object);
            
            //add mocked session
            mockedHttpRequest.SetupGet(x => x.Items).Returns(new Dictionary<string, object>() {  
                { ServiceExtensions.RequestItemsSessionKey, mockCustomSession } 
            });

            mockedHttpRequest.SetupGet(r => r.Cookies).Returns(new Dictionary<string, System.Net.Cookie>() {  
                { "ss-id", new System.Net.Cookie("ss-id", "MockedId") },
                { "ss-pid", new System.Net.Cookie("ss-pid", "MockedPid") }
            });

            if (formDataJSON != null)
            {
                mockedHttpRequest.SetupGet(r => r.FormData).Returns(new System.Collections.Specialized.NameValueCollection()
                { 
                    { "requestData", formDataJSON }
                });
            }

            mockedRequestContext.Setup(x => x.Get<IHttpRequest>()).Returns(mockedHttpRequest.Object);

            if (AddFile)
            {
                //add mocked file
                string FileName = ResourcesFileNames.TestImageJpg.GetDescription();
                string FilePathName = ResourcesPath + FileName;

                using (var mockFileStream = new FileStream(FilePathName, FileMode.Open, FileAccess.Read) ) 
                {
                    MemoryStream mockMemoryStream = new MemoryStream();
                    mockFileStream.CopyTo(mockMemoryStream);

                    var mockedFile = new Mock<IFile>();
                    mockedFile.SetupGet(x => x.ContentType).Returns("image/jpeg");
                    mockedFile.SetupGet(x => x.ContentLength).Returns(mockFileStream.Length);
                    mockedFile.SetupGet(x => x.FileName).Returns("testFileName.jpg");
                    mockedFile.SetupGet(x => x.InputStream).Returns(mockMemoryStream);

                    mockedRequestContext.SetupGet(x => x.Files).Returns(new[] { mockedFile.Object });

                }
            }

            //register and create service           
            serviceAppHost.Container.RegisterAutoWiredType(typeof(T));

            T oService = new T
            {
                RequestContext = mockedRequestContext.Object,
                BasePath = basePath,
            };
            
            return oService;
        }

        public class ServiceTestException : CustomException
        {
            protected int? OrderRequestId = null;
        }


        public static partial class ServiceTestExceptions
        {


            public class NoValidOrderForTestException : ServiceTestException
            {

                public int? OrderRequestId = null;

                public override string Message
                {
                    get
                    {
                        string msg = "No valid orders were found for this test.";
                        if (OrderRequestId != null)
                        {
                            msg += OrderRequestId.ToString();
                        }
                        return msg;
                    }
                }

                public NoValidOrderForTestException(int? orderRequestId = null)
                    : base()
                {

                    OrderRequestId = orderRequestId;

                }

            }
        }

    }
}
