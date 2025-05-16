namespace CDTApi.DTOs
{
    public class TransferRequestDTO
    {
        public int UserId { get; set; } = int.MinValue;
        public int BrokerageAccountId { get; set; } = int.MinValue;
        public int DAFAccountId { get; set; } = int.MinValue;
        public decimal Amount { get; set; } = decimal.MinValue;
        public string ReferenceNote { get; set; } = string.Empty;
    }


}
