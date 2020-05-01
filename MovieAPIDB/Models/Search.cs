using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MovieAPIDB.Models
{

    public class Search
    {
        [JsonProperty("Search")]
        public List<SearchResult> Results { get; set; }
        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }
    }

    public class SearchResult
    {
        [JsonProperty("Title")]
        public string Title { get; set; }
        [JsonProperty("Year")]
        public string Year { get; set; }
        [JsonProperty("imdbID")]
        public string ImdbID { get; set; }
        [JsonProperty("Type")]
        public string Type { get; set; }
        [JsonProperty("Poster")]
        public string Poster { get; set; }
    }

}
