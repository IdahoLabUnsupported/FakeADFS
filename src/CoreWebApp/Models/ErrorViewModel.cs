// Copyright 2018 Battelle Energy Alliance, LLC
// ALL RIGHTS RESERVED
using System;

namespace CoreWebApp.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}