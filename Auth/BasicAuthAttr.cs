// BasicAuthorizationAttribute.cs
using Microsoft.AspNetCore.Authorization;

public class BasicAuthAttribute : AuthorizeAttribute
{
	public BasicAuthAttribute()
	{
		Policy = "BasicAuth";
	}
}


