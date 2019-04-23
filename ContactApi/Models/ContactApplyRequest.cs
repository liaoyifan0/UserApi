using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.Models
{
    public class ContactApplyRequest
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Company { get; set; }

        public string Title { get; set; }

        public string Avatar { get; set; }

        public int ApplierId { get; set; }

        /// <summary>
        /// 是否通过，0未通过，1已通过
        /// </summary>
        public string Approvaled { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime HandledTime { get; set; }
    }
}
