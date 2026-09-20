namespace BNet.Cafe.Client.Ashx
{
    public class PricingRateItem
    {
        public int Id { get; set; }
        public string CustomerType { get; set; }
        public int Minutes { get; set; }
        public double Price { get; set; }
        public bool IsDeleted { get; set; }
    }
}