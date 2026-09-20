namespace BNet.Cafe.Client.Ashx
{
    public class UserData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string DateCreated { get; set; }
        public string TimeCreated { get; set; }
        public bool IsDeleted { get; set; }
        public int TotalDuration { get; set; }
        public string FormattedTotalDuration { get; set; }
        public string Password { get; set; }
    }
}