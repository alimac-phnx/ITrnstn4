using System.Data;

namespace ITrnstn4Old.Data
{
    public class Constraint
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string ErrorMessage { get; set; }

        public Constraint(string name, string type, string errorMessage)
        {
            Name = name;
            Type = type;
            ErrorMessage = errorMessage;
        }

        public bool Check(string message)
        {
            return message.Contains(Name);
        }

        public bool CheckUniqueness(string message)
        {
            string[] uniqueKeywords = new string[] { Name, Type };

            return uniqueKeywords.Any(keyword => message.Contains(keyword));
        }
    }
}
