using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FHMC.NLogWrapper.LayoutRendererWrappers.MCL
{

    [AppDomainFixedOutput]
    [ThreadAgnostic]
    [ThreadSafe]
    public class MCLLayoutRendererWrapper : NLog.LayoutRenderers.Wrappers.WrapperLayoutRendererBase
    {

        public string extractValue(string valueName, string extractFrom)
        {
            string value = extractFrom.Split("|"[0]).ToList().Where(q => q.Contains(valueName)).FirstOrDefault().ToString();
            string retVal = value.Replace(valueName + "=", "");
           
            return retVal;
        }

        protected override string Transform(string text)
        {
            throw new NotSupportedException();
        }

    }

    //MCLRequestLog log = new MCLRequestLog
    //{
    //    MCLRequestTypeId = MCLRequestTypeId,
    //    RequestDateTime = RequestDateTime,
    //    UDNQueueId = UDNQueueId ?? 0,
    //    RequestFileName = RequestFileName
    //};

    [LayoutRenderer("MCLRequestTypeId")]
    public class MCLRequestTypeIdLayoutRendererWrapper : MCLLayoutRendererWrapper
    {

        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string varName = "MCLRequestTypeId";

            //string ExtractFrom = Inner.Render(logEvent);
            string ExtractFrom = logEvent.Message;

            if (ExtractFrom.Contains(varName))
            {
                string value = extractValue(varName, ExtractFrom);
                builder.Append(value);
            }

        }

    }

    [LayoutRenderer("RequestDateTime")]
    public class RequestDateTimeLayoutRendererWrapper : MCLLayoutRendererWrapper
    {

        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string varName = "RequestDateTime";

            //string ExtractFrom = Inner.Render(logEvent);
            string ExtractFrom = logEvent.Message;

            if (ExtractFrom.Contains(varName))
            {
                string value = extractValue(varName, ExtractFrom);
                builder.Append(value);
            }

        }

    }

    [LayoutRenderer("QueueId")]
    public class QueueIdLayoutRendererWrapper : MCLLayoutRendererWrapper
    {

        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string varName = "QueueId";

            //string ExtractFrom = Inner.Render(logEvent);
            string ExtractFrom = logEvent.Message;

            if (ExtractFrom.Contains(varName))
            {
                string value = extractValue(varName, ExtractFrom);
                builder.Append(value);
            }

        }

    }

    [LayoutRenderer("RequestFileName")]
    public class RequestFileNameLayoutRendererWrapper : MCLLayoutRendererWrapper
    {

        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string varName = "RequestFileName";

            //string ExtractFrom = Inner.Render(logEvent);
            string ExtractFrom = logEvent.Message;

            if (ExtractFrom.Contains(varName))
            {
                string value = extractValue(varName, ExtractFrom);
                builder.Append(value);
            }

        }

    }


    //MCLResponseLog log = new MCLResponseLog
    //{
    //    ResponseDateTime = ResponseDateTime,
    //    MCLRequestLogId = MCLRequestLogId,
    //    ResponseFileName = ResponseFileName,
    //    StatusCode = StatusCode,
    //    StatusNote = StatusNote,
    //    VendorOrderIdentifier = VendorOrderIdentifier
    //};

    [LayoutRenderer("ResponseDateTime")]
    public class ResponseDateTimeLayoutRendererWrapper : MCLLayoutRendererWrapper
    {

        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string varName = "ResponseDateTime";

            //string ExtractFrom = Inner.Render(logEvent);
            string ExtractFrom = logEvent.Message;

            if (ExtractFrom.Contains(varName))
            {
                string value = extractValue(varName, ExtractFrom);
                builder.Append(value);
            }

        }

    }

    [LayoutRenderer("MCLRequestLogId")]
    public class MCLRequestLogIdLayoutRendererWrapper : MCLLayoutRendererWrapper
    {

        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string varName = "MCLRequestLogId";

            //string ExtractFrom = Inner.Render(logEvent);
            string ExtractFrom = logEvent.Message;

            if (ExtractFrom.Contains(varName))
            {
                string value = extractValue(varName, ExtractFrom);
                builder.Append(value);
            }

        }

    }

    [LayoutRenderer("ResponseFileName")]
    public class ResponseFileNameLayoutRendererWrapper : MCLLayoutRendererWrapper
    {

        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string varName = "ResponseFileName";

            //string ExtractFrom = Inner.Render(logEvent);
            string ExtractFrom = logEvent.Message;

            if (ExtractFrom.Contains(varName))
            {
                string value = extractValue(varName, ExtractFrom);
                builder.Append(value);
            }

        }

    }

    [LayoutRenderer("StatusCode")]
    public class StatusCodeLayoutRendererWrapper : MCLLayoutRendererWrapper
    {

        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string varName = "StatusCode";

            //string ExtractFrom = Inner.Render(logEvent);
            string ExtractFrom = logEvent.Message;

            if (ExtractFrom.Contains(varName))
            {
                string value = extractValue(varName, ExtractFrom);
                builder.Append(value);
            }

        }

    }

    [LayoutRenderer("StatusNote")]
    public class StatusNoteLayoutRendererWrapper : MCLLayoutRendererWrapper
    {

        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string varName = "StatusNote";

            //string ExtractFrom = Inner.Render(logEvent);
            string ExtractFrom = logEvent.Message;

            if (ExtractFrom.Contains(varName))
            {
                string value = extractValue(varName, ExtractFrom);
                builder.Append(value);
            }

        }

    }

    [LayoutRenderer("VendorOrderIdentifier")]
    public class VendorOrderIdentifierLayoutRendererWrapper : MCLLayoutRendererWrapper
    {

        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string varName = "VendorOrderIdentifier";

            //string ExtractFrom = Inner.Render(logEvent);
            string ExtractFrom = logEvent.Message;

            if (ExtractFrom.Contains(varName))
            {
                string value = extractValue(varName, ExtractFrom);
                builder.Append(value);
            }

        }

    }

   



}
