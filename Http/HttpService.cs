using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NLog;

namespace ASRR.Revit.Core.Http
{
    public class HttpService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly HttpClient _httpClient;
        private string _baseAddress;

        public HttpService()
        {
            _httpClient = new HttpClient();
        }

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public void SetBaseAddress(string baseAddress, bool initClient = false)
        {
            if (baseAddress == null) throw new ArgumentNullException(nameof(baseAddress));
            if (_httpClient.BaseAddress != null)
                throw new InvalidOperationException($"Cannot set base address. HttpClient was initialized with base address: '{_httpClient.BaseAddress}'");
            if (initClient) _httpClient.BaseAddress = new Uri(baseAddress);
            else _baseAddress = baseAddress;
        }

        public void ResetApiKey(string apiKey)
        {
            if (_httpClient.DefaultRequestHeaders.Contains("X-API-Key"))
                _httpClient.DefaultRequestHeaders.Remove("X-API-Key");
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
        }

        public HttpResponseMessage Get(string path)
        {
            var url = GetCleanUrl(path);
            var response = RunTask(Task.Run(async () => await _httpClient.GetAsync(url)), path);
            if (response == null) _logger.Error($"Failed to GET {url} — response was null (check earlier log entries for the underlying exception)");
            return response;
        }

        public T GetForObject<T>(string path)
        {
            var url = GetCleanUrl(path);
            _logger.Debug($"GET object from {url} (type: {typeof(T).Name})");
            var response = RunTask(Task.Run(async () => await _httpClient.GetFromJsonAsync<T>(url)), path);
            if (response == null) _logger.Error($"Failed to GET object from {url} — response was null (check earlier log entries for the underlying exception)");
            return response;
        }


        public byte[] Download(string path)
        {
            var url = GetCleanUrl(path);
            var response = RunTask(Task.Run(async () => await _httpClient.GetByteArrayAsync(url)), path);
            if (response == null) _logger.Error($"Failed to download from {url} — response was null (check earlier log entries for the underlying exception)");
            return response;
        }

        public HttpResponseMessage Post(string path, HttpContent content)
        {
            var url = GetCleanUrl(path);
            var response = RunTask(Task.Run(async () => await _httpClient.PostAsync(url, content)), path);
            if (response == null) _logger.Error($"Failed to POST to {url} — response was null (check earlier log entries for the underlying exception)");
            return response;
        }

        public T1 PostForObject<T1, T2>(string path, T2 content)
        {
            var url = GetCleanUrl(path);
            _logger.Info($"Posting to {url} content: {content}");
            var response = RunTask(Task.Run(async () => await _httpClient.PostAsJsonAsync(url, content)), path);
            _logger.Info(response);
            if (response != null) return RunTask(Task.Run(async () => await response.Content.ReadFromJsonAsync<T1>()), path);
            _logger.Error($"Failed to POST object to {url} — response was null (check earlier log entries for the underlying exception)");
            return default;
        }

        private string GetCleanUrl(string path)
        {
            return _httpClient.BaseAddress == null ? CombineUris(_baseAddress, path) : CleanUpPath(path);
        }

        public static string CombineUris(params string[] uris)
        {
            if (uris == null)
                throw new ArgumentNullException(nameof(uris));

            var urisList = uris.ToList();

            var result = "";

            for (var i = 0; i < urisList.Count; i++)
                if (urisList[i] != null)
                {
                    var trimmedUri = urisList[i];
                    trimmedUri = trimmedUri.TrimStart('/', '\\');
                    trimmedUri = trimmedUri.TrimEnd('/', '\\');
                    var slash = i == 0 ? "" : "/";
                    result += $"{slash}{trimmedUri}";
                }

            return result;
        }

        private static string CleanUpPath(string path)
        {
            return path == null ? throw new ArgumentNullException(nameof(path)) : CombineUris(path);
        }

        private static T RunTask<T>(Task<T> task, string path = null)
        {
            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                var innermost = ex;
                while (innermost.InnerException != null) innermost = innermost.InnerException;

                _logger.Error(ex.InnerException ?? ex,
                    $"HTTP request failed{(path != null ? $" for '{path}'" : "")}. " +
                    $"Root cause: [{innermost.GetType().Name}] {innermost.Message}");
                return default;
            }
            return task.IsCompleted ? task.Result : default;
        }
    }
}