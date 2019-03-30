using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.Identity.Dtos
{
    public class PollyOptions
    {
        public int RetryCount { get; set; }

        public int ExceptionCountBeforeBreaking { get; set; }
    }
}
