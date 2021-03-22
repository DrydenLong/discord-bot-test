using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Models
{
    public class User
    {
        public int DiscriminatorValue => UserId;
        public int UserId { get; set; }
        public bool SuperAdmin { get; set; }
        public bool Active { get; set; }
    }
}
