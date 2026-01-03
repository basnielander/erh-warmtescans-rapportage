using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace ERH.HeatScans.Reporting.Server.Framework.Controllers;

/// <summary>
/// Base controller that provides common functionality for authorized API endpoints
/// </summary>
public abstract class AuthorizedApiController : ApiController
{
    /// <summary>
    /// Validates required parameters
    /// </summary>
    /// <param name="paramName">Parameter name for error message</param>
    /// <param name="paramValue">Parameter value to validate</param>
    /// <returns>BadRequest result if validation fails, null otherwise</returns>
    protected IHttpActionResult ValidateRequired(string paramName, string paramValue)
    {
        if (string.IsNullOrWhiteSpace(paramValue))
        {
            return BadRequest($"{paramName} is required");
        }
        return null;
    }

    /// <summary>
    /// Validates required object parameters
    /// </summary>
    /// <param name="paramName">Parameter name for error message</param>
    /// <param name="paramValue">Parameter value to validate</param>
    /// <returns>BadRequest result if validation fails, null otherwise</returns>
    protected IHttpActionResult ValidateRequired(string paramName, object paramValue)
    {
        if (paramValue == null)
        {
            return BadRequest($"{paramName} is required");
        }
        return null;
    }

    /// <summary>
    /// Gets the access token from the request and validates it
    /// </summary>
    /// <param name="accessToken">Output parameter for the access token</param>
    /// <returns>Unauthorized result if token is missing/invalid, null otherwise</returns>
    protected IHttpActionResult ValidateAccessToken(out string accessToken)
    {
        accessToken = AccessToken.Get(Request);
        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized();
        }
        return null;
    }

    /// <summary>
    /// Executes an action with automatic error handling
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <returns>Result of the action or InternalServerError on exception</returns>
    protected IHttpActionResult ExecuteWithErrorHandling(Func<IHttpActionResult> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }

    /// <summary>
    /// Executes an async action with automatic error handling
    /// </summary>
    /// <param name="action">Async action to execute</param>
    /// <returns>Result of the action or InternalServerError on exception</returns>
    protected async Task<IHttpActionResult> ExecuteWithErrorHandlingAsync(Func<Task<IHttpActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            return InternalServerError(ex);
        }
    }

    /// <summary>
    /// Executes an authorized async action with automatic access token validation and error handling
    /// </summary>
    /// <param name="action">Async action to execute with access token</param>
    /// <returns>Result of the action, Unauthorized, or InternalServerError</returns>
    protected async Task<IHttpActionResult> ExecuteAuthorizedAsync(Func<string, Task<IHttpActionResult>> action)
    {
        return await ExecuteWithErrorHandlingAsync(async () =>
        {
            var authResult = ValidateAccessToken(out var accessToken);
            if (authResult != null)
            {
                return authResult;
            }

            return await action(accessToken);
        });
    }

    /// <summary>
    /// Executes an authorized action with automatic access token validation and error handling
    /// </summary>
    /// <param name="action">Action to execute with access token</param>
    /// <returns>Result of the action, Unauthorized, or InternalServerError</returns>
    protected IHttpActionResult ExecuteAuthorized(Func<string, IHttpActionResult> action)
    {
        return ExecuteWithErrorHandling(() =>
        {
            var authResult = ValidateAccessToken(out var accessToken);
            if (authResult != null)
            {
                return authResult;
            }

            return action(accessToken);
        });
    }
}
