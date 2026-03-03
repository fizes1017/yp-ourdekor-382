using System.IO;
using MetalStructuresAPI.Data;
using MetalStructuresAPI.DTOs;
using MetalStructuresAPI.Models;
using MetalStructuresAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MetalStructuresAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommercialProposalsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly CommercialProposalPdfService _pdfService;

    public CommercialProposalsController(ApplicationDbContext context, CommercialProposalPdfService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAndDownloadPdf([FromBody] CreateCommercialProposalDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (string.IsNullOrWhiteSpace(dto.CustomerCompany) || string.IsNullOrWhiteSpace(dto.CustomerPerson) ||
                string.IsNullOrWhiteSpace(dto.CustomerPhone) || string.IsNullOrWhiteSpace(dto.CustomerEmail))
            {
                return BadRequest(new { message = "Заполните обязательные поля: компания, контактное лицо, телефон, email" });
            }

            var calculation = await _context.Calculations
                .Include(c => c.CalculationItems)
                    .ThenInclude(ci => ci.Material)
                .FirstOrDefaultAsync(c => c.Id == dto.CalculationId);

            if (calculation == null)
                return NotFound(new { message = "Расчет не найден" });

            if (calculation.ManagerId != userId)
                return Forbid();

            var companyInfo = await _context.CompanyInfo.FirstOrDefaultAsync();
            if (companyInfo == null)
                return BadRequest(new { message = "Реквизиты компании не настроены. Обратитесь к администратору. Добавьте запись в таблицу company_info (можно выполнить скрипт Migrations/SeedCompanyInfo.sql)." });

            var proposalNumber = $"КП-{DateTime.Now:yyyyMMdd}-{await _context.CommercialProposals.CountAsync() + 1}";

            var proposal = new CommercialProposal
            {
                CalculationId = dto.CalculationId,
                ManagerId = userId,
                CustomerCompany = dto.CustomerCompany.Trim(),
                CustomerPerson = dto.CustomerPerson.Trim(),
                CustomerPhone = dto.CustomerPhone.Trim(),
                CustomerEmail = dto.CustomerEmail.Trim(),
                CustomerAddress = string.IsNullOrWhiteSpace(dto.CustomerAddress) ? null : dto.CustomerAddress.Trim(),
                ProposalNumber = proposalNumber,
                CreatedAt = DateTime.UtcNow,
                Comments = string.IsNullOrWhiteSpace(dto.Comments) ? null : dto.Comments.Trim()
            };

            _context.CommercialProposals.Add(proposal);
            await _context.SaveChangesAsync();

            var items = calculation.CalculationItems.ToList();
            var pdfBytes = _pdfService.GeneratePdf(companyInfo, proposal, calculation, items);

            var safeCompanyName = string.Join("_", proposal.CustomerCompany.Replace("\"", "").Replace("/", "-").Replace("\\", "-").Split(Path.GetInvalidFileNameChars()));
            var fileName = $"КП_{proposalNumber}_{safeCompanyName}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException != null ? " " + ex.InnerException.Message : "";
            var msg = "Ошибка при формировании КП: " + ex.Message + inner;
            Console.WriteLine("[КП] Exception: " + ex.ToString());
            return StatusCode(500, new { message = msg });
        }
    }
}
