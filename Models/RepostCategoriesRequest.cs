using Newtonsoft.Json;

namespace SPA.Models
{
    public class RepostCategoriesRequest
    {
        [JsonProperty("user")]
        public string UserName { get; set; }
        [JsonProperty("period")]
        public int AnalysisPeriod { get; set; }
    }
}
