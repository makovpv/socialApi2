using System;

namespace SPA.Models
{
    public class VK_Like
    {
        public Int64 UserId { get; set; }
        public Int64 ObjectId { get; set; }
        public Int64? OwnerId { get; set; }
    }
}
