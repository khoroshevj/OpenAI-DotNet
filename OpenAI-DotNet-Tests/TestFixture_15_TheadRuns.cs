using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenAI.Assistants;
using OpenAI.Chat;
using OpenAI.Tests.Weather;
using OpenAI.Threads;
using Message = OpenAI.Threads.Message;

namespace OpenAI.Tests
{
    internal class TestFixture_15_TheadRuns : AbstractTestFixture
    {
        private static CreateThreadRequest TestThread { get; } = new CreateThreadRequest(
            new List<Message>
            {
                new Message("Test message")
            },
            new Dictionary<string, string>
            {
                ["text"] = "test"
            }
        );

        private static CreateAssistantRequest TestAssistant { get; } = new CreateAssistantRequest("gpt-3.5-turbo-1106");

        [Test]
        public async Task Test_01_CreateThreadRun()
        {
            Assert.NotNull(OpenAIClient.ThreadsEndpoint);

            var assistant = await OpenAIClient.AssistantsEndpoint.CreateAssistantAsync(TestAssistant);
            var thread = await OpenAIClient.ThreadsEndpoint.CreateThreadAsync(TestThread);

            var request = new CreateThreadRunRequest(assistant.Id)
            {
                Instructions = "Run test instructions",
                Model = "gpt-3.5-turbo",
                Metadata = new Dictionary<string, string>
                {
                    ["key"] = "value"
                }
            };

            var run = await OpenAIClient.ThreadsEndpoint.CreateThreadRunAsync(thread.Id, request);

            Assert.IsNotNull(run);
            Assert.AreEqual("gpt-3.5-turbo", run.Model);
            Assert.AreEqual("Run test instructions", run.Instructions);

            Assert.IsNotNull(run.Metadata);
            Assert.Contains("key", run.Metadata.Keys.ToList());
            Assert.AreEqual("value", run.Metadata["key"]);
        }

        [Test]
        public async Task Test_02_CreateThreadAndRun()
        {
            var assistant = await OpenAIClient.AssistantsEndpoint.CreateAssistantAsync(TestAssistant);

            var request = new CreateThreadAndRunRequest(assistant.Id)
            {
                Thread = new CreateThreadAndRunRequest.ThreadForRun
                {
                    Messages = new List<Message>
                    {
                        new("Thread message text")
                    }
                },
                Instructions = "Run test instructions",
                Model = "gpt-3.5-turbo",
                Metadata = new Dictionary<string, string>
                {
                    ["key"] = "value"
                }
            };

            var run = await OpenAIClient.ThreadsEndpoint.CreateThreadAndRunAsync(request);

            Assert.IsNotNull(run);
            Assert.AreEqual("gpt-3.5-turbo", run.Model);
            Assert.AreEqual("Run test instructions", run.Instructions);

            Assert.IsNotNull(run.Metadata);
            Assert.Contains("key", run.Metadata.Keys.ToList());
            Assert.AreEqual("value", run.Metadata["key"]);

            Assert.IsNotNull(run.ThreadId);
        }

        [Test]
        public async Task Test_03_ListThreadRuns()
        {
            var assistant = await OpenAIClient.AssistantsEndpoint.CreateAssistantAsync(TestAssistant);
            var thread = await OpenAIClient.ThreadsEndpoint.CreateThreadAsync(TestThread);

            var request = new CreateThreadRunRequest(assistant.Id);
            var run = await OpenAIClient.ThreadsEndpoint.CreateThreadRunAsync(thread.Id, request);

            var list = await OpenAIClient.ThreadsEndpoint.ListThreadRunsAsync(thread.Id);

            Assert.IsNotNull(list);
            Assert.IsNotEmpty(list.Data);

            foreach (var threadRun in list.Data)
            {
                var retrieved =
                    await OpenAIClient.ThreadsEndpoint.RetrieveRunAsync(threadRun.ThreadId, threadRun.Id);

                Assert.IsNotNull(retrieved);

                Console.WriteLine($"[{retrieved.ThreadId}] -> {retrieved.Id}");
            }
        }

        [Test]
        public async Task Test_04_ModifyThreadRun()
        {
            var assistant = await OpenAIClient.AssistantsEndpoint.CreateAssistantAsync(TestAssistant);
            var thread = await OpenAIClient.ThreadsEndpoint.CreateThreadAsync(TestThread);

            var request = new CreateThreadRunRequest(assistant.Id);
            var run = await OpenAIClient.ThreadsEndpoint.CreateThreadRunAsync(thread.Id, request);

            // run in Queued and InProgress can't be modified
            var loopCounter = 0;
            while (run.Status == RunStatus.InProgress || run.Status == RunStatus.Queued)
            {
                await Task.Delay(2_000);
                loopCounter++;
                run = await OpenAIClient.ThreadsEndpoint.RetrieveRunAsync(thread.Id, run.Id);

                if (loopCounter == 10)
                {
                    Assert.Fail("Spent too much in long in InProgress/Queued status");
                }
            }

            var modified = await OpenAIClient.ThreadsEndpoint.ModifyThreadRunAsync(
                thread.Id,
                run.Id,
                new Dictionary<string, string>
                {
                    ["key"] = "value"
                });

            Assert.IsNotNull(modified);
            Assert.AreEqual(run.Id, modified.Id);
            Assert.IsNotNull(modified.Metadata);
            Assert.Contains("key", modified.Metadata.Keys.ToList());
            Assert.AreEqual("value", modified.Metadata["key"]);
        }

        [Test]
        public async Task Test_05_CancelRun()
        {
            var assistant = await OpenAIClient.AssistantsEndpoint.CreateAssistantAsync(TestAssistant);
            var thread = await OpenAIClient.ThreadsEndpoint.CreateThreadAsync(TestThread);

            var request = new CreateThreadRunRequest(assistant.Id);
            var run = await OpenAIClient.ThreadsEndpoint.CreateThreadRunAsync(thread.Id, request);

            run = await OpenAIClient.ThreadsEndpoint.CancelThreadRunAsync(thread.Id, run.Id);

            var loopCounter = 0;
            while (run.Status == RunStatus.Cancelling)
            {
                await Task.Delay(2_000);
                loopCounter++;
                run = await OpenAIClient.ThreadsEndpoint.RetrieveRunAsync(thread.Id, run.Id);

                if (loopCounter == 10)
                {
                    Assert.Fail("Spent too much in Cancelling status");
                }
            }

            Assert.AreEqual(RunStatus.Cancelled, run.Status);
        }

        [Test]
        public async Task Test_06_SubmitToolOutput()
        {
            var assistant = await OpenAIClient.AssistantsEndpoint.CreateAssistantAsync(TestAssistant);
            var createThreadRequest = new CreateThreadRequest(
                new List<Message>
                {
                    new("I'm in Kuala-Lumpur, please tell me what's the temperature in celsius now?")
                });

            var thread = await OpenAIClient.ThreadsEndpoint.CreateThreadAsync(createThreadRequest);

            var function =
                new Function(
                    nameof(WeatherService.GetCurrentWeather),
                    "Get the current weather in a given location",
                    new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["location"] = new JsonObject
                            {
                                ["type"] = "string",
                                ["description"] = "The city and state, e.g. San Francisco, CA"
                            },
                            ["unit"] = new JsonObject
                            {
                                ["type"] = "string",
                                ["enum"] = new JsonArray { "celsius", "fahrenheit" }
                            }
                        },
                        ["required"] = new JsonArray { "location", "unit" }
                    });

            var functionTool = new AssistantTool(function);
            var request = new CreateThreadRunRequest(assistant.Id)
            {
                Tools = new [] { functionTool }
            };

            var run = await OpenAIClient.ThreadsEndpoint.CreateThreadRunAsync(thread.Id, request);

            // waiting while run in Queued and InProgress
            var loopCounter = 0;
            while (run.Status == RunStatus.InProgress || run.Status == RunStatus.Queued)
            {
                await Task.Delay(2_000);
                loopCounter++;
                run = await OpenAIClient.ThreadsEndpoint.RetrieveRunAsync(thread.Id, run.Id);

                if (loopCounter == 10)
                {
                    Assert.Fail("Spent too much in long in InProgress/Queued status");
                }
            }
            
            Assert.AreEqual(RunStatus.RequiresAction, run.Status);
            Assert.IsNotNull(run.RequiredAction);
            Assert.AreEqual("submit_tool_outputs", run.RequiredAction.Type);
            Assert.IsNotNull(run.RequiredAction.SubmitToolOutputs);
            Assert.IsNotEmpty(run.RequiredAction.SubmitToolOutputs.ToolCalls);
            Assert.AreEqual(1, run.RequiredAction.SubmitToolOutputs.ToolCalls.Count);
            var toolCall = run.RequiredAction.SubmitToolOutputs.ToolCalls[0];
            Assert.AreEqual("function", toolCall.Type);
            Assert.IsNotNull(toolCall.Function);
            Assert.AreEqual(nameof(WeatherService.GetCurrentWeather), toolCall.Function.Name);
            Assert.IsNotNull(toolCall.Function.Arguments);
            
            var functionArgs = JsonSerializer.Deserialize<WeatherArgs>(toolCall.Function.Arguments);
            var functionResult = WeatherService.GetCurrentWeather(functionArgs);

            var submitRequest = new SubmitThreadRunToolOutputsRequest
            {
                ToolOutputs = new List<ToolOutput>
                {
                    new ToolOutput(toolCall.Id, functionResult)
                }
            };

            run = await OpenAIClient.ThreadsEndpoint.SubmitToolOutputsAsync(thread.Id, run.Id, submitRequest);
            
            // waiting while run in Queued and InProgress
            loopCounter = 0;
            while (run.Status == RunStatus.InProgress || run.Status == RunStatus.Queued)
            {
                await Task.Delay(2_000);
                loopCounter++;
                run = await OpenAIClient.ThreadsEndpoint.RetrieveRunAsync(thread.Id, run.Id);

                if (loopCounter == 10)
                {
                    Assert.Fail("Spent too much in long in InProgress/Queued status");
                }
            }
            
            Assert.AreEqual(RunStatus.Completed, run.Status);
        }

        [Test]
        public async Task Test_07_ListRunSteps()
        {
            var assistant = await OpenAIClient.AssistantsEndpoint.CreateAssistantAsync(TestAssistant);
            var thread = await OpenAIClient.ThreadsEndpoint.CreateThreadAsync(TestThread);

            var request = new CreateThreadRunRequest(assistant.Id);
            var run = await OpenAIClient.ThreadsEndpoint.CreateThreadRunAsync(thread.Id, request);

            // waiting while run in Queued and InProgress
            var loopCounter = 0;
            while (run.Status == RunStatus.Queued)
            {
                await Task.Delay(2_000);
                loopCounter++;
                run = await OpenAIClient.ThreadsEndpoint.RetrieveRunAsync(thread.Id, run.Id);

                if (loopCounter == 10)
                {
                    Assert.Fail("Spent too much in long in Queued status");
                }
            }
            
            var list = await OpenAIClient.ThreadsEndpoint.ListTheadRunStepsAsync(thread.Id, run.Id);

            Assert.IsNotNull(list);
            Assert.IsNotEmpty(list.Data);
            
            foreach (var step in list.Data)
            {
                var retrieved =
                    await OpenAIClient.ThreadsEndpoint.RetrieveTheadRunStepAsync(thread.Id, run.Id, step.Id);
                
                Assert.IsNotNull(retrieved);

                Console.WriteLine($"[{retrieved.ThreadId}] -> {retrieved.Id}");
            }
        }
    }
}