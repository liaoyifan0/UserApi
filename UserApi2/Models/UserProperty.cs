using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserApi2.Models
{
    public class UserProperty
    {
        public int UserId { get; set; }

        [StringLength(100)]
        public string Key { get; set; }

        public string Text { get; set; }


        [StringLength(100)]
        public string Value { get; set; }
    }
}   
