using System;

namespace LegacyApp
{
    public class UserService
    {
        private readonly ClientRepository _clientRepository;
        public UserService()
        {
            _clientRepository = new ClientRepository();
        }

        public bool AddUser(string firname, string surname, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firname) || string.IsNullOrEmpty(surname))
            {
                return false;
            }

            if (email.Contains("@") && !email.Contains("."))
            {
                return false;
            }
                       
            if (CalculateAge(dateOfBirth) < 21)
            {
                return false;
            }                                
            
            var user = new User
            {
                Client = _clientRepository.GetById(clientId),
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                Firstname = firname,
                Surname = surname
            };

            SetCreditLimit(user);

            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }
            
            UserDataAccess.AddUser(user);

            return true;
        }

        private void SetCreditLimit(User user)
        {
            if (user.Client.Name == "VeryImportantClient")
            {
                // Skip credit check
                user.HasCreditLimit = false;
            }
            else
            {
                // Do credit check
                user.HasCreditLimit = true;
                using (var userCreditService = new UserCreditServiceClient())
                {
                    var creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);

                    if (user.Client.Name == "ImportantClient")
                        creditLimit = creditLimit * 2; //double credit limit

                    user.CreditLimit = creditLimit;
                }
            }
        }

        private int CalculateAge(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;

            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }

            return age;
        }
    }
}