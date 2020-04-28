using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPIDB.Models
{
    public class UserMovie
    {
        public int ID { get; set; }
        public virtual User User { get; set; }
        public virtual Movie Movie { get; set; }
        public int UserID { get; set; }
        public string IMDBID { get; set; }
    }
}
