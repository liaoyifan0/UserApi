﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.Models
{
    public class Contact
    {

        public int UserId { get; set; }

        public string Name { get; set; }

        public string Company { get; set; }

        public string Title { get; set; }

        public string Avatar { get; set; }

        public List<string> Tags { get; set; } = new List<string>();
    }
}
