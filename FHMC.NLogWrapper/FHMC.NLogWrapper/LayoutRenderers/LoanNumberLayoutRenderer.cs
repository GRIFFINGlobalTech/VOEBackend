using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace FHMC.NLogWrapper.LayoutRendererWrappers
{
    /// <summary>
    /// Extract LoanNumber, if present
    /// </summary>
    /// <example>
    /// ${loannumber:${message}}
    /// ${loannumber:${exception:format=tostring}
    /// </example>
    [LayoutRenderer("loannumber")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    [ThreadSafe]
    public class LoanNumberLayoutRendererWrapper : NLog.LayoutRenderers.Wrappers.WrapperLayoutRendererBase
    {

        
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {

            string ExtractFrom = Inner.Render(logEvent);
            Regex regex = new Regex(@"[0-9]{10}");
            Match match = regex.Match(ExtractFrom);

            if (match != null)
            {
                builder.Append(match.Value);
            }
        }


        protected override string Transform(string text)
        {
            throw new NotSupportedException();
        }



    }

}
