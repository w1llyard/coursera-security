using System;
using System.Collections.Generic;
using System.Linq;

namespace SafeVault.Web.Helpers
{
    public static class ValidationHelpers
    {
        public static bool IsValidInput(string input, string allowedSpecialCharacters = "!")
        {
            if (string.IsNullOrEmpty(input))
                return false;

            var validCharacters = allowedSpecialCharacters.ToHashSet();
            return input.All(c => char.IsLetterOrDigit(c) || validCharacters.Contains(c));
        }

        public static bool IsValidXSSInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            string lowerInput = input.ToLower();
            if (lowerInput.Contains("<script") || lowerInput.Contains("<iframe"))
                return false;

            return true;
        }
    }
}
