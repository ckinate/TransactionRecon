using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Domain.Common
{
    public class ResultValue<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }

        // Private constructor to enforce factory methods
        private ResultValue(bool success, T? data, string? message)
        {
            Success = success;
            Data = data;
            Message = message;
        }
        // Factory method for successful result
        public static ResultValue<T> Ok(T data, string message) => new ResultValue<T>(true, data, message);

        // Factory method for failed result
        public static ResultValue<T> Fail(string errorMessage) => new ResultValue<T>(false, default, errorMessage);
    }
}
