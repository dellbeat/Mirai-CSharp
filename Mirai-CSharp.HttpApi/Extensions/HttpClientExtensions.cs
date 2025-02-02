using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using System.Net.Http.Json;
using System.Text;
#endif

#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)
namespace Mirai.CSharp.HttpApi.Extensions
{
    /// <summary>
    /// <see cref="HttpClient"/> 的扩展方法
    /// </summary>
    public static partial class HttpClientExtensions
    {
        private static Version DefaultHttpVersion { get; } = new(2, 0);

        private static readonly MediaTypeHeaderValue DefaultJsonMediaType = new("application/json") { CharSet = "utf-8" };

        /// <summary>
        /// 异步发起一个 Http 请求
        /// </summary>
        /// <param name="client">要进行请求的 <see cref="HttpClient"/></param>
        /// <param name="method">请求方式</param>
        /// <param name="uri">请求目标</param>
        /// <param name="content">请求正文</param>
        /// <param name="token">用于取消请求的 <see cref="CancellationToken"/></param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="HttpRequestException"/>
        /// <returns>表示此异步操作的 <see cref="Task"/></returns>
        public static async Task<HttpResponseMessage> SendAsync(this HttpClient client, HttpMethod method, Uri uri, HttpContent? content, CancellationToken token = default)
        {
            using HttpRequestMessage request = new HttpRequestMessage(method, uri);
            request.Content = content;
            request.Version = DefaultHttpVersion;
            return await client.SendAsync(request, token);
        }

#if NET5_0_OR_GREATER
        /// <summary>
        /// 将服务器响应正文异步序列化为 <see cref="byte"/>[]
        /// </summary>
        /// <param name="responseTask">要处理的一个异步请求任务</param>
        /// <param name="token">用于取消此操作的一个 <see cref="CancellationToken"/></param>
        /// <returns>表示此异步操作的 <see cref="Task"/></returns>
        public static async Task<byte[]> GetBytesAsync(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            return await response.Content.ReadAsByteArrayAsync(token);
        }

        /// <summary>
        /// 将服务器响应正文异步序列化为 <see cref="string"/>
        /// </summary>
        /// <param name="responseTask">要处理的一个异步请求任务</param>
        /// <param name="token">用于取消此操作的一个 <see cref="CancellationToken"/></param>
        /// <returns>表示此异步操作的 <see cref="Task"/></returns>
        public static async Task<string> GetStringAsync(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync(token);
        }
#else
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0060 // Remove unused parameter
        /// <summary>
        /// 将服务器响应正文异步序列化为 <see cref="byte"/>[]
        /// </summary>
        /// <param name="responseTask">要处理的一个异步请求任务</param>
        /// <param name="token">本参数将被忽略, 因为 <see cref="HttpContent.ReadAsByteArrayAsync()"/> 方法没有一个用于接收 <see cref="CancellationToken"/> 的重载</param>
        /// <returns>表示此异步操作的 <see cref="Task"/></returns>
        public static async Task<byte[]> GetBytesAsync(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// 将服务器响应正文异步序列化为 <see cref="string"/>
        /// </summary>
        /// <param name="responseTask">要处理的一个异步请求任务</param>
        /// <param name="token">本参数将被忽略, 因为 <see cref="HttpContent.ReadAsByteArrayAsync()"/> 方法没有一个用于接收 <see cref="CancellationToken"/> 的重载</param>
        /// <returns>表示此异步操作的 <see cref="Task"/></returns>
        public static async Task<string> GetStringAsync(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            return await response.Content.ReadAsStringAsync();
        }
#pragma warning restore IDE0060
#pragma warning restore IDE0079
#endif

#if NET5_0_OR_GREATER
        private static Encoding? GetEncoding(string? charset)
        {
            Encoding? encoding = null;
            if (charset != null)
            {
                try
                {
                    // Remove at most a single set of quotes.
                    if (charset.Length > 2 && charset[0] == '\"' && charset[^1] == '\"')
                    {
                        encoding = Encoding.GetEncoding(charset[1..^1]);
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(charset);
                    }
                }
                catch (ArgumentException e)
                {
                    throw new InvalidOperationException("The character set provided in ContentType is invalid.", e);
                }
            }
            return encoding;
        }

        /// <inheritdoc cref="GetObjectAsync{T}(Task{HttpResponseMessage}, JsonSerializerOptions?, CancellationToken)"/>
        public static Task<T?> GetObjectAsync<T>(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
            => responseTask.GetObjectAsync<T?>(null, token);

        /// <summary>
        /// 将服务器响应正文异步序列化为 <typeparamref name="T"/> 的一个实例
        /// </summary>
        /// <param name="responseTask">要处理的一个异步请求任务</param>
        /// <param name="options">反序列化时要使用的 <see cref="JsonSerializerOptions"/></param>
        /// <param name="token">用于取消反序列化的 <see cref="CancellationToken"/></param>
        /// <returns>表示此异步操作的 <see cref="Task"/></returns>
        public static async Task<T?> GetObjectAsync<T>(this Task<HttpResponseMessage> responseTask, JsonSerializerOptions? options, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync<T?>(options, token);
        }

        /// <inheritdoc cref="GetObjectAsync(Task{HttpResponseMessage}, Type, JsonSerializerOptions?, CancellationToken)"/>
        public static Task<object?> GetObjectAsync(this Task<HttpResponseMessage> responseTask, Type returnType, CancellationToken token = default)
            => responseTask.GetObjectAsync(returnType, null, token);

        /// <summary>
        /// 将服务器响应正文异步序列化为 <paramref name="returnType"/> 表示的一个实例
        /// </summary>
        /// <param name="returnType">用于转换和返回的 <see cref="Type"/></param>
        /// <inheritdoc cref="GetObjectAsync{T}(Task{HttpResponseMessage}, JsonSerializerOptions?, CancellationToken)"/>
        public static async Task<object?> GetObjectAsync(this Task<HttpResponseMessage> responseTask, Type returnType, JsonSerializerOptions? options, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            return await response.Content.ReadFromJsonAsync(returnType, options, token);
        }

        /// <inheritdoc cref="GetJsonAsync(Task{HttpResponseMessage}, JsonDocumentOptions, CancellationToken)"/>
        public static Task<JsonDocument> GetJsonAsync(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
        {
            return responseTask.GetJsonAsync(default, token);
        }

        /// <summary>
        /// 将服务器响应正文异步反序列化为一个 <see cref="JsonDocument"/> 实例
        /// </summary>
        /// <param name="responseTask">要处理的一个异步请求任务</param>
        /// <param name="options">反序列化时要使用的 <see cref="JsonDocumentOptions"/></param>
        /// <param name="token">用于取消反序列化的 <see cref="CancellationToken"/></param>
        /// <returns>表示此异步操作的 <see cref="Task"/></returns>
        public static async Task<JsonDocument> GetJsonAsync(this Task<HttpResponseMessage> responseTask, JsonDocumentOptions options, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            Stream stream = response.Content.ReadAsStream(token); // Since Content.ReadAsStreamAsync is returned synchronously.
            Encoding? encoding = GetEncoding(response.Content.Headers.ContentType?.CharSet);
            if (encoding != null && encoding != Encoding.UTF8)
            {
                stream = Encoding.CreateTranscodingStream(stream, encoding, Encoding.UTF8);
            }
            using (stream)
            {
                return await JsonDocument.ParseAsync(stream, options, token);
            }
        }
#else
        /// <inheritdoc cref="GetObjectAsync{T}(Task{HttpResponseMessage}, JsonSerializerOptions?, CancellationToken)"/>
        public static Task<T?> GetObjectAsync<T>(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
            => responseTask.GetObjectAsync<T?>(null, token);

        /// <summary>
        /// 将服务器响应正文异步序列化为 <typeparamref name="T"/> 的一个实例
        /// </summary>
        /// <param name="responseTask">要处理的一个异步请求任务</param>
        /// <param name="options">反序列化时要使用的 <see cref="JsonSerializerOptions"/></param>
        /// <param name="token">用于取消反序列化的 <see cref="CancellationToken"/></param>
        /// <remarks>
        /// 请确保服务器响应的 Json 是以 UTF-8 编码的
        /// </remarks>
        /// <returns>表示此异步操作的 <see cref="Task"/></returns>
        public static async Task<T?> GetObjectAsync<T>(this Task<HttpResponseMessage> responseTask, JsonSerializerOptions? options, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            using Stream stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T?>(stream, options, token);
        }

        /// <inheritdoc cref="GetObjectAsync(Task{HttpResponseMessage}, Type, JsonSerializerOptions?, CancellationToken)"/>
        public static Task<object?> GetObjectAsync(this Task<HttpResponseMessage> responseTask, Type returnType, CancellationToken token = default)
            => responseTask.GetObjectAsync(returnType, null, token);

        /// <summary>
        /// 将服务器响应正文异步序列化为 <paramref name="returnType"/> 表示的一个实例
        /// </summary>
        /// <param name="returnType">用于转换和返回的 <see cref="Type"/></param>
        public static async Task<object?> GetObjectAsync(this Task<HttpResponseMessage> responseTask, Type returnType, JsonSerializerOptions? options, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            using var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync(stream, returnType, options, token);
        }

        /// <inheritdoc cref="GetJsonAsync(Task{HttpResponseMessage}, JsonDocumentOptions, CancellationToken)"/>
        public static Task<JsonDocument> GetJsonAsync(this Task<HttpResponseMessage> responseTask, CancellationToken token = default)
        {
            return responseTask.GetJsonAsync(default, token);
        }

        /// <summary>
        /// 将服务器响应正文异步反序列化为一个 <see cref="JsonDocument"/> 实例
        /// </summary>
        /// <param name="responseTask">要处理的一个异步请求任务</param>
        /// <param name="options">反序列化时要使用的 <see cref="JsonDocumentOptions"/></param>
        /// <param name="token">用于取消反序列化的 <see cref="CancellationToken"/></param>
        /// <remarks>
        /// 请确保服务器响应的 Json 是以 UTF-8 编码的
        /// </remarks>
        /// <returns>表示此异步操作的 <see cref="Task"/></returns>
        public static async Task<JsonDocument> GetJsonAsync(this Task<HttpResponseMessage> responseTask, JsonDocumentOptions options, CancellationToken token = default)
        {
            using HttpResponseMessage response = await responseTask.ConfigureAwait(false);
            using Stream stream = await response.Content.ReadAsStreamAsync();
            return await JsonDocument.ParseAsync(stream, options, token);
        }
#endif
        /// <summary>
        /// Sets Content-Type in response to application/json.
        /// </summary>
        /// <param name="responseTask">An asynchronous operation that represents the HTTP response.</param>
        public static Task<HttpResponseMessage> ForceJson(this Task<HttpResponseMessage> responseTask)
        {
            return responseTask.ContinueWith(p =>
            {
#if NETSTANDARD2_0
                if (p.IsCompleted && !p.IsFaulted && !p.IsCanceled) // treats response as json
#else
                if (p.IsCompletedSuccessfully)
#endif
                {
                    HttpContentHeaders headers = p.Result.Content.Headers;
                    if (headers.ContentType == null)
                    {
                        headers.ContentType = DefaultJsonMediaType;
                    }
                    else if (headers.ContentType.MediaType != "application/json")
                    {
                        headers.ContentType.MediaType = "application/json";
                    }
                }
                return p;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        public static Task<HttpResponseMessage> EnsureSuccessStatusCode(this Task<HttpResponseMessage> responseTask)
        {
            return responseTask.ContinueWith(p =>
            {
#if NETSTANDARD2_0
                if (p.IsCompleted && !p.IsFaulted && !p.IsCanceled) // treats response as json
#else
                if (p.IsCompletedSuccessfully)
#endif
                {
                    p.Result.EnsureSuccessStatusCode();
                }
                return p;
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        public static void SetUserAgent(this HttpClient client, string userAgent)
        {
            client.DefaultRequestHeaders.SetUserAgent(userAgent);
        }

        public static void SetAccept(this HttpClient client, string accept)
        {
            client.DefaultRequestHeaders.SetAccept(accept);
        }

        public static void SetAcceptLanguage(this HttpClient client, string acceptLanguage)
        {
            client.DefaultRequestHeaders.SetAcceptLanguage(acceptLanguage);
        }

        public static void SetSecPolicy(this HttpClient client, string? mode = "cors", string? site = "same-site", string? dest = "empty")
        {
            client.DefaultRequestHeaders.SetSecPolicy(mode, site, dest);
        }
    }
}
