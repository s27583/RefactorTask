using System;

namespace LegacyApp
{

    public interface IClientRepository
    {
        Client GetById(int clientId);
    }

    public interface IUserCreditService
    {
        int GetCreditLimit(string lastName, DateTime dateOfBirth);
    }


    public class UserService
    {
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            if (!ValidateUserInput(firstName, lastName, email, dateOfBirth))
                return false;

            var user = CreateUser(firstName, lastName, email, dateOfBirth, clientId);
            if (user == null)
                return false;

            SetCreditLimit(user);

            if (!IsValidCreditLimit(user))
                return false;

            SaveUser(user);

            return true;
        }

        private bool ValidateUserInput(string firstName, string lastName, string email, DateTime dateOfBirth)
        {
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                return false;

            if (!email.Contains("@") || !email.Contains("."))
                return false;

            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day))
                age--;

            if (age < 21)
                return false;

            return true;
        }

        private User CreateUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            var clientRepository = new ClientRepository();
            var client = clientRepository.GetById(clientId);

            if (client == null)
                return null;

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };

            return user;
        }

        private void SetCreditLimit(User user)
        {
            var userCreditService = new UserCreditService();
            if (user.Client is Client client)
            {
                if (client.Type == "VeryImportantClient")
                {
                    user.HasCreditLimit = false;
                }
                else
                {
                    int creditLimit = userCreditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                    if (client.Type == "ImportantClient")
                    {
                        user.CreditLimit = creditLimit * 2;
                    }
                    else
                    {
                        user.CreditLimit = creditLimit;
                    }
                    user.HasCreditLimit = true;
                }
            }
            else
            {
                throw new ArgumentException("User's client is not of type Client");
            }
        }


        private bool IsValidCreditLimit(User user)
        {
            return !user.HasCreditLimit || user.CreditLimit >= 500;
        }

        private void SaveUser(User user)
        {
            UserDataAccess.AddUser(user);
        }
    }

}
