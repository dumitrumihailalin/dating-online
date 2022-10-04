using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.DTO
{
    public class LikedDto
    {
        public string username { get; set; }
        public string likedByUsername { get; set; }
    }
}
