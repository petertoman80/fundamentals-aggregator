﻿using System;
using System.Collections.Generic;

namespace FundamentalsAggregator
{
    public class ProviderResults
    {
        public ProviderResults(string providerName, Uri url, IDictionary<string, string> fundamentals)
        {
            if (providerName == null) throw new ArgumentNullException("providerName");
            if (url == null) throw new ArgumentNullException("url");
            if (fundamentals == null) throw new ArgumentNullException("fundamentals");
            ProviderName = providerName;
            Url = url;
            Fundamentals = fundamentals;
        }

        public ProviderResults(string providerName, Exception error)
        {
            if (providerName == null) throw new ArgumentNullException("providerName");
            ProviderName = providerName;
            Error = error;
        }

        public string ProviderName { get; private set; }
        public Uri Url { get; private set; }
        public IDictionary<string, string> Fundamentals { get; private set; }
        public Exception Error { get; private set; }
        public bool IsError { get { return Error != null; } }
    }
}