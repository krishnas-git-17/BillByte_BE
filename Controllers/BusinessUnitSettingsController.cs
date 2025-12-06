using Microsoft.AspNetCore.Mvc;
using Billbyte_BE.DTO;
using BillByte.Interface;

namespace BillByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessUnitSettingsController : ControllerBase
    {
        private readonly IBusinessUnitSettingRepository _repo;

        public BusinessUnitSettingsController(IBusinessUnitSettingRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var data = await _repo.GetSettingsAsync();
            if (data == null)
                return Ok(new { Message = "No settings found" });

            return Ok(data);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings(UpdateBusinessUnitSettingDto dto)
        {
            var updated = await _repo.UpdateSettingsAsync(dto);
            return Ok(updated);
        }
    }
}
