using System;
namespace SharpiesMafia.Models
{
    public class User
    {
        public long id { get; set; }
        public string name { get; set; }
        public string connection_id { get; set; }
        public long game_id { get; set; }
        public string role { get; set; }
        public bool is_dead { get; set; }
        public int vote_count { get; set; }
    }
}
