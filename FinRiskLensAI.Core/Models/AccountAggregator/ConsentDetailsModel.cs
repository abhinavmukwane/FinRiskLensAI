namespace FinRiskLensAI.Core.Models.AccountAggregator
{
    public class ConsentDetailsById
    {
        public Header Header { get; set; }
        public Body Body { get; set; }
    }

    public class Header
    {
        public string ChannelId { get; set; }
        public string Rid { get; set; }
        public DateTime Ts { get; set; }
    }

    public class Body
    {
        public int errorCode { get; set; }
        public string errorMsg { get; set; }
        public string ConsentId { get; set; }
        public string Status { get; set; }
        public DateTime CreateTimestamp { get; set; }
        public ConsentDetail ConsentDetail { get; set; }
        public ConsentUse ConsentUse { get; set; }
    }

    public class ConsentDetail
    {
        public DateTime ConsentStart { get; set; }
        public DateTime ConsentExpiry { get; set; }
        public string ConsentMode { get; set; }
        public string FetchType { get; set; }

        public List<string> ConsentTypes { get; set; }
        public List<string> FiTypes { get; set; }

        public DataConsumer DataConsumer { get; set; }
        public DataProvider DataProvider { get; set; }

        public List<Account> Accounts { get; set; }

        public Customer Customer { get; set; }
        public Purpose Purpose { get; set; }

        public FIDataRange FIDataRange { get; set; }
        public DataLife DataLife { get; set; }
        public Frequency Frequency { get; set; }

        public List<DataFilter> DataFilter { get; set; }
    }

    public class DataConsumer
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }

    public class DataProvider
    {
        public string Id { get; set; }
        public string Type { get; set; }
    }

    public class Account
    {
        public string FiType { get; set; }
        public string FipId { get; set; }
        public string AccType { get; set; }
        public string LinkRefNumber { get; set; }
        public string MaskedAccNumber { get; set; }
    }

    public class Customer
    {
        public string Id { get; set; }
    }

    public class Purpose
    {
        public string Code { get; set; }
        public string RefUri { get; set; }
        public string Text { get; set; }
        public Category Category { get; set; }
    }

    public class Category
    {
        public string Type { get; set; }
    }

    public class FIDataRange
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class DataLife
    {
        public string Unit { get; set; }
        public int Value { get; set; }
    }

    public class Frequency
    {
        public string Unit { get; set; }
        public int Value { get; set; }
    }

    public class DataFilter
    {
        public string Type { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }

    public class ConsentUse
    {
        public string LogUri { get; set; }
        public int Count { get; set; }
        public DateTime LastUseDateTime { get; set; }
    }
}
