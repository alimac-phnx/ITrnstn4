using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Data;

namespace ITrnstn4Old.Data
{
    public class DbErrorHandler
    {
        private readonly Constraint nickConstraint = new Constraint("CK_User_Nickname_Length", "Nickname", "The nickname must be between 2 to 20 characters.");
        
        private readonly Constraint emailConstraint = new Constraint("IX_Users_Email", "UNIQUE", "The email has already been registered.");
        
        private readonly Constraint passwordConstraint = new Constraint("CK_User_Password_Validation", "Password", "The password must be 6-16 characters long, contain at least one uppercase letter, one lowercase letter, and one digit.");

        public void HandleDatabaseError(Exception ex, ModelStateDictionary modelState)
        {
            string message = ex.InnerException.Message;

            CheckConstraint(nickConstraint, message, modelState);

            CheckUniqueConstraint(emailConstraint, message, modelState);

            CheckConstraint(passwordConstraint, message, modelState);
        }

        private void PrintError(Constraint constraint, ModelStateDictionary modelState)
        {
            modelState.AddModelError(constraint.Type, constraint.ErrorMessage);
        }

        private void CheckConstraint(Constraint constraint, string message, ModelStateDictionary modelState)
        {
            if (constraint.Check(message))
            {
                PrintError(constraint, modelState);

                return;
            }
        }

        private void CheckUniqueConstraint(Constraint constraint, string message, ModelStateDictionary modelState)
        {
            if (constraint.CheckUniqueness(message))
            {
                PrintError(constraint, modelState);

                return;
            }
        }
    }
}
