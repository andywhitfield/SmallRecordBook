using System.Text;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmallRecordBook.Web.Pages;

public class SigninModel(ILogger<SigninModel> logger, IConfiguration configuration, IFido2 fido2) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }
    [BindProperty]
    public string? Email { get; set; }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        logger.LogInformation($"Checking user: {Email} / {ReturnUrl}");

        if (string.IsNullOrEmpty(Email))
            return Page();

        var options = fido2.RequestNewCredential(
            new Fido2User() { Id = Encoding.UTF8.GetBytes(Email), Name = Email, DisplayName = Email },
            [],
            AuthenticatorSelection.Default,
            AttestationConveyancePreference.None,
            new()
            {
                Extensions = true,
                UserVerificationMethod = true,
                AppID = configuration.GetValue<string>("FidoOrigins")
            }
        ).ToJson();

        logger.LogInformation($"Posting to verify: {Email}");
        
        return RedirectToPage("./signinverify", routeValues: new { ReturnUrl, Email, VerifyOptions = options });

        /*
        if (!ModelState.IsValid)
            return View("Login", new LoginViewModel(HttpContext, returnUrl));

        var response = await mediator.Send(new SigninRequest(email));
        return View("LoginVerify", new LoginVerifyViewModel(HttpContext, returnUrl, email,
            response.IsReturningUser, response.VerifyOptions));
        */
    }
}
