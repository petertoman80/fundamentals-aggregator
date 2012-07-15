using FundamentalsAggregator.Scrapers;
using NUnit.Framework;
using SharpTestsEx;

namespace FundamentalsAggregator.Specs.Scrapers
{
    public class FtDotComFinancialsTests
    {
        [TestFixture]
        public class When_fetching_data
        {
            static ScraperResults results;

            static readonly TickerSymbol[] Symbols = new[]
                                                         {
                                                             new TickerSymbol("AAPL", Exchange.Nasdaq), 
                                                             new TickerSymbol("GOOG", Exchange.Nasdaq), 
                                                             new TickerSymbol("ENRC", Exchange.Lse),
                                                             new TickerSymbol("GXY", Exchange.Asx),
                                                             new TickerSymbol("TME", Exchange.Nzx)
                                                         };

            [Test, TestCaseSource("Symbols")]
            public void It_should_scrape_fundamentals(TickerSymbol tickerSymbol)
            {
                results = new FtDotComFinancials().GetFundamentals(tickerSymbol);
                results.Url.Should().Not.Be.Null();
                AssertHelper.AssertFundamental<float>(results, "Current ratio", Is.Not.EqualTo(0));
            }
        }
    }
}