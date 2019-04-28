using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.ViewModel
{
    public class TagContactInputModel
    {

        public int ContactId { get; set; }

        public List<string> Tags { get; set; }
    }
}
