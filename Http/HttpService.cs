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

        public HttpService()
        {
            _httpClient = new HttpClient();
        }

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public void SetBaseAddress(string baseUri)
        {
            _httpClient.BaseAddress = new Uri(baseUri);
        }

        public HttpResponseMessage Get(string path)
        {
            var response = RunTask(Task.Run(async () => await _httpClient.GetAsync(CleanUpPath(path))));
            if (response == null) _logger.Error($"Failed to GET {path}");
            return response;
        }

        public T GetForObject<T>(string path)
        {
            var response = RunTask(Task.Run(async () => await _httpClient.GetFromJsonAsync<T>(CleanUpPath(path))));
            if (response == null) _logger.Error($"Failed to GET object from {path}");
            return response;
        }


        public byte[] Download(string path)
        {
            var response = RunTask(Task.Run(async () => await _httpClient.GetByteArrayAsync(CleanUpPath(path))));
            if (response == null) _logger.Error($"Failed to download from {path}");
            return response;
        }

        public HttpResponseMessage Post(string path, HttpContent content)
        {
            var response = RunTask(Task.Run(async () => await _httpClient.PostAsync(CleanUpPath(path), content)));
            if (response == null) _logger.Error($"Failed to POST to {path}");
            return response;
        }

        public T1 PostForObject<T1, T2>(string path, T2 content)
        {
            _logger.Info($"Posting to {path} content: {content}");
            var response = RunTask(Task.Run(async () => await _httpClient.PostAsJsonAsync(CleanUpPath(path), content)));
            _logger.Info(response);
            if (response != null) return RunTask(Task.Run(async () => await response.Content.ReadFromJsonAsync<T1>()));
            _logger.Error($"Failed to POST object to {path}");
            return default;
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

        private static T RunTask<T>(Task<T> task)
        {
            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;
                _logger.Error($"Failed to make request. Exception: {message}");
                return default;
            }
            return task.IsCompleted ? task.Result : default;
        }
    }
}