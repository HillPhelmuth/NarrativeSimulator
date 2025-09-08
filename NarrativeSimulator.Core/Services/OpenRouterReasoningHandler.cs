﻿using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace NarrativeSimulator.Core.Services;

internal class OpenRouterReasoningHandler(HttpMessageHandler innerHandler, ILoggerFactory output) : DelegatingHandler(innerHandler)
{
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new() { WriteIndented = true };

    private readonly ILogger<OpenRouterReasoningHandler> _output = output.CreateLogger<OpenRouterReasoningHandler>();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestUrl = request.RequestUri?.ToString();
        _output.LogInformation($"Request Url: {requestUrl ?? string.Empty}");
        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsStringAsync(cancellationToken);
            //_output.LogInformation("===ORIGINAL REQUEST ===\n\n" + content);
            try
            {
                //string formattedContent = JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(content), s_jsonSerializerOptions);


                // Also parse and modify the content to remove specified properties
                var root = JsonNode.Parse(content);
                if (root is not null)
                {
                    // Remove the specified properties if they exist
                    root.AsObject().Remove("top_p");
                    root.AsObject().Remove("presence_penalty");
                    root.AsObject().Remove("frequency_penalty");
                    root.AsObject().Remove("parallel_tool_calls");
                    // Add provider object to the root with the specified order array
                    var model = root["model"]?.GetValue<string>() ?? "unknown";
                    _output.LogInformation($"Model: {model}");
                    JsonArray providers;
                    if (model.Contains("gpt-oss-120b", StringComparison.OrdinalIgnoreCase))
                    {
                        providers =
                        [
                            "Cerebras",
                            "groq"
                        ];
                    }
                    else
                    {
                        providers =
                        [
                            "groq"
                        ];
                    }
                    root["provider"] = new JsonObject
                    {
                        ["only"] = providers
                    };
                    _output.LogInformation("Providers: " + string.Join(", ", providers.Select(p => p.GetValue<string>())));
                    // Remove "strict" property from response_format.json_schema if present
                    if (root["response_format"] is JsonObject responseFormat && responseFormat["json_schema"] is JsonObject jsonSchema && providers.Contains("Cerebras"))
                    {
                        jsonSchema.Remove("strict");
                    }

                    // Serialize the modified content back into the request
                    var modifiedContent = root.ToJsonString();
                    request.Content = new StringContent(modifiedContent, Encoding.UTF8, "application/json");
                    //_output.LogInformation("=== REQUEST ===\n\n" + modifiedContent);
                }
            }
            catch (JsonException)
            {
                _output.LogInformation(content);
            }
            catch
            {
                // If parsing fails for property removal, proceed without modification
            }
            _output.LogInformation(string.Empty);
        }

        // Check if streaming is requested in the content
        bool isStreamingRequested = false;
        if (request.Content is not null)
        {
            var requestContent = await request.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                var root = JsonNode.Parse(requestContent);
                isStreamingRequested = root?["stream"]?.GetValue<bool>() ?? false;
            }
            catch
            {
                // If parsing fails, assume non-streaming
                isStreamingRequested = false;
            }
        }
        // Check if the original request root contains a "tools" property
        bool requestHasTools = false;
        if (request.Content is not null)
        {
            var requestContent = await request.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                var requestRoot = JsonNode.Parse(requestContent);
                requestHasTools = requestRoot?["tools"] != null;
            }
            catch { /* ignore parse errors */ }
        }
        // Send the original request.
        var response = await base.SendAsync(request, cancellationToken);

        // Handle streaming responses (SSE)
        if (isStreamingRequested &&
            response.IsSuccessStatusCode &&
            response.Content.Headers.ContentType?.MediaType == "text/event-stream")
        {
            //return await HandleStreamingResponse(response, cancellationToken);
            return response; // Temporarily disable streaming handling
        }

        // Only process JSON content and successful responses.
        if (response is not
            { IsSuccessStatusCode: true, Content.Headers.ContentType.MediaType: "application/json" }) return response;

        var originalJson = await response.Content.ReadAsStringAsync(cancellationToken);
        //_output.LogInformation("=== RESPONSE ===\n" + originalJson);
        if (string.IsNullOrWhiteSpace(originalJson)) return response;



        // Only modify the response if the request does NOT have a "tools" property
        // Temporarily disable this behavior 
        if (false)
        {
            try
            {
                var root = JsonNode.Parse(originalJson);
                if (root is null) return response;

                var choices = root["choices"]?.AsArray();
                if (choices is null || choices.Count == 0) return response;

                var firstChoice = choices[0];
                var message = firstChoice?["message"];
                if (message is null) return response;

                var content = message["content"]?.GetValue<string>();
                var reasoning = message["reasoning_content"]?.GetValue<string>();

                // If both content and reasoning_content exist, append reasoning to content.
                if (!string.IsNullOrWhiteSpace(content) && !string.IsNullOrWhiteSpace(reasoning))
                {
                    message["content"] = $"<think>{reasoning}</think>\n\n{content}";

                    // Optionally remove the reasoning_content field entirely:
                    // message.AsObject().Remove("reasoning_content");

                    var modifiedJson = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
                    response.Content = new StringContent(modifiedJson, Encoding.UTF8, "application/json");
                }
            }
            catch
            {
                // If parsing fails, just return the original response.
                return response;
            }
        }

        return response;
    }

    private async Task<HttpResponseMessage> HandleStreamingResponse(HttpResponseMessage originalResponse, CancellationToken cancellationToken)
    {
        // Get the original stream
        var originalStream = await originalResponse.Content.ReadAsStreamAsync(cancellationToken);

        // Create a new memory stream to hold the modified content
        var modifiedStream = new MemoryStream();
        var streamWriter = new StreamWriter(modifiedStream, leaveOpen: true);

        // Read the original stream line by line
        using var reader = new StreamReader(originalStream);

        // Variables to accumulate content across chunks
        string accumulatedContent = string.Empty;
        string accumulatedReasoning = string.Empty;

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                await streamWriter.WriteLineAsync(line);
                continue;
            }

            if (!line.StartsWith("data:"))
            {
                await streamWriter.WriteLineAsync(line);
                continue;
            }

            // Handle SSE data lines
            string jsonData = line.Substring(5).Trim();
            if (string.IsNullOrWhiteSpace(jsonData) || jsonData == "[DONE]")
            {
                await streamWriter.WriteLineAsync(line);
                continue;
            }

            _output.LogInformation("=== SSE CHUNK ===");
            _output.LogInformation(jsonData);

            try
            {
                var node = JsonNode.Parse(jsonData);
                if (node is null)
                {
                    await streamWriter.WriteLineAsync(line);
                    continue;
                }

                var choices = node["choices"]?.AsArray();
                if (choices is null || choices.Count == 0)
                {
                    await streamWriter.WriteLineAsync(line);
                    continue;
                }

                var firstChoice = choices[0];
                var delta = firstChoice?["delta"];

                // For the final chunk with finish_reason
                if (delta is null || delta.AsObject().Count == 0)
                {
                    var finishReason = firstChoice?["finish_reason"]?.GetValue<string>();
                    if (finishReason == "stop" && !string.IsNullOrWhiteSpace(accumulatedContent) &&
                        !string.IsNullOrWhiteSpace(accumulatedReasoning))
                    {
                        // Create a final delta with the accumulated content and reasoning
                        var modifiedContent = $"{accumulatedContent}\n\n<think>{accumulatedReasoning}</think>";
                        var finalDelta = new JsonObject
                        {
                            ["content"] = modifiedContent
                        };

                        // Replace the empty delta with our modified version
                        firstChoice["delta"] = finalDelta;

                        // Write the modified JSON
                        var modifiedJson = node.ToJsonString();
                        await streamWriter.WriteLineAsync($"data: {modifiedJson}");
                        continue;
                    }

                    await streamWriter.WriteLineAsync(line);
                    continue;
                }

                // Handle content updates
                if (delta["content"] is JsonNode contentNode)
                {
                    accumulatedContent += contentNode.GetValue<string>();
                }

                // Handle reasoning_content updates
                if (delta["reasoning_content"] is JsonNode reasoningNode)
                {
                    accumulatedReasoning += reasoningNode.GetValue<string>();

                    // If we have reasoning content, modify the delta to include it in the content field
                    // This allows streaming UI to show the reasoning as it comes in
                    if (!string.IsNullOrEmpty(accumulatedReasoning))
                    {
                        // Create a modified delta without the reasoning_content field
                        var modifiedDelta = new JsonObject();
                        foreach (var property in delta.AsObject())
                        {
                            if (property.Key != "reasoning_content")
                            {
                                modifiedDelta[property.Key] = property.Value;
                            }
                        }

                        // We leave the content as is for now, and will add the reasoning in the final chunk
                        firstChoice["delta"] = modifiedDelta;

                        var modifiedJson = node.ToJsonString();
                        await streamWriter.WriteLineAsync($"data: {modifiedJson}");
                        continue;
                    }
                }

                // Write the unmodified line if we didn't make any changes
                await streamWriter.WriteLineAsync(line);
            }
            catch
            {
                // If parsing fails, pass through the original line
                await streamWriter.WriteLineAsync(line);
            }
        }

        // Flush the writer, reset the position to the beginning of the stream
        await streamWriter.FlushAsync(cancellationToken);
        modifiedStream.Position = 0;

        // Create a new response with the modified stream
        var modifiedResponse = new HttpResponseMessage(originalResponse.StatusCode)
        {
            Content = new StreamContent(modifiedStream)
        };

        // Copy the original headers
        foreach (var header in originalResponse.Content.Headers)
        {
            modifiedResponse.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return modifiedResponse;
    }

    public static HttpClient CreateOpenRouterResilientClient(ILoggerFactory loggerFactory)
    {
        var delegatingHandler = new OpenRouterReasoningHandler(new HttpClientHandler(), loggerFactory);
        // 1. Retry policy: retry up to 3 times with exponential backoff when a transient error occurs
        //    or when a 429 (Too Many Requests) response is received.
        var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError() // handles HttpRequestException, HTTP 5xx and 408 responses
            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))
            );

        // 2. Per-attempt timeout: Each individual HTTP call is cancelled if it runs for more than 5 minutes.
        var attemptTimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMinutes(5));

        // 3. Circuit breaker: If 5 consecutive calls fail, break for 20 minutes.
        var circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(20)
            );

        // 4. Total request timeout: The overall HTTP call (including any retries) is cancelled after 30 minutes.
        var totalTimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMinutes(30));

        // 5. Wrap the policies.
        //    The order here is important. In this case, the total timeout is outermost, then the retry policy,
        //    then the circuit breaker, and finally the per-attempt timeout is innermost.
        var policyWrap = Policy.WrapAsync(
            totalTimeoutPolicy,
            retryPolicy,
            circuitBreakerPolicy,
            attemptTimeoutPolicy
        );

        // 6. Create a handler that applies these policies.
        var policyHandler = new PolicyHttpMessageHandler(policyWrap)
        {
            InnerHandler = delegatingHandler
        };

        // 7. Finally, create and return the HttpClient instance.
        var client = new HttpClient(policyHandler);
        return client;
    }
}