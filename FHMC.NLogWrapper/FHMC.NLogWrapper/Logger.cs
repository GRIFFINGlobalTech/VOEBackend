using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.NLogWrapper
{
    public class Logger
    {

        private NLog.Logger _Logger;

        public Logger(string LoggerName = null)
        {

            //register loannumber custom layout renderer
            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.LoanNumberLayoutRendererWrapper>("loannumber");
            NLog.LayoutRenderers.LayoutRenderer.Register<NLog.LayoutRenderers.Wrappers.LeftCustomLayoutRendererWrapper>("left-custom");

            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.MCL.MCLRequestTypeIdLayoutRendererWrapper>("mclrequesttypeid");
            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.MCL.RequestDateTimeLayoutRendererWrapper>("requestdatetime");
            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.MCL.QueueIdLayoutRendererWrapper>("queueid");
            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.MCL.RequestFileNameLayoutRendererWrapper>("requestfilename");
            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.MCL.ResponseDateTimeLayoutRendererWrapper>("responsedatetime");
            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.MCL.MCLRequestLogIdLayoutRendererWrapper>("mclrequestlogid");
            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.MCL.ResponseFileNameLayoutRendererWrapper>("responsefilename");
            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.MCL.StatusCodeLayoutRendererWrapper>("statuscode");
            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.MCL.StatusNoteLayoutRendererWrapper>("statusnote");
            NLog.LayoutRenderers.LayoutRenderer.Register<FHMC.NLogWrapper.LayoutRendererWrappers.MCL.VendorOrderIdentifierLayoutRendererWrapper>("vendororderidentifier");

            //NLog.Targets.MailTarget emailTarget = null;

            //try
            //{
                //removed 3/28/2025 for switch to graph email send
                //emailTarget = (NLog.Targets.MailTarget)NLog.LogManager.Configuration.AllTargets
                //   .Where(q => q.Name == "email").FirstOrDefault();
                //emailTarget.SmtpPassword = @"";  //03-10-2025
                
                //if (emailTarget.SmtpPassword.ToString() == "")
                //{
                //    throw new Exception("Mail Target Password not Updated");
                //}
            //}
            //catch (Exception ex)
            //{
                //skip error where there is no mail target
                //if (emailTarget != null) { throw ex; };
            //}

            if (LoggerName == null)
            {
                _Logger = NLog.LogManager.GetCurrentClassLogger();
            }
            else
            {
                _Logger = NLog.LogManager.GetLogger(LoggerName);
            }

        }

        public void Info(string message)
        {
            _Logger.Info(message);
        }

        public void Trace(string message)
        {
            _Logger.Trace(message);
        }

        public void Info(string message, Exception ex)
        {
            _Logger.Info(message, ex);

        }

        public void Error(string message, Exception ex)
        {
            _Logger.Error(message, ex);

        }

        public void Fatal(string message, Exception ex)
        {
            _Logger.Fatal(message, ex);

        }


    }
}


