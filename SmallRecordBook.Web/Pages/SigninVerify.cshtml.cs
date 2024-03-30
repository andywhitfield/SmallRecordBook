using System.Security.Claims;
using System.Text.Json;
using Fido2NetLib;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmallRecordBook.Web.Pages;

public class SigninVerifyModel(ILogger<SigninVerifyModel> logger, IFido2 fido2) : PageModel
{
    public bool IsReturningUser { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Email { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }
    [BindProperty(SupportsGet = true)]
    public string? VerifyOptions { get; set; }
    [BindProperty]
    public string? VerifyResponse { get; set; }

    public void OnGet()
    {
        IsReturningUser = false; // TODO
    }

    public async Task<IActionResult> OnPost()
    {
        // TODO: just a bare bones implementation...assuming a new user
        var options = CredentialCreateOptions.FromJson(VerifyOptions);

        AuthenticatorAttestationRawResponse? authenticatorAttestationRawResponse = JsonSerializer.Deserialize<AuthenticatorAttestationRawResponse>(VerifyResponse ?? "");
        if (authenticatorAttestationRawResponse == null)
        {
            logger.LogWarning($"Cannot parse signin verify response: {VerifyResponse}");
            return Page();
        }

        logger.LogTrace($"Successfully parsed response: {VerifyResponse}");

        var success = await fido2.MakeNewCredentialAsync(authenticatorAttestationRawResponse, options, (_, _) => Task.FromResult(true));
        logger.LogInformation($"got success status: {success.Status} error: {success.ErrorMessage}");
        if (success.Result == null)
        {
            logger.LogWarning($"Could not create new credential: {success.Status} - {success.ErrorMessage}");
            return Page();
        }

        logger.LogTrace($"Got new credential: {JsonSerializer.Serialize(success.Result)}");

        List<Claim> claims = [new Claim(ClaimTypes.Name, Email!)];
        ClaimsIdentity claimsIdentity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        AuthenticationProperties authProperties = new() { IsPersistent = true };
        await Request.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

        logger.LogTrace($"Signed in: {Email}");

        return Redirect("./");
    }
}
