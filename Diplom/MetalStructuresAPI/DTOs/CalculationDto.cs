namespace MetalStructuresAPI.DTOs;

public class CalculationDto
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public DateOnly CalculatedAt { get; set; }
    public List<CalculationItemDto> Items { get; set; } = new();
}

public class CalculationItemDto
{
    public int Id { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialArticle { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Unit { get; set; } = string.Empty;
}

public class CreateCalculationDto
{
    public List<CreateCalculationItemDto> Items { get; set; } = new();
}

public class CreateCalculationItemDto
{
    public int MaterialId { get; set; }
    public decimal Quantity { get; set; }
}


