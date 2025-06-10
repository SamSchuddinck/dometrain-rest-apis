using System;

namespace Movies.Api;

public static class AuthContstants
{
    public const string AdminUserPolicyName = "Admin";
    public const string AdminUserClaimName = "admin";

    public const string TrusterMemberPolicyName = "TrustedMember";
    public const string TrusterMemberClaimName = "trusted_member";
}
