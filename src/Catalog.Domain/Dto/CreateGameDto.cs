namespace Catalog.Domain.Dto
{
    public record CreateGameDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal PromotionalPrice { get; set; }
        public bool Published { get; set; }
        public bool Active { get; set; }
    }
}
