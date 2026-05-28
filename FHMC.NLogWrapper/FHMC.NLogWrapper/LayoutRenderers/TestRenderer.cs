using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLog.LayoutRenderers.Wrappers
{
    using NLog.Config;

    /// <summary>
    /// Left part of a text
    /// </summary>
    [LayoutRenderer("left-custom")]
    [AppDomainFixedOutput]
    [ThreadAgnostic]
    [ThreadSafe]
    public sealed class LeftCustomLayoutRendererWrapper : WrapperLayoutRendererBase
    {
        /// <summary>
        /// Gets or sets the length in characters. 
        /// </summary>
        /// <docgen category="Transformation Options" order="10"/>
        [RequiredParameter]
        public int Length { get; set; }

        /// <inheritdoc/>
        protected override void RenderInnerAndTransform(LogEventInfo logEvent, StringBuilder builder, int orgLength)
        {
            if (Length <= 0)
            {
                return;
            }

            builder.Append(Inner.Render(logEvent));
            
            var renderedLength = builder.Length - orgLength;
            if (renderedLength > Length)
            {
                builder.Length = orgLength + Length;
            }

           
        }

        /// <inheritdoc/>
        protected override string Transform(string text)
        {
            throw new NotSupportedException();
        }
    }
}
