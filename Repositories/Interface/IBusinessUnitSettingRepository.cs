using BillByte.Model;

public interface IBusinessUnitSettingRepository
{
    Task<BusinessUnitSetting?> GetSettingsAsync();
    Task<BusinessUnitSetting> UpdateSettingsAsync(UpdateBusinessUnitSettingDto dto);
}
