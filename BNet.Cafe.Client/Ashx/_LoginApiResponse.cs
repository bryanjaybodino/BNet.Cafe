namespace BNet.Cafe.Client.Ashx
{
    public class LoginApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public UserData Data { get; set; }
    }
}