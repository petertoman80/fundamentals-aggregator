﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FundamentalsAggregator.TickerSymbolFormatters;
using log4net;

namespace FundamentalsAggregator.Scrapers
{
    public class YahooFinance : IScraper
    {
        static readonly ILog Log = LogManager.GetLogger(typeof (YahooFinance));

        static readonly ITickerSymbolFormatter Formatter =
            new YahooFinanceFormatter();

        const string CsvUrlFormat = "http://finance.yahoo.com/d/quotes.csv?s={0}&f={1}";
        const string UrlFormat = "http://uk.finance.yahoo.com/q?s={0}";

        // From http://www.gummy-stuff.org/Yahoo-data.htm
        static readonly IDictionary<string, string> ApiParameters =
            new Dictionary<string, string>
                {
                    {"b4", "Book Value"},
                    {"d", "Dividend/Share"},
                    {"e", "Earnings/Share"}, // EPS?
                    {"e7", "EPS Estimate Current Year"},
                    {"e8", "EPS Estimate Next Year"},
                    {"e9", "EPS Estimate Next Quarter"},
                    {"g1", "Holdings Gain Percent"},
                    {"g3", "Annualized Gain"},
                    {"g4", "Holdings Gain"},
                    {"j4", "EBITDA"},
                    {"p5", "Price/Sales"},
                    {"p6", "Price/Book"},
                    {"q", "Ex-Dividend Date"},
                    {"r", "P/E Ratio"},
                    {"r1", "Dividend Pay Date"},
                    {"r5", "PEG Ratio"},
                    {"r6", "Price/EPS Estimate Current Year"},
                    {"r7", "Price/EPS Estimate Next Year"},
                    {"s7", "Short Ratio"},
                    {"y", "Dividend Yield"},
                };

        public string ProviderName
        {
            get { return "Yahoo Finance"; }
        }

        public ScraperResults GetFundamentals(TickerSymbol symbol)
        {
            if (symbol == null) throw new ArgumentNullException("symbol");

            var formattedSymbol = Formatter.Format(symbol);

            IDictionary<string, string> fundamentals;
            try
            {
                fundamentals = GetFundamentals(formattedSymbol);
            }
            catch (Exception e)
            {
                throw new ScraperException(symbol, this, e);
            }

            var friendlyUrl = new Uri(String.Format(UrlFormat, formattedSymbol));

            if (!fundamentals.Any())
                throw new NoFundamentalsAvailableException();

            return new ScraperResults(friendlyUrl, fundamentals);
        }

        static Dictionary<string, string> GetFundamentals(string formattedSymbol)
        {
            var csvUrl = new Uri(String.Format(CsvUrlFormat, formattedSymbol, String.Join("", ApiParameters.Keys)));

            Log.DebugFormat("Looking up {0} from {1}", formattedSymbol, csvUrl);

            string csvLine;
            using (var webClient = new WebClient())
                csvLine = webClient.DownloadString(csvUrl);

            Log.DebugFormat("Got data ({0} chars)", csvLine.Length);

            var values = csvLine.Split(',').Select(s => s.Trim('"').Trim()).ToList();

            var fundamentals = new Dictionary<string, string>();

            for (var i = 0; i < values.Count; i++)
            {
                var name = ApiParameters.ElementAt(i).Value;
                var value = values[i].Trim();

                if (IsNull(value))
                {
                    Log.DebugFormat("Missing: {0} = {1}", name, value);
                    continue;
                }

                Log.DebugFormat("Found: {0} = {1}", name, value);
                fundamentals.Add(name, value);
            }
            return fundamentals;
        }

        public static bool IsNull(string value)
        {
            return String.IsNullOrWhiteSpace(value) ||
                   value == "N/A" ||
                   value == "-" ||
                   value == "- - -";
        }
    }

}