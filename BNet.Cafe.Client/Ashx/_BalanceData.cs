namespace BNet.Cafe.Client.Ashx
{
    public class BalanceData
    {
        public string UserId { get; set; }
        public double Balance { get; set; }
    }

    public class BalanceApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public BalanceData Data { get; set; }
    }
}