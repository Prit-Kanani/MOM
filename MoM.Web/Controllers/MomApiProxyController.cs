using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace MoM.Web.Controllers
{
    [ApiController]
    [Route("mom-api")]
    public class MomApiProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MomApiProxyController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("meetings")]
        public Task<IActionResult> GetMeetings()
        {
            return ForwardAsync("api/meetings", HttpMethod.Get);
        }

        [HttpGet("meetings/stats")]
        public Task<IActionResult> GetMeetingStats()
        {
            return ForwardAsync("api/meetings/stats", HttpMethod.Get);
        }

        [HttpGet("meetings/{id:int}")]
        public Task<IActionResult> GetMeeting(int id)
        {
            return ForwardAsync($"api/meetings/{id}", HttpMethod.Get);
        }

        [HttpPost("meetings")]
        public Task<IActionResult> CreateMeeting([FromBody] object payload)
        {
            return ForwardAsync("api/meetings", HttpMethod.Post, payload);
        }

        [HttpPut("meetings/{id:int}")]
        public Task<IActionResult> UpdateMeeting(int id, [FromBody] object payload)
        {
            return ForwardAsync($"api/meetings/{id}", HttpMethod.Put, payload);
        }

        [HttpDelete("meetings/{id:int}")]
        public Task<IActionResult> DeleteMeeting(int id)
        {
            return ForwardAsync($"api/meetings/{id}", HttpMethod.Delete);
        }

        [HttpGet("export/{id:int}")]
        public async Task<IActionResult> ExportMeeting(int id)
        {
            var client = _httpClientFactory.CreateClient("MomApi");
            using var response = await client.GetAsync($"api/export/{id}", HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            var mediaType = response.Content.Headers.ContentType?.MediaType ?? "application/pdf";
            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar ??
                response.Content.Headers.ContentDisposition?.FileName ??
                $"MoM_{id}.pdf";

            return File(bytes, mediaType, fileName.Trim('"'));
        }

        private async Task<IActionResult> ForwardAsync(string path, HttpMethod method, object? payload = null)
        {
            var client = _httpClientFactory.CreateClient("MomApi");
            using var request = new HttpRequestMessage(method, path);

            if (payload is not null)
            {
                request.Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    MediaTypeNames.Application.Json);
            }

            using var response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return NoContent();
            }

            var content = await response.Content.ReadAsStringAsync();
            var mediaType = response.Content.Headers.ContentType?.MediaType ?? "application/json";

            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                ContentType = mediaType
            };
        }

        private static class MediaTypeNames
        {
            public static class Application
            {
                public const string Json = "application/json";
            }
        }
    }
}
