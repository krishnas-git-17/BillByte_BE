using BillByte.Interface;
using Billbyte_BE.Models;
using Billbyte_BE.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Billbyte_BE.Controllers
{
    [ApiController]
    [Route("api/table-preferences")]
    public class TablePreferencesController : ControllerBase
    {
        private readonly ITablePreferenceRepository _repository;

        public TablePreferencesController(ITablePreferenceRepository repository)
        {
            _repository = repository;
        }

        // GET
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _repository.GetAllAsync();
            return Ok(data);
        }

        // POST (bulk create)
        [HttpPost]
        public async Task<IActionResult> Post(List<TablePreference> request)
        {
            await _repository.AddRangeAsync(request);
            return Ok(new { message = "Table preferences created" });
        }

        // PUT
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, TablePreference request)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = request.Name;
            existing.TableCount = request.TableCount;

            await _repository.UpdateAsync(existing);
            return Ok(new { message = "Updated successfully" });
        }

        // DELETE by id
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return Ok(new { message = "Deleted successfully" });
        }

        // DELETE all
        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            await _repository.DeleteAllAsync();
            return Ok(new { message = "All table preferences deleted" });
        }

        //[HttpGet("generate-hash")]
        //public IActionResult GenerateHash()
        //{
        //    var hash = BCrypt.Net.BCrypt.HashPassword("Krishna@123");
        //    return Ok(hash);
        //}
    }
}
