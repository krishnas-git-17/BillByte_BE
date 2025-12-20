using Billbyte_BE.Data;
using Billbyte_BE.Models;
using Billbyte_BE.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Billbyte_BE.Controllers
{
    [ApiController]
    [Route("api/table-state")]
    public class TableStateController : ControllerBase
    {
        private readonly ITableStateRepository _repo;

        public TableStateController(ITableStateRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpPost("start/{tableId}")]
        public async Task<IActionResult> Start(string tableId)
        {
            await _repo.StartTimerAsync(tableId);
            return Ok();
        }

        [HttpPost("stop/{tableId}")]
        public async Task<IActionResult> Stop(string tableId)
        {
            await _repo.StopTimerAsync(tableId);
            return Ok();
        }

        [HttpPost("status")]
        public async Task<IActionResult> SetStatus([FromBody] TableState payload)
        {
            await _repo.SetStatusAsync(payload.TableId, payload.Status);
            return Ok();
        }
    }


}
