namespace CDTApi.DTOs
{
    public class DonationRequestDTO
    {
        public int UserId { get; set; } = int.MinValue;
        public int CharityId { get; set; } = int.MinValue;
        public decimal Amount { get; set; } = decimal.MinValue;
        public string Email {get; set;} = string.Empty;
        public string CharityName { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }

}
