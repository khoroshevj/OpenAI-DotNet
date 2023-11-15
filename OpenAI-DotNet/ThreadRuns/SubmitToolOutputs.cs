using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.ThreadRuns
{
    public sealed class SubmitToolOutputs
    {
        /// <summary>
        /// A list of the relevant tool calls.
        /// </summary>
        [JsonPropertyName("tool_calls")]
        public IReadOnlyList<ToolCall> ToolCalls { get; set; }
    }
}