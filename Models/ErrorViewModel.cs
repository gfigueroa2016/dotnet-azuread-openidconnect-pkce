using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureAD.DotNet.Models
{
    public class ErrorViewModel
    {
        public Exception Exception { get; set; }
        public string CustomErrorMessage { get; set; }

        public ErrorViewModel()
        {
            this.Exception = Exception;
            this.CustomErrorMessage = CustomErrorMessage;    
        }

        public ErrorViewModel(HandleErrorInfo errorInfo)
        {
            this.Exception = errorInfo.Exception;
        }
    }
}