using System;

namespace LegacyApp
{
    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            if (!IsValidEmail(email))
            {
                return false;
            }

            if (!IsOldEnough(dateOfBirth))
            {
                return false;
            }

            var clientType = GetClientType(clientId);

            var user = CreateUser(firstName, lastName, email, dateOfBirth, clientType);

            SetUserCreditLimit(user, clientType);

            if (ShouldRejectUser(user))
            {
                return false;
            }

            UserDataAccess.AddUser(user);
            return true;
        }

        private bool IsValidEmail(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }

        private bool IsOldEnough(DateTime dateOfBirth)
        {
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
            {
                age--;
            }

            return age >= 21;
        }

        private string GetClientType(int clientId)
        {
            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);
            return client != null ? client.Type : null;
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, string clientType)
        {
            return new User
            {
                Client = null,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
        }

        private void SetUserCreditLimit(User user, string clientType)
        {
            if (clientType == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else
            {
                user.HasCreditLimit = true;
                using (var userCreditService = new UserCreditService())
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    user.CreditLimit = clientType == "ImportantClient" ? creditLimit * 2 : creditLimit;
                }
            }
        }

        private bool ShouldRejectUser(User user)
        {
            return user.HasCreditLimit && user.CreditLimit < 500;
        }
    }
}
