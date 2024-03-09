using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using Newtonsoft.Json;

namespace MyLifeInSeconds.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeController : ControllerBase
    {
        [HttpGet("calculate-time/{dob}/{location}")]
        public async Task<IActionResult> CalculateTimeSinceBirth(string dob, string location)
        {
            try
            {
                // Parse date of birth
                DateTime birthDate = DateTime.ParseExact(dob, "ddMMyyyy", null);

                // Get current time using a separate thread
                Task<long> getTimeTask = Task.Run(() => GetCurrentTimeInSeconds(location));

                // Calculate time since birth
                long currentTimeInSeconds = await getTimeTask;
                long timeSinceBirth = currentTimeInSeconds - (long)(DateTime.UtcNow - birthDate).TotalSeconds;

                return Ok(new { TimeSinceBirth = timeSinceBirth });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        private async Task<long> GetCurrentTimeInSeconds(string location)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = $"http://worldtimeapi.org/api/timezone/{location}";

                try
                {
                    // Make the API call asynchronously
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read and parse the response
                        string jsonResult = await response.Content.ReadAsStringAsync();
                        WorldTimeApiResponse result = JsonConvert.DeserializeObject<WorldTimeApiResponse>(jsonResult);

                        // Extract the Unix timestamp
                        return result.unixtime;
                    }
                    else
                    {
                        // Handle the error case (you may want to log or throw an exception)
                        return 0;
                    }
                }
                catch (HttpRequestException)
                {
                    // Handle HTTP request exception
                    return 0;
                }
            }
        }
    }
}
