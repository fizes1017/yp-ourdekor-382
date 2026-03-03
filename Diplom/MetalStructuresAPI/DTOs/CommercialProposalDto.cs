namespace MetalStructuresAPI.DTOs;

public class CreateCommercialProposalDto
{
    public int CalculationId { get; set; }
    public string CustomerCompany { get; set; } = string.Empty;
    public string CustomerPerson { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerAddress { get; set; }
    public string? Comments { get; set; }
}
