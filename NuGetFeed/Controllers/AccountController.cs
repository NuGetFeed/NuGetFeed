using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.RelyingParty;
using Norm;
using NuGetFeed.Models;

namespace NuGetFeed.Controllers
{
    public class AccountController : Controller
    {
        private static readonly OpenIdRelyingParty OpenIdRelyingParty = new OpenIdRelyingParty();

        private readonly IMongo _mongo;

        public AccountController(IMongo mongo)
        {
            _mongo = mongo;
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

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        private ActionResult CreateUser(string userName, string firstName, string lastName, string email)
        {
            var users = _mongo.GetCollection<User>();

            User user = users.AsQueryable().SingleOrDefault(u => u.Username == userName);
            if (user == null)
            {
                user = new User
                           {
                               Username = userName,
                               FirstName = firstName,
                               LastName = lastName,
                               Email = email
                           };
                users.Insert(user);
            }

            FormsAuthentication.SetAuthCookie(userName, true);
            return RedirectToAction("Index", "Home");
        }
    }
}
