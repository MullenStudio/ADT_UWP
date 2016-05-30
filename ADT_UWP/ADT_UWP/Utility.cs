//-----------------------------------------------------------------------
// <copyright file="Utility.cs" company="Mullen Studio">
//     Copyright (c) Mullen Studio. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MullenStudio.ADT_UWP
{
    using Windows.Security.Credentials;

    /// <summary>
    /// Provides utility methods.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Clears all passwords in the vault.
        /// </summary>
        /// <param name="passwordVault">The password vault.</param>
        public static void ClearAllPasswords(this PasswordVault passwordVault)
        {
            foreach (var passwordCredential in passwordVault.RetrieveAll())
            {
                passwordVault.Remove(passwordCredential);
            }
        }

        /// <summary>
        /// Retrieves the first password credential in the vault.
        /// </summary>
        /// <param name="passwordVault">The password vault.</param>
        /// <returns>The first password credential, or null if there is no password credential.</returns>
        public static PasswordCredential Retrieve(this PasswordVault passwordVault)
        {
            var passwordCredentials = passwordVault.RetrieveAll();
            if (passwordCredentials.Count == 0)
            {
                return null;
            }

            return passwordVault.Retrieve(passwordCredentials[0].Resource, passwordCredentials[0].UserName);
        }
    }
}
