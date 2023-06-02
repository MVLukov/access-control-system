namespace ACS.Repository
{
    public interface IUserRepository
    {
        User Login(LoginDTO credentials);
        ResultStatus ChangePassword(MyProfileDTO model, int userId);
    }
}