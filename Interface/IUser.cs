
using libMasterObject;

namespace SvcAccount.Interface
{
    public interface IUser
    {
        Task<List<T>> GetByIDAllInCompany<T>(int UserID, int CompanyID);
        Task<T> GetByID<T>(int UserID);
        Task<int> PostData(USR_User_Details model);
        Task<int> CreateUserAccount(USR_User_CreateAcc model);
        Task<int> PutData(USR_User_Details model);
        Task<Boolean> DeleteData(USR_User_Details model);
    }
}
