using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.Dtos
{
    public class PollyOptions
    {
        public int RetryCount { get; set; }

        public int ExceptionCountBeforeBreaking { get; set; }
    }
}
