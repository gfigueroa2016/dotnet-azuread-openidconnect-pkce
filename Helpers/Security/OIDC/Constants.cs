using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureAD.DotNet.Helpers.Security.OIDC
{
    /// <summary>
    /// claim keys constants
    /// </summary>
    public static class ClaimConstants
    {
        public const string ObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";
        public const string tid = "tid";
        public const string PreferredUsername = "preferred_username";
        public const string Email = "email";
        public const string SamAccountName = "sAMAccountName";
        public const string GivenName = "name";
        public readonly static string JobClassId = "JobClassID";
        public readonly static string EmployeeID = "EmployeeID";
        public readonly static string Title = "Title";
        public readonly static string FacilityNumber = "FacilityNumber";
        public readonly static string Division = "Division";
        public readonly static string JobClassDescription = "JobClassDescription";
        public readonly static string Roles = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    }
}