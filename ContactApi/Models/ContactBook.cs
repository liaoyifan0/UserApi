using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.Models
{
    public class ContactBook
    {

        public int UserId { get; set; }


        public IList<Contact> Contacts { get; set; }
    }
}
