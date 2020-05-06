using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPIDB.Models
{
    public class Favorite
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string IMDBID { get; set; }
    }
}
