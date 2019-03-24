using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserApi2.Models
{
    public class UserTag
    {
        public int UserId { get; set; }

        [StringLength(100)]
        public string Tag { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
