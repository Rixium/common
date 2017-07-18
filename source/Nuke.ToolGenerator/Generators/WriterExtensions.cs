// Copyright Matthias Koch 2017.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Linq;
using JetBrains.Annotations;
using Nuke.ToolGenerator.Model;
using Nuke.ToolGenerator.Writers;

namespace Nuke.ToolGenerator.Generators
{
    public static class WriterExtensions
    {
        public static T WriteSummary<T> (this T writerWrapper, Task task)
            where T : IWriterWrapper
        {
            return writerWrapper.WriteSummary(task.Help ?? task.Tool.Help, task.Tool.OfficialUrl);
        }

        public static T WriteSummary<T> (this T writerWrapper, Property property)
            where T : IWriterWrapper
        {
            return writerWrapper.WriteSummary(property.Help);
        }

        public static T WriteSummary<T> (this T writerWrapper, DataClass dataClass)
            where T : IWriterWrapper
        {
            return writerWrapper.WriteSummary(dataClass.Tool.Help);
        }

        public static T WriteSummary<T> (this T writerWrapper, Enumeration enumeration)
            where T : IWriterWrapper
        {
            return writerWrapper.WriteSummary(enumeration.Tool.Help);
        }

        public static T WriteSummaryExtension<T> (this T writerWrapper, string actionText, Property property)
            where T : IWriterWrapper
        {
            return writerWrapper.WriteSummary($"<p><em>{actionText}.</em></p>{property.Help.Paragraph()}");
        }

        public static T WriteSummary<T> (this T writerWrapper, [CanBeNull] string help, string url = null)
            where T : IWriterWrapper
        {
            if (help == null)
                return writerWrapper;

            var officialUrlParagraph = url != null
                ? $"<p>For more details, visit the <a href=\"{url}\">official website</a>.</p>"
                : string.Empty;

            writerWrapper.WriteLine($"/// <summary>{help.Paragraph()}{officialUrlParagraph}</summary>");
            return writerWrapper;
        }
    }
}
