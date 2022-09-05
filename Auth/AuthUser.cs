// AuthenticatedUser.cs
using System.Security.Principal;

public class AuthUser : IIdentity
{
	public AuthUser(string authType, bool isAuth, string name)
	{
		AuthenticationType = authType;
		IsAuthenticated = isAuth;
		Name = name;
	}

	public string AuthenticationType { get; }

	public bool IsAuthenticated { get; }

	public string Name { get; }
}