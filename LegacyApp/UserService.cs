using System;

namespace LegacyApp
{
    public interface IClientRepository
    {
         Client GetById(int IdClient);
    }
    public interface ICreditLimitService
    {
         int GetCreditLimit(string lastName, DateTime birthDate);
    }
    
    public interface IUserDataAdapter
    {
        void AddUser(User user);
    }
    
    public class UserDataAccessAdapter : IUserDataAdapter
    {
        public void AddUser(User user)
        {
            UserDataAccess.AddUser(user);
        }
    }
    public class UserService
    {
        private IClientRepository _clientRepository;
        private ICreditLimitService _creditLimitService;
        private IUserDataAdapter _userDataAdapter;

        
        public UserService(IClientRepository clientRepository,ICreditLimitService creditLimitService , IUserDataAdapter userDataAdapter)
        {
            _clientRepository = clientRepository;
            _creditLimitService = creditLimitService;
            _userDataAdapter = userDataAdapter;
        }
        
        [Obsolete]
        public UserService()
        {
            _clientRepository = new ClientRepository();
            _creditLimitService = new UserCreditService();
            _userDataAdapter = new UserDataAccessAdapter();
        }

        public bool CheckName(string first, string last)
        {
            return string.IsNullOrEmpty(first) || string.IsNullOrEmpty(last);
        }

        public bool CheckEmail(string email)
        {
            return !email.Contains("@") && !email.Contains(".");
        }

        public bool CheckAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
                age--;

            return age < 21;
        }

        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            
            if (CheckName(firstName, lastName) || CheckEmail(email) || CheckAge(dateOfBirth))
            {
                return false;
            }
            
            var client = _clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else if (client.Type == "ImportantClient")
            {

                int creditLimit = _creditLimitService.GetCreditLimit(user.LastName, user.DateOfBirth) * 2;
                    user.CreditLimit = creditLimit;
                
            }
            else
            {
                user.HasCreditLimit = true;
                {
                    int creditLimit = _creditLimitService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = creditLimit;
                }
            }

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            _userDataAdapter.AddUser(user);
            return true;
        }
    }
}
