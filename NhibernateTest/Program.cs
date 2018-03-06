using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Mapping;
using NHibernate.Tool.hbm2ddl;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace NhibernateTest
{

    public class Company
    {
        public virtual string Ticker { get; set; }
        public virtual ICollection<StockQuote> Quotes { get; set; }
    }
    public class StockQuote
    {
        public virtual string Ticker { get; set; }

        public virtual int Date { get; set; }

        public virtual double Open { get; set; }
        public virtual double High { get; set; }
        public virtual double Low { get; set; }
        public virtual double Close { get; set; }
        public virtual double Volume { get; set; }
        
        public virtual bool ValueEquals(StockQuote other)
        {
            return other.Ticker == Ticker &&
                   other.Date == Date;
        }

        public virtual bool Equals(object obj)
        {
            if (!(obj is StockQuote cast)) return false;
            return this.ValueEquals(cast);
        }

        public virtual int GetHashCode()
        {
            return Date + Ticker.Select(x => int.Parse(x.ToString())).Sum();
        }

        public virtual string ToString()
        {
            return $"{Ticker} {Date}";
        }
    }
    public class CompanyNhibernateMap : ClassMap<Company>
    {
        public CompanyNhibernateMap()
        {
            Table("Companies");
            Id(x => x.Ticker);
            //Map(x => x.Ticker);
            HasMany(x => x.Quotes)
            //.Table("StockQuote")
            .KeyColumn("Ticker")
            //.ForeignKeyConstraintName("CompanyQuotesConstrint")
            //.Not.Inverse()
            //.Inverse()
            .Cascade.All();
        }
    }
    public class StockQuoteNhibernateMap : ClassMap<StockQuote>
    {
        public StockQuoteNhibernateMap()
        {
            //Schema("dbo");
            Table("StockQuotes");
            CompositeId()
                .KeyProperty(x => x.Ticker)
                .KeyProperty(x => x.Date);
            Map(x => x.Open).Column("Open");
            Map(x => x.High).Column("High");
            Map(x => x.Low).Column("Low");
            Map(x => x.Close).Column("Close");
            Map(x => x.Volume).Column("Volume");
        }
    }
    class Program
    {
        static void Main(string[] args)
        {

            const string ticker = "test2";
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var testQuote1 =
                    new StockQuote
                    {
                        Ticker = ticker,
                        Date = 20180101,
                        Open = 11,
                        High = 12,
                        Low = 10,
                        Close = 11.2,
                        Volume = 100
                    };
            var testQuote2 =
                    new StockQuote
                    {
                        Ticker = ticker,
                        Date = 20180102,
                        Open = 11,
                        High = 12,
                        Low = 10,
                        Close = 11.2,
                        Volume = 100
                    };
            var stock = new Company
            {
                Ticker = ticker,
                Quotes = new Collection<StockQuote>
                {
                    testQuote2
                }
            };

            const string connectionString = @"server=(localdb)\MSSQLLocalDB;Initial Catalog=StockMarketDb;Integrated Security=True;";

            var factory = Fluently.Configure()
                    .Database(MsSqlConfiguration.MsSql2012
                        .ConnectionString(connectionString).ShowSql())
                    .Mappings(m =>
                        m.FluentMappings
                            .Add<CompanyNhibernateMap>()
                            .Add<StockQuoteNhibernateMap>()
                            )
                            .ExposeConfiguration(c => SchemaMetadataUpdater.QuoteTableAndColumns(c))
                            .BuildSessionFactory();


            var session = factory.OpenSession();

            session.Save(stock);
            session.Flush();
        }
    }
}
