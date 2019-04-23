using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.Dtos
{
    public class UserIdentity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        public string Name { get; set; }

    }
}
