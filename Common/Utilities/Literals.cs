using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    public class Literals
    {
        #region Culturas

        public static string CultureEn = "en-US";
        public static string CultureEs = "es-ES";

        #endregion

        #region Controllers

        public static string UsersController = "Users";
        public static string TicketsController = "Tickets";
        public static string MessagesController = "Messages";

        #endregion

        #region Roles

        public static string Role_SupportManager = "SupportManager";
        public static string Role_SupportTechnician = "SupportTechnician";

        #endregion

        #region Claims de los usuarios

        public static string Claim_FullName = "FullName";
        public static string Claim_LanguageId = "LanguageId";
        public static string Claim_Role = "Role";
        public static string Claim_UserId = "UserId";
        public static string Claim_Email = "Email";
        public static string Claim_PhoneNumber = "PhoneNumber";

        #endregion
    }
}
