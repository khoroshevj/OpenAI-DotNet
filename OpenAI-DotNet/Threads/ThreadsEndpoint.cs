using System;
using OpenAI.Extensions;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.Threads
{
    /// <summary>
    /// Create threads that assistants can interact with.
    /// <see href="https://platform.openai.com/docs/api-reference/threads"/>
    /// </summary>
    public class ThreadsEndpoint : BaseEndPoint
    {
        public ThreadsEndpoint(OpenAIClient api) : base(api) { }

        protected override string Root => "threads";

        /// <summary>
        /// Create a thread.
        /// </summary>
        /// <param name="request"><see cref="CreateThreadRequest"/>.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Thread"/>.</returns>
        public async Task<Thread> CreateThreadAsync(CreateThreadRequest request, CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonSerializer.Serialize(request, OpenAIClient.JsonSerializationOptions).ToJsonStringContent(EnableDebug);
            var response = await Api.Client.PostAsync(GetUrl(), jsonContent, cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return response.DeserializeResponse<Thread>(responseAsString, OpenAIClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Retrieves a thread.
        /// </summary>
        /// <param name="threadId">The id of the <see cref="Thread"/> to retrieve.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Thread"/>.</returns>
        public async Task<Thread> RetrieveThreadAsync(string threadId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync(GetUrl($"/{threadId}"), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return response.DeserializeResponse<Thread>(responseAsString, OpenAIClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Modifies a thread.
        /// </summary>
        /// <remarks>
        /// Only the <see cref="Thread.Metadata"/> can be modified.
        /// </remarks>
        /// <param name="threadId">The id of the <see cref="Thread"/> to modify.</param>
        /// <param name="metaData">Set of 16 key-value pairs that can be attached to an object.
        /// This can be useful for storing additional information about the object in a structured format.
        /// Keys can be a maximum of 64 characters long and values can be a maximum of 512 characters long.
        /// </param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="Thread"/>.</returns>
        public async Task<Thread> ModifyThreadAsync(string threadId, IReadOnlyDictionary<string, string> metaData, CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonSerializer.Serialize(new { metadata = metaData }, OpenAIClient.JsonSerializationOptions).ToJsonStringContent(EnableDebug);
            var response = await Api.Client.PostAsync(GetUrl($"/{threadId}"), jsonContent, cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return response.DeserializeResponse<Thread>(responseAsString, OpenAIClient.JsonSerializationOptions);
        }

        /// <summary>
        /// Delete a thread.
        /// </summary>
        /// <param name="threadId">The id of the <see cref="Thread"/> to delete.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>True, if was successfully deleted.</returns>
        public async Task<bool> DeleteThreadAsync(string threadId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.DeleteAsync(GetUrl($"/{threadId}"), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Deserialize<DeletedResponse>(responseAsString, OpenAIClient.JsonSerializationOptions)?.Deleted ?? false;
        }
    
        /// <summary>
        /// Create a message.
        /// </summary>
        /// <param name="threadId">The id of the thread to create a message for.</param>
        /// <param name="request"></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadMessage"/>.</returns>
        public async Task<ThreadMessage> CreateThreadMessageAsync(string threadId, CreateThreadMessageRequest request, CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonSerializer.Serialize(request, OpenAIClient.JsonSerializationOptions).ToJsonStringContent(EnableDebug);
            var response = await Api.Client.PostAsync(GetUrl($"/{threadId}/messages"), jsonContent, cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var created = JsonSerializer.Deserialize<ThreadMessage>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return created;
        }

        /// <summary>
        /// Retrieve a message.
        /// </summary>
        /// <param name="threadId">The id of the thread to which this message belongs.</param>
        /// <param name="messageId">The id of the message to retrieve.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>The message object matching the specified id.</returns>
        public async Task<ThreadMessage> RetrieveThreadMessageAsync(string threadId, string messageId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync(GetUrl($"/{threadId}/messages/{messageId}"), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var created = JsonSerializer.Deserialize<ThreadMessage>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return created;
        }

        /// <summary>
        /// Modifies a message.
        /// </summary>
        /// <remarks>
        /// Only the <see cref="ThreadMessage.Metadata"/> can be modified.
        /// </remarks>
        /// <param name="threadId">The id of the thread to which this message belongs.</param>
        /// <param name="messageId">The id of the message to modify.</param>
        /// <param name="metadata">Set of 16 key-value pairs that can be attached to an object.
        /// This can be useful for storing additional information about the object in a structured format.
        /// Keys can be a maximum of 64 characters long and values can be a maxium of 512 characters long.
        /// </param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadMessage"/>.</returns>
        public async Task<ThreadMessage> ModifyThreadMessageAsync(
            string threadId, string messageId, Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonSerializer.Serialize(new { metadata = metadata }, OpenAIClient.JsonSerializationOptions).ToJsonStringContent(EnableDebug);
            var response = await Api.Client.PostAsync(GetUrl($"/{threadId}/messages/{messageId}"), jsonContent, cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var created = JsonSerializer.Deserialize<ThreadMessage>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return created;
        }

        /// <summary>
        /// Returns a list of messages for a given thread.
        /// </summary>
        /// <param name="threadId">The id of the thread the messages belong to.</param>
        /// <param name="limit">A limit on the number of objects to be returned. Limit can range between 1 and 100, and the default is 20.</param>
        /// <param name="order">Sort order by the created_at timestamp of the objects. asc for ascending order and desc for descending order.</param>
        /// <param name="after">A cursor for use in pagination. after is an object id that defines your place in the list.
        /// For instance, if you make a list request and receive 100 objects, ending with obj_foo,
        /// your subsequent call can include after=obj_foo in order to fetch the next page of the list.
        /// </param>
        /// <param name="before">A cursor for use in pagination. before is an object id that defines your place in the list.
        /// For instance, if you make a list request and receive 100 objects, ending with obj_foo,
        /// your subsequent call can include before=obj_foo in order to fetch the previous page of the list.
        /// </param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadMessagesList"/>.</returns>
        public async Task<ThreadMessagesList> ListThreadMessagesAsync(
            string threadId, int? limit = null, string order = "desc", string after = null, string before = null,
            CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, string>();
            if (limit.HasValue) parameters.Add("limit", limit.ToString());
            if (!String.IsNullOrEmpty(order)) parameters.Add("order", order);
            if (!String.IsNullOrEmpty(after)) parameters.Add("after", after);
            if (!String.IsNullOrEmpty(before)) parameters.Add("before", before);

            var response = await Api.Client.GetAsync(GetUrl($"/{threadId}/messages", parameters), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var messages = JsonSerializer.Deserialize<ThreadMessagesList>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return messages;
        }

        /// <summary>
        /// Retrieve message file
        /// </summary>
        /// <param name="threadId">The id of the thread to which the message and File belong.</param>
        /// <param name="messageId">The id of the message the file belongs to.</param>
        /// <param name="fileId">The id of the file being retrieved.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadMessageFile"/>.</returns>
        public async Task<ThreadMessageFile> RetrieveMessageFile(
            string threadId, string messageId, string fileId,
            CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync(GetUrl($"/{threadId}/messages/{messageId}/files/{fileId}"), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var created = JsonSerializer.Deserialize<ThreadMessageFile>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return created;
        }
    
        /// <summary>
        /// Returns a list of message files.
        /// </summary>
        /// <param name="threadId">The id of the thread that the message and files belong to.</param>
        /// <param name="messageId">The id of the message that the files belongs to.</param>
        /// <param name="limit">A limit on the number of objects to be returned. Limit can range between 1 and 100, and the default is 20.</param>
        /// <param name="order">Sort order by the created_at timestamp of the objects. asc for ascending order and desc for descending order.</param>
        /// <param name="after">A cursor for use in pagination. after is an object id that defines your place in the list.
        /// For instance, if you make a list request and receive 100 objects, ending with obj_foo,
        /// your subsequent call can include after=obj_foo in order to fetch the next page of the list.
        /// </param>
        /// <param name="before">A cursor for use in pagination. before is an object id that defines your place in the list.
        /// For instance, if you make a list request and receive 100 objects, ending with obj_foo,
        /// your subsequent call can include before=obj_foo in order to fetch the previous page of the list.
        /// </param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadMessageFilesList"/>.</returns>
        public async Task<ThreadMessageFilesList> ListMessageFilesAsync(
            string threadId, string messageId, int? limit = null, string order = "desc", string after = null, string before = null,
            CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, string>();
            if (limit.HasValue) parameters.Add("limit", limit.ToString());
            if (!String.IsNullOrEmpty(order)) parameters.Add("order", order);
            if (!String.IsNullOrEmpty(after)) parameters.Add("after", after);
            if (!String.IsNullOrEmpty(before)) parameters.Add("before", before);

            var response = await Api.Client.GetAsync(GetUrl($"/{threadId}/messages/{messageId}/files", parameters), cancellationToken).ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var messages = JsonSerializer.Deserialize<ThreadMessageFilesList>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return messages;
        }

        /// <summary>
        /// Create a run.
        /// </summary>
        /// <param name="threadId">The id of the thread to run.</param>
        /// <param name="request"></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadRun"/>.</returns>
        public async Task<ThreadRun> CreateThreadRunAsync(string threadId, CreateThreadRunRequest request,
            CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonSerializer.Serialize(request, OpenAIClient.JsonSerializationOptions)
                .ToJsonStringContent(EnableDebug);
            var response = await Api.Client.PostAsync(GetUrl($"/{threadId}/runs"), jsonContent, cancellationToken)
                .ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var created = JsonSerializer.Deserialize<ThreadRun>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return created;
        }

        /// <summary>
        /// Retrieves a run.
        /// </summary>
        /// <param name="threadId">The id of the thread that was run.</param>
        /// <param name="runId">The id of the run to retrieve.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadRun"/>.</returns>
        public async Task<ThreadRun> RetrieveRunAsync(string threadId, string runId,
            CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync(GetUrl($"/{threadId}/runs/{runId}"), cancellationToken)
                .ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var run = JsonSerializer.Deserialize<ThreadRun>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return run;
        }

        /// <summary>
        /// Modifies a run.
        /// </summary>
        /// <remarks>
        /// Only the <see cref="ThreadRun.Metadata"/> can be modified.
        /// </remarks>
        /// <param name="threadId">The id of the thread that was run.</param>
        /// <param name="runId">The id of the <see cref="ThreadRun"/> to modify.</param>
        /// <param name="metadata">Set of 16 key-value pairs that can be attached to an object.
        /// This can be useful for storing additional information about the object in a structured format.
        /// Keys can be a maximum of 64 characters long and values can be a maxium of 512 characters long.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadRun"/>.</returns>
        public async Task<ThreadRun> ModifyThreadRunAsync(string threadId, string runId,
            Dictionary<string, string> metadata, CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonSerializer.Serialize(new { metadata = metadata }, OpenAIClient.JsonSerializationOptions)
                .ToJsonStringContent(EnableDebug);
            var response = await Api.Client.PostAsync(GetUrl($"/{threadId}/runs/{runId}"), jsonContent, cancellationToken)
                .ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var created = JsonSerializer.Deserialize<ThreadRun>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return created;
        }

        /// <summary>
        /// Returns a list of runs belonging to a thread.
        /// </summary>
        /// <param name="threadId">The id of the thread the run belongs to.</param>
        /// <param name="limit">A limit on the number of objects to be returned. Limit can range between 1 and 100, and the default is 20.</param>
        /// <param name="order">Sort order by the created_at timestamp of the objects. asc for ascending order and desc for descending order.</param>
        /// <param name="after">A cursor for use in pagination. after is an object id that defines your place in the list.
        /// For instance, if you make a list request and receive 100 objects, ending with obj_foo,
        /// your subsequent call can include after=obj_foo in order to fetch the next page of the list.</param>
        /// <param name="before">A cursor for use in pagination. before is an object id that defines your place in the list.
        /// For instance, if you make a list request and receive 100 objects, ending with obj_foo,
        /// your subsequent call can include before=obj_foo in order to fetch the previous page of the list.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns>A list of run objects.</returns>
        public async Task<ThreadRunsList> ListThreadRunsAsync(
            string threadId, int? limit = null, string order = "desc", string after = null, string before = null,
            CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, string>();
            if (limit.HasValue) parameters.Add("limit", limit.ToString());
            if (!String.IsNullOrEmpty(order)) parameters.Add("order", order);
            if (!String.IsNullOrEmpty(after)) parameters.Add("after", after);
            if (!String.IsNullOrEmpty(before)) parameters.Add("before", before);

            var response = await Api.Client.GetAsync(GetUrl($"/{threadId}/runs", parameters), cancellationToken)
                .ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var runs = JsonSerializer.Deserialize<ThreadRunsList>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return runs;
        }

        /// <summary>
        /// When a run has the status: "requires_action" and required_action.type is submit_tool_outputs,
        /// this endpoint can be used to submit the outputs from the tool calls once they're all completed.
        /// All outputs must be submitted in a single request.
        /// </summary>
        /// <param name="threadId">The id of the thread to which this run belongs.</param>
        /// <param name="runId">The id of the run that requires the tool output submission.</param>
        /// <param name="request"></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadRun"/>.</returns>
        public async Task<ThreadRun> SubmitToolOutputsAsync(
            string threadId,
            string runId,
            SubmitThreadRunToolOutputsRequest request,
            CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonSerializer.Serialize(request, OpenAIClient.JsonSerializationOptions)
                .ToJsonStringContent(EnableDebug);
            var response = await Api.Client.PostAsync(
                    GetUrl($"/{threadId}/runs/{runId}/submit_tool_outputs"),
                    jsonContent,
                    cancellationToken)
                .ConfigureAwait(false);

            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var run = JsonSerializer.Deserialize<ThreadRun>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return run;
        }

        /// <summary>
        /// Cancels a run that is in_progress.
        /// </summary>
        /// <param name="threadId">The id of the thread to which this run belongs.</param>
        /// <param name="runId">The id of the run to cancel.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadRun"/>.</returns>
        public async Task<ThreadRun> CancelThreadRunAsync(string threadId, string runId,
            CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.PostAsync(GetUrl($"/{threadId}/runs/{runId}/cancel"), content: null, cancellationToken)
                .ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var created = JsonSerializer.Deserialize<ThreadRun>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return created;
        }
    
        /// <summary>
        /// Create a thread and run it in one request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="ThreadRun"/>.</returns>
        public async Task<ThreadRun> CreateThreadAndRunAsync(CreateThreadAndRunRequest request, CancellationToken cancellationToken = default)
        {
            var jsonContent = JsonSerializer.Serialize(request, OpenAIClient.JsonSerializationOptions)
                .ToJsonStringContent(EnableDebug);
            var response = await Api.Client.PostAsync(GetUrl($"/runs"), jsonContent, cancellationToken)
                .ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var created = JsonSerializer.Deserialize<ThreadRun>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return created;
        }

        /// <summary>
        /// Retrieves a run step.
        /// </summary>
        /// <param name="threadId">The id of the thread to which the run and run step belongs.</param>
        /// <param name="runId">The id of the run to which the run step belongs.</param>
        /// <param name="stepId">The id of the run step to retrieve.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="RunStep"/>.</returns>
        public async Task<RunStep> RetrieveTheadRunStepAsync(string threadId, string runId, string stepId, CancellationToken cancellationToken = default)
        {
            var response = await Api.Client.GetAsync(GetUrl($"/{threadId}/runs/{runId}/steps/{stepId}"), cancellationToken)
                .ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var step = JsonSerializer.Deserialize<RunStep>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return step;
        }

        /// <summary>
        /// Retrieves a run step.
        /// </summary>
        /// <param name="threadId">The id of the thread to which the run and run step belongs.</param>
        /// <param name="runId">The id of the run to which the run step belongs.</param>
        /// <param name="limit">A limit on the number of objects to be returned. Limit can range between 1 and 100, and the default is 20.</param>
        /// <param name="order">Sort order by the created_at timestamp of the objects. asc for ascending order and desc for descending order.</param>
        /// <param name="after">A cursor for use in pagination. after is an object ID that defines your place in the list.
        /// For instance, if you make a list request and receive 100 objects, ending with obj_foo,
        /// your subsequent call can include after=obj_foo in order to fetch the next page of the list.</param>
        /// <param name="before">A cursor for use in pagination. before is an object ID that defines your place in the list.
        /// For instance, if you make a list request and receive 100 objects, ending with obj_foo,
        /// your subsequent call can include before=obj_foo in order to fetch the previous page of the list.</param>
        /// <param name="cancellationToken">Optional, <see cref="CancellationToken"/>.</param>
        /// <returns><see cref="RunStepsList"/>.</returns>
        public async Task<RunStepsList> ListTheadRunStepsAsync(
            string threadId, string runId,
            int? limit = null, string order = "desc", string after = null, string before = null,
            CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, string>();
            if (limit.HasValue) parameters.Add("limit", limit.ToString());
            if (!String.IsNullOrEmpty(order)) parameters.Add("order", order);
            if (!String.IsNullOrEmpty(after)) parameters.Add("after", after);
            if (!String.IsNullOrEmpty(before)) parameters.Add("before", before);

            var response = await Api.Client.GetAsync(GetUrl($"/{threadId}/runs/{runId}/steps", parameters), cancellationToken)
                .ConfigureAwait(false);
            var responseAsString = await response.ReadAsStringAsync(EnableDebug, cancellationToken).ConfigureAwait(false);
            var step = JsonSerializer.Deserialize<RunStepsList>(responseAsString, OpenAIClient.JsonSerializationOptions);

            return step;
        }
    }
}