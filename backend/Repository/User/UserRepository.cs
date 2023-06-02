namespace ACS.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ACSDbContext _dbContext;

        public UserRepository(ACSDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public User Login(LoginDTO credentials)
        {
            try
            {
                User getUser = _dbContext.Users.FirstOrDefault(x => x.Username == credentials.Username);

                if (getUser != null)
                {
                    bool verifyPassword = PasswordHelper.VerifyPassword(credentials.Password, getUser.PasswordHash, getUser.PasswordSalt);

                    if (verifyPassword)
                    {
                        return getUser;
                    }
                }

                return null;
            }
            catch (System.Exception)
            {

                throw;
            }
        }
        public ResultStatus ChangePassword(MyProfileDTO credentials, int userId)
        {
            try
            {
                User getUser = _dbContext.Users.FirstOrDefault(x => x.Id == userId);

                if (getUser != null)
                {
                    bool verifyCurrPassword = PasswordHelper.VerifyPassword(credentials.CurrentPassword, getUser.PasswordHash, getUser.PasswordSalt);

                    if (verifyCurrPassword)
                    {
                        byte[] salt;
                        string newPassword = PasswordHelper.HashPassword(credentials.NewPassword, out salt);

                        getUser.PasswordHash = newPassword;
                        getUser.PasswordSalt = salt;

                        _dbContext.SaveChanges();

                        return ResultStatus.PasswordChanged;
                    }
                    else
                    {
                        return ResultStatus.PasswordNotChanged;
                    }
                }

                return ResultStatus.Err;
            }
            catch (System.Exception)
            {

                throw;
            }
        }

    }
}