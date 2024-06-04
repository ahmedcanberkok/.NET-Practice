namespace WebApplication1.Models
{
    public class ExchangeRateResponse
    {
        public string base_code { get; set; }
        public Dictionary<string, decimal> conversion_rates { get; set; }
    }
}
