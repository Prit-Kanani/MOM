using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoM.Api.Models;
using MoM.Api.Services;

namespace MoM.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController(MomContext context, PdfService pdfService) : ControllerBase
    {
        private readonly MomContext _context = context;
        private readonly PdfService _pdfService = pdfService;

        [HttpGet("{id}")]
        public async Task<IActionResult> ExportToPdf(int id)
        {
            var meeting = await _context.Meetings
                .Include(m => m.VenueMappings)
                    .ThenInclude(mv => mv.Venue)
                .Include(m => m.UserMappings)
                    .ThenInclude(mu => mu.User)
                .Include(m => m.Agendas)
                    .ThenInclude(a => a.OwnerUser)
                .Include(m => m.ActionItems)
                    .ThenInclude(a => a.ResponsibilityUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meeting == null) return NotFound();

            try
            {
                var pdf = _pdfService.GenerateMomPdf(meeting);
                return File(pdf, "application/pdf", $"MoM_{meeting.Date:yyyyMMdd}_{meeting.Id}.pdf");
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "PDF generation failed",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
