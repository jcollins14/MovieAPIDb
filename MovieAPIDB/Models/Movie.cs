using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MovieAPIDB.Models
{
    public class Movie
    {
        public int ID { get; set; }
        [JsonProperty("Title")]
        public string Title { get; set; }
        [JsonProperty("Year")]
        public int YearReleased { get; set; }
        [JsonProperty("Genre")]
        public string Genres { get; set; }
        [JsonProperty("Plot")]
        public string Plot { get; set; }
        [JsonProperty("Poster")]
        public string PosterLink { get; set; }
        [JsonProperty("imdbID")]
        public string IMDBID { get; set; }
    }
}
