using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Models.Responses
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string TransactionId { get; set; }
        public float? ErrorCode { get; set; }
        public List<string> Errors { get; set; }
        public T Result { get; set; }
        public ApiResponse()
        {
            Errors = new List<string>();
        }
    }
}
