using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using DotNetOpenAuth.GoogleOAuth2;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.RelyingParty;
using Microsoft.Web.WebPages.OAuth;
using NuGetFeed.Infrastructure.Repositories;
using NuGetFeed.Models;

namespace NuGetFeed.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRepository _userRepository;
        private static readonly OpenIdRelyingParty OpenIdRelyingParty = new OpenIdRelyingParty();

        public AccountController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ActionResult LogIn(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        public ActionResult OpenId(string openIdUrl)
        {
            var response = OpenIdRelyingParty.GetResponse();
            if (response == null)
            {
                Identifier id;
                if (Identifier.TryParse(openIdUrl, out id))
                {
                    var request = OpenIdRelyingParty.CreateRequest(openIdUrl);
                    var fetch = new FetchRequest();
                    fetch.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
                    fetch.Attributes.AddRequired(WellKnownAttributes.Name.First);
                    fetch.Attributes.AddRequired(WellKnownAttributes.Name.Last);
                    request.AddExtension(fetch);
                    return request.RedirectingResponse.AsActionResult();
                }

                return RedirectToAction("Index", "Home");
            }

            switch (response.Status)
            {
                case AuthenticationStatus.Authenticated:
                    var fetch = response.GetExtension<FetchResponse>();
                    string firstName = "unknown";
                    string lastName = "unknown";
                    string email = "unknown";
                    if (fetch != null)
                    {
                        firstName = fetch.GetAttributeValue(WellKnownAttributes.Name.First);
                        lastName = fetch.GetAttributeValue(WellKnownAttributes.Name.Last);
                        email = fetch.GetAttributeValue(WellKnownAttributes.Contact.Email);
                    }

                    return CreateUser(response.ClaimedIdentifier, firstName, lastName, email);
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private ActionResult CreateUser(string userName, string firstName, string lastName, string email)
        {
            var user = _userRepository.GetByUsername(userName);
            if (user == null)
            {
                user = new User
                           {
                               Username = userName,
                               FirstName = firstName,
                               LastName = lastName,
                               Email = email
                           };
                _userRepository.Insert(user);
            }

            FormsAuthentication.SetAuthCookie(userName, true);

            if (Request.QueryString["ReturnUrl"] != null)
            {
                return Redirect(Request.QueryString["ReturnUrl"]);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }


        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            GoogleOAuth2Client.RewriteRequest();
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            return CreateUser(result.UserName, result.ExtraData["given_name"], result.ExtraData["family_name"],
                result.ExtraData["email"]);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

    }

    internal class ExternalLoginResult : ActionResult
    {
        public ExternalLoginResult(string provider, string returnUrl)
        {
            Provider = provider;
            ReturnUrl = returnUrl;
        }

        public string Provider { get; private set; }
        public string ReturnUrl { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
        }
    }

}
