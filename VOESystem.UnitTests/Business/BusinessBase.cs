using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NUnit.Framework;
using ServiceStack.CacheAccess;
using ServiceStack.CacheAccess.Providers;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.Testing;
using VOEBackend.Interfaces;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;
using VOESystem.Data.Interfaces;

namespace VOESystem.UnitTests.Business
{
    public class BusinessBase : VOESystem.Data.Business.BusinessBase
    {

        protected FHMC.NLogWrapper.Logger logger { get; private set; }
        
        public static string AssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        public static string ProjectPath = AssemblyPath.Substring(0, AssemblyPath.IndexOf("\\bin\\"));
        public static string ResourcesPath = ProjectPath + "\\Resources\\";

        public static string baseUrl = ConfigurationManager.AppSettings["UnitTestBaseURL"].ToString();  //"http://localhost:62266/";
        public static string basePath = ConfigurationManager.AppSettings["UnitTestBasePath"].ToString();
        public static string loginUrl = ConfigurationManager.AppSettings["UnitTestLoginURL"].ToString();  //"http://localhost:62266/login";
        public static string UserName = ConfigurationManager.AppSettings["UnitTestUserName"].ToString();  //"cdesimone";
        public static string Password = ConfigurationManager.AppSettings["UnitTestPassword"].ToString();  //"";
        public static string UserFullName = ConfigurationManager.AppSettings["UnitTestUserFullName"].ToString();  //"Christine DeSimone";
        public static string UserEmail = ConfigurationManager.AppSettings["UnitTestUserEmail"].ToString();  //"Christine DeSimone";
        public static List<string> UserRoles = ConfigurationManager.AppSettings["UnitTestUserRoles"].ToString().Split(","[0]).ToList();  //"Christine DeSimone";
        
        public enum ResourcesFileNames
        {
            [System.ComponentModel.Description("BorrowerAuthForm.pdf")]
            BorrowerAuthFormPDF,
            [System.ComponentModel.Description("TestImage.jpg")]
            TestImageJpg
        }

        public BusinessBase()
        {
            
            //yes this overrides the logger in the voesystem.data base class
            logger = new FHMC.NLogWrapper.Logger(GetType().FullName);

        }

        //this is for calling service classes directly
        private BasicAppHost _serviceAppHost;
        protected BasicAppHost serviceAppHost
        {
            get
            {

                if (_serviceAppHost == null)
                {
                    _serviceAppHost = new BasicAppHost().Init();
                    _serviceAppHost.Container.Register<IDbConnectionFactory>(
                        new OrmLiteConnectionFactory(ConnectionString,
                        true, SqlServerDialect.Provider));

#if DEPLOY
                    _serviceAppHost.Container.Register<ILoanInfo>(new VOEBackend.Encompass.Loans());
                    _serviceAppHost.Container.Register<IVerifyLogin>(new VOEBackend.Encompass.Authentication());
#else
                    _serviceAppHost.Container.Register<ILoanInfo>(new VOESystem.TestClasses.TestLoanInfo());
                    _serviceAppHost.Container.Register<FHMC.Interfaces.Encompass.IVerifyLogin>(new VOESystem.TestClasses.TestVerifyLogin());
#endif

                }

                return _serviceAppHost;

            }

        }
        
        private IDbConnection _Db;
        protected IDbConnection Db
        {
            get
            {
                if (_Db == null)
                {
                    _Db = serviceAppHost.Container.Resolve<IDbConnectionFactory>().Open();
                }

                return _Db;
            }

        }

        public void LogOrderNumber(string OrderNumber)
        {
            string TestName = TestContext.CurrentContext.Test.Name;
            logger.Info(TestName + " - Order Number: " + OrderNumber);
        }
        public void LogOrderNumber(int OrderRequestId)
        {
            string OrderNumber = Db.Where<OrderDetailView>(q => q.OrderRequestId == OrderRequestId).FirstOrDefault().OrderNumber;

            string TestName = TestContext.CurrentContext.Test.Name;
            logger.Info(TestName + " - Order Number: " + OrderNumber);
        }
        public void LogLoanNumber(string LoanNumber)
        {
            string TestName = TestContext.CurrentContext.Test.Name;
            logger.Info(TestName + " - Loan Number: " + LoanNumber);
        }

        public OrderSearchResp getRandomOrderByCriteria(List<string> statusListNames, List<string> reqTypeListName, List<string> encLoanStatuses,
            List<string> employmentStatuses, int iOldestOrderDateDays = 0, string userName = null)
        {

            //remove 90 day restriction for local development
            if (IsLocalDev())
            {
                iOldestOrderDateDays = 0;
                logger.Info("Is Local Development");
            }
            

            OrderSearchResp order = getOrdersByCriteria(statusListNames, reqTypeListName, encLoanStatuses,
                employmentStatuses, iOldestOrderDateDays, userName, null, null, 10)
                .OrderBy(x => Guid.NewGuid()).FirstOrDefault();

            return order;
        }

        public string getRandomLoanNumberByCriteria(List<string> statusListNames, List<string> reqTypeListName, List<string> encLoanStatuses,
            List<string> employmentStatuses, int iOldestOrderDateDays = 0, string userName = null)
        {

            //remove 90 day restriction for local development
            if (IsLocalDev())
            {
                iOldestOrderDateDays = 0;
            }
            
            List<OrderSearchResp> orders = getOrdersByCriteria(statusListNames, reqTypeListName, encLoanStatuses,
                employmentStatuses, iOldestOrderDateDays, userName, null, null, 50);

            //if no orders throw error
            if (orders.Count == 0)
            {
                throw new Exception("No valid loans found for this test");
            }

            //if loan status is included, then get the current loan status from the database just to make sure
            bool bFoundLoan = false;

            OrderOps oOp = new OrderOps();
            OrderSearchResp order = null;

            while (!bFoundLoan)
            {
                order = orders.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                LogLoanNumber(order.LoanNumber);
                //in this case we are not specifying loan status so we don't need to check it
                if (encLoanStatuses.Count == 0)
                {
                    bFoundLoan = true;
                }
                //if this is not in one of the loan statuses then find another loan
                else
                {
                    emdbLoanInfoView loan = Db.Where<emdbLoanInfoView>(q => q.LoanNumber == order.LoanNumber).FirstOrDefault();
                    if (loan != null)
                    {
                        if (encLoanStatuses.Contains(loan.EncLoanStatus))
                        {
                            bFoundLoan = true;
                        }

                    }
                } 
            }

                

            return order.LoanNumber;

        }

        public List<OrderSearchResp> getOrdersByCriteria(List<string> statusListNames, List<string> reqTypeListName, List<string> encLoanStatuses,
            List<string> employmentStatuses, int iOldestOrderDateDays = 0, string userName = null, string LoanNumber = null, bool? IsRevision = null, 
            int RecordLimit = 0, bool? IsNonBorrower = null)
        {

            List<OrderSearchResp> retVal = new List<OrderSearchResp>() { };

            //remove 90 day restriction for local development
            if (IsLocalDev())
            {
                iOldestOrderDateDays = 0;
            }

            List<OrderSearchReq.OrderStatus> statusList = getObjectListFromNames<OrderSearchReq.OrderStatus, OrderStatus>(
                Db, statusListNames);
            List<OrderSearchReq.RequestType> reqTypeList = getObjectListFromNames<OrderSearchReq.RequestType, RequestType>(
                Db, reqTypeListName);
            List<OrderSearchReq.EncLoanStatus> loanStatusList = getBaseDataObjectListFromNames<OrderSearchReq.EncLoanStatus>(
                Db, encLoanStatuses);


            Data.Business.OrderOps Oop = new Data.Business.OrderOps();
            List<OrderSearchResp> orders = Oop.getOrdersByCriteria(Db, LoanNumber, statusList, reqTypeList, loanStatusList,
                null, null, null, null, null, false, iOldestOrderDateDays, RecordLimit, IsNonBorrower)
                .Where<OrderSearchResp>(q => q.EncLoanStatus != "" )
                .Where <OrderSearchResp>(r => !r.LoanNumber.StartsWith("8888")).ToList();  //this is for unit testing
        
            if (employmentStatuses.Count > 0)
            {
                //filter by list of employment statuses
                orders = orders.Where<OrderSearchResp>(q => Sql.In(q.EncEmploymentStatus, employmentStatuses.ToArray())).ToList();
            }

            if (userName != null)
            {
                //filter by specialist
                orders = orders.Where<OrderSearchResp>(q => q.VerificationSpecialist == userName).ToList();
            }

            if (IsRevision != null)
            {
                //filter by isrevision
                orders = orders.Where<OrderSearchResp>(q => q.IsRevision == IsRevision).ToList();
            }

            //DEBUG
            //orders = orders.Where<OrderSearchResp>(q => q.OrderRequestId == 119025).ToList();
            

            retVal = orders;



            return retVal;

        }

        public string convertFileURLtoFilePathName(string FileURL)
        {

            string retVal = String.Empty;

            string repoFolderName = Path.GetDirectoryName(RepositoryPath) + "\\";
            string urlFolderName = (baseUrl + "VOERepository/").ToLower();
            FileURL = FileURL.ToLower();

            return FileURL.Replace(urlFolderName, repoFolderName).Replace("/", @"\");

        }


        public string getRandomSSN() {

            string retVal = String.Empty;
            List<int> ssnInstances = new List<int>() { };

            while (true)
            {
                Random rnd = new Random();
                retVal = rnd.Next(100000000, 999999999).ToString("000-00-0000");

               
                //make sure this is not already in db.  if so, do it again
                ssnInstances = Db.Where<OrderRequest>(q => q.BorrowerSSN == retVal)
                                            .Select(r => r.Id).ToList();

                if (ssnInstances.Count == 0)
                {
                    break;  //this ssn is not used in teh db so we can use it
                }



            }

            return retVal;

        }


        public UserRoleView getUserDetails(string userName)
        {
            return Db.Where<UserRoleView>(q => q.UserName == userName).FirstOrDefault();
        }

        public Document getDocumentById(int DocumentId)
        {
            Document retVal = null;

            retVal = Db.Where<Document>(q => q.Id == DocumentId).FirstOrDefault();

            return retVal;


        }

        public List<T> getObjectListFromNames<T, U>(OrmLiteConnection dbConn, List<string> nameList)
            where T : IIdValueBoolListable, new()
            where U : IIdNameListable
        {

            //for instance
            //T = OrderSearchReq.OrderStatus (int Id, bool Value)
            //U = OrderStatus  (int Id, string Name)

            List<T> retVal = new List<T>() { };

            if (nameList != null)
            {
                List<U> baseDataValues = dbConn.Select<U>().ToList();

                foreach (string name in nameList)
                {

                    //U matchingItem = baseDataValues.Where<U>(q => q.Name == name).FirstOrDefault();
                    //if (matchingItem != null)
                    //{
                    //    retVal.Add(new T
                    //    {
                    //        Id = matchingItem.Id,
                    //        Value = true
                    //    });
                    //}

                    //modified to support multiple values
                    List<U> matchingItems = new List<U>();
                    matchingItems = baseDataValues.Where<U>(q => q.Name == name).ToList();
                    if (matchingItems.Count > 0)
                    {
                        foreach (U matchingItem in matchingItems)
                        {
                            retVal.Add(new T
                            {
                                Id = matchingItem.Id,
                                Value = true
                            });
                        }
                    }


                }
            }

            return retVal;
        }

        protected bool updateIsEligibleOrderAssignment(bool IsEligible)
        {
            //returns initial value
            bool retVal = Db.Where<UserRoleView>(q => q.UserName == UserName).FirstOrDefault().IsEligibleOrderAssignment;

            Db.Update<Data.DBSchema.User>(new { IsEligibleOrderAssignment = IsEligible }, q => q.UserName == UserName);

            return retVal;
        }

        protected static string ConnectionString
        {
            get
            {
                if (System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Contains("C:/Data/FirstMortgage"))
                {
                    return ConfigurationManager.ConnectionStrings["DevConnectionString"].ToString();
                }
                else if (System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Contains("VOESystemNew"))
                {
                    return ConfigurationManager.ConnectionStrings["TestConnectionString"].ToString();
                }
                else
                {
                    return ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString();
                }
            }
        }

        public class CustomException : Exception, ISerializable
        {

            public CustomException()
            {

            }
            public CustomException(string message)
                : base(message)
            {
                
            }
            public CustomException(string message, Exception inner)
                : base(message, inner)
            {
                
            }

            protected CustomException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {

            }
   
        }

 
    }

    


}
