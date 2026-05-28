using System;
using System.Net.Http;

namespace FHMC.Graph
{
    public class FileIO : BaseClass
    {

        //string bidevsiteid = "gofirsthome.sharepoint.com,8e5bfa09-2303-4dc0-acac-5963c9f914c3,bf6ce78c-718a-49e4-bf18-518f1935805b";
        //string qadriveid = "b!CfpbjgMjwE2srFljyfkUw4znbL-KceRJvxhRjxk1gFsZ3OArrDiBTpVDsXeNUh-l";
        //string approvedFolderid = "01RL37UK3HMF5BQEYL7VB2SABKJH7ZI4Z3";
        //string titlecompaniesfileid = "01RL37UK4UQXPFTADQ2VEIPRD3KI2THUCL";

        public FileIO(object logger) : base(logger) { }

        public bool updateFile(string driveId, string fileId, string filePathName, ContentType contentType)
        {

            bool retVal = false;

            try
            {

                string siteRequestUrl = graphUrl + @"/drives/" + driveId + "/items/" + fileId + "/content";

                string response = String.Empty;

                using (var stream = System.IO.File.OpenRead(filePathName))
                {
                    var length = stream.Length;
                    var fileContents = new byte[length];
                    var result = stream.Read(fileContents, 0, (int)length);
                    response = makeGraphRequest(siteRequestUrl, HttpMethod.Put, fileContents, null, contentType);
                }

                Log.Info("File Updated: " + filePathName);
                retVal = true;

            }
            catch (Exception ex)
            {
                Log.Error("Error Updating File: " + filePathName, ex);
            }

            return retVal;  

        }
    }
}
