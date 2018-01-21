using System;
using System.Collections.Generic;
using VkNet.Model;

namespace SPA.Models
{
    public class Person
    {
        public long Id { get; set; }
        public int Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Religion { get; set; }

        public List<GroupCounter> GroupReposts { get; set; }
        public IEnumerable<Post> Posts { get; set; }

        public Person()
        {
            GroupReposts = new List<GroupCounter>();
            Posts = new List<Post>();
        }
    }
}
