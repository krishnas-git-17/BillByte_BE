using Billbyte_BE.DTO;
using BillByte.Model;
using Billbyte_BE.Data;
using Microsoft.EntityFrameworkCore;

namespace Billbyte_BE.Repositories
{
    public class BusinessUnitSettingRepository : IBusinessUnitSettingRepository
    {
        private readonly AppDbContext _db;

        public BusinessUnitSettingRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<BusinessUnitSetting?> GetSettingsAsync()
        {
            return await _db.BusinessUnitSettings.FirstOrDefaultAsync();
        }

        public async Task<BusinessUnitSetting> UpdateSettingsAsync(UpdateBusinessUnitSettingDto dto)
        {
            var settings = await _db.BusinessUnitSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new BusinessUnitSetting();
                _db.BusinessUnitSettings.Add(settings);
            }

            settings.IsTableServeNeeded = dto.IsTableServeNeeded;
            settings.NonAcTables = dto.NonAcTables;
            settings.AcTables = dto.AcTables;
            settings.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return settings;
        }
    }
}
