using System;
using System.Linq;
using FundamentalsAggregator.DerivedValues;
using FundamentalsAggregator.Scrapers;

namespace FundamentalsAggregator
{
    public class Aggregator : IAggregator
    {
        readonly ScraperRunner[] scrapers;
        readonly IDerivedValue[] derivedValues;

        public Aggregator()
        {
            scrapers = new IScraper[]
                           {
                               new FtDotComFinancials(),
                               new FtDotComSummary(),
                               new BloombergBusinessweekRatios(),
                               new YahooFinance(),
                               new MorningstarKeyRatios(),
                               //new GoogleFinanceSummary(),
                               new MorningstarCurrentValuation(),
                               new MorningstarForwardValuation()
                           }.Select(s => new ScraperRunner(s))
                           .ToArray();

            derivedValues = new IDerivedValue[]
                                {
                                    new NeffTest()
                                };
        }

        public AggregationResults Aggregate(TickerSymbol symbol)
        {
            if (symbol == null) throw new ArgumentNullException("symbol");

            var providerResults = scrapers
                .AsParallel()
                .Select(s => s.GetFundamentals(symbol))
                .OrderBy(r => r.ProviderName)
                .ToList();

            // Find the stock's long name e.g. J.P. Morgan Chase & Co.
            var longName = providerResults
                .Where(r => r.ProviderName == new YahooFinance().ProviderName)
                .SelectMany(r => r.Fundamentals)
                .Where(p => p.Name == "Name")
                .Select(p => p.Value)
                .FirstOrDefault();

            var derivedValues = this.derivedValues
                .Select(d => d.Calculate(providerResults))
                .ToList();

            return new AggregationResults(symbol, providerResults, DateTime.UtcNow, longName, derivedValues);
        }
    }
}