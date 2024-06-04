using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "572ed4511e042c8150692ee9"; // Buraya kendi API anahtarınızı ekleyin

        public CurrencyController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

       

        [HttpGet("api/getMultipleExchangeRates")]
        [Authorize] // Sadece bu metod için JWT kimlik doğrulaması gerektirir
        public async Task<IActionResult> GetMultipleExchangeRates([FromQuery] string baseCurrency, [FromQuery] string targetCurrencies)
        {
            string url = $"https://v6.exchangerate-api.com/v6/{_apiKey}/latest/{baseCurrency}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var exchangeRates = JsonConvert.DeserializeObject<ExchangeRateResponse>(data);

                var rates = new Dictionary<string, decimal>();
                foreach (var currency in targetCurrencies.Split(','))
                {
                    if (exchangeRates.conversion_rates.ContainsKey(currency))
                    {
                        rates[currency] = exchangeRates.conversion_rates[currency];
                    }
                    else
                    {
                        return NotFound($"Exchange rate for {currency} not found.");
                    }
                }
                return Ok(new { baseCurrency, rates });
            }
            return StatusCode((int)response.StatusCode, response.ReasonPhrase);
        }
    }

    public class ExchangeRateResponse
    {
        public string base_code { get; set; }
        public Dictionary<string, decimal> conversion_rates { get; set; }
    }
}
