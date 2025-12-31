namespace Catalog.Domain.Dto
{
    public class CreateOrderDto
    {
        public string UserId { get; set; } = string.Empty;
		public string EmailUser { get; set; } = string.Empty;
		public string GameId { get; set; } = string.Empty;
    }
}
