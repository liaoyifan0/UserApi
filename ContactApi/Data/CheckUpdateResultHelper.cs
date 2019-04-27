using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactApi.Data
{
    public static class CheckUpdateResultHelper
    {

        public static bool CheckUpdateOneSuccessfully(UpdateResult result)
        {
            return result.MatchedCount == result.ModifiedCount && result.ModifiedCount == 1;
        }

        public static bool CheckUpdateSuccessfully(UpdateResult result)
        {
            return result.MatchedCount == result.ModifiedCount;
        }
    }
}
