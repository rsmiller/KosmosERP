using System.IO.Compression;
using System.Text;
using Microsoft.EntityFrameworkCore;
using KosmosERP.BusinessLayer;
using KosmosERP.BusinessLayer.AuthenticiationProviders.Models;
using KosmosERP.BusinessLayer.Modules;
using KosmosERP.Models;
using KosmosERP.Tests.Modules.Shared;

namespace KosmosERP.Tests.Modules
{
    public class SAMLModuleTests : BaseTestModule<SAMLModule>
    {
        // Well-known settings the assertions below are built against.
        private const string BaseUrl = "https://sp.example.com/";
        private const string EntityId = "https://sp.example.com/metadata";
        private const string SsoUrl = "https://idp.example.com/sso";
        private const string SloUrl = "https://idp.example.com/slo";

        private const string AcsUrl = "https://sp.example.com/api/v1/SAML/consume";
        private const string SpSloUrl = "https://sp.example.com/api/v1/SAML/logout/response";

        [SetUp]
        public async Task SetupModule()
        {
            var the_module = BuildModule(CreateSettings());

            await base.SetupModule(the_module);
        }

        protected override async Task SetupData()
        {
            await base.SetupData();
        }

        // ---------- Metadata ----------

        [Test]
        public void GetMetadata_DescribesTheServiceProvider()
        {
            var metadata = _Module.GetMetadata();

            Assert.That(metadata, Does.Contain($"entityID=\"{EntityId}\""));
            Assert.That(metadata, Does.Contain("<md:SPSSODescriptor"));
            Assert.That(metadata, Does.Contain($"<md:AssertionConsumerService"));
            Assert.That(metadata, Does.Contain($"Location=\"{AcsUrl}\""));
            Assert.That(metadata, Does.Contain("Binding=\"urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST\""));
        }

        [Test]
        public void GetMetadata_AdvertisesSingleLogoutBeforeAcs()
        {
            var metadata = _Module.GetMetadata();

            Assert.That(metadata, Does.Contain("<md:SingleLogoutService"));
            Assert.That(metadata, Does.Contain($"Location=\"{SpSloUrl}\""));

            // Schema ordering: SingleLogoutService must precede AssertionConsumerService.
            Assert.That(metadata.IndexOf("<md:SingleLogoutService"),
                Is.LessThan(metadata.IndexOf("<md:AssertionConsumerService")));
        }

        // ---------- BeginSAML (SP-initiated login) ----------

        [Test]
        public async Task BeginSAML_BuildsAuthnRequestAndRedirect()
        {
            var result = await _Module.BeginSAML(new SAMLRequest { RelayState = "/dashboard" });

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data.Id, Does.StartWith("_"));
            Assert.That(result.Data.RelayState, Is.EqualTo("/dashboard"));

            // Redirect targets the IdP SSO endpoint and carries the encoded request + relay state.
            Assert.That(result.Data.RedirectUrl, Does.StartWith($"{SsoUrl}?SAMLRequest="));
            Assert.That(result.Data.RedirectUrl, Does.Contain("RelayState=%2Fdashboard"));

            // The encoded request inflates to a well-formed AuthnRequest pointing back at our ACS.
            var xml = Inflate(result.Data.Request);
            Assert.That(xml, Does.Contain("<samlp:AuthnRequest"));
            Assert.That(xml, Does.Contain($"Destination=\"{SsoUrl}\""));
            Assert.That(xml, Does.Contain($"AssertionConsumerServiceURL=\"{AcsUrl}\""));
            Assert.That(xml, Does.Contain($"<saml:Issuer>{EntityId}</saml:Issuer>"));
            Assert.That(xml, Does.Contain($"ID=\"{result.Data.Id}\""));
        }

        [Test]
        public async Task BeginSAML_FailsWhenAuthorityNotConfigured()
        {
            var settings = CreateSettings();
            settings.Authority = "";
            var module = BuildModule(settings);

            var result = await module.BeginSAML(new SAMLRequest());

            Assert.That(result.Success, Is.False);
            Assert.That(result.ResultCode, Is.EqualTo(ResultCode.Invalid));
        }

        [Test]
        public async Task BeginSAML_FailsWhenBaseUrlNotConfigured()
        {
            var settings = CreateSettings();
            settings.BaseURL = "";
            var module = BuildModule(settings);

            var result = await module.BeginSAML(new SAMLRequest());

            Assert.That(result.Success, Is.False);
            Assert.That(result.ResultCode, Is.EqualTo(ResultCode.Invalid));
        }

        // ---------- Consume (login) guard paths ----------
        // Full signature validation needs a signed assertion + matching IdP cert (see notes),
        // but the input guards are deterministic and worth pinning.

        [Test]
        public async Task Consume_RejectsNullResponse()
        {
            var result = await _Module.LoginSAML(new SAMLWrapper { Response = null });

            Assert.That(result.Success, Is.False);
            Assert.That(result.ResultCode, Is.EqualTo(ResultCode.NullItemInput));
        }

        [Test]
        public async Task Consume_RejectsEmptyResponse()
        {
            var result = await _Module.LoginSAML(new SAMLWrapper { Response = "" });

            Assert.That(result.Success, Is.False);
            Assert.That(result.ResultCode, Is.EqualTo(ResultCode.NullItemInput));
        }

        // ---------- BeginLogout (SP-initiated SLO) ----------

        [Test]
        public async Task BeginLogout_BuildsLogoutRequestForTheSessionUser()
        {
            var result = await _Module.BeginLogout(_SessionId, "/goodbye");

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.Not.Null);
            Assert.That(result.Data.Id, Does.StartWith("_"));
            Assert.That(result.Data.RedirectUrl, Does.StartWith($"{SloUrl}?SAMLRequest="));

            var xml = Inflate(result.Data.Request);
            Assert.That(xml, Does.Contain("<samlp:LogoutRequest"));
            Assert.That(xml, Does.Contain($"Destination=\"{SloUrl}\""));
            Assert.That(xml, Does.Contain($"<saml:Issuer>{EntityId}</saml:Issuer>"));
            // The subject is the IdP NameID (the user's external id).
            Assert.That(xml, Does.Contain($">{_User.external_id}</saml:NameID>"));
        }

        [Test]
        public async Task BeginLogout_TerminatesTheLocalSession()
        {
            await _Module.BeginLogout(_SessionId, null);

            var session = await _Context.UserSessionStates.FirstAsync(m => m.session_id == _SessionId);

            // The provider expires the session to now; it must no longer be valid into the future.
            Assert.That(session.session_expires, Is.LessThanOrEqualTo(DateTime.UtcNow.AddSeconds(2)));
        }

        [Test]
        public async Task BeginLogout_FailsWhenSessionIdMissing()
        {
            var result = await _Module.BeginLogout("", null);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ResultCode, Is.EqualTo(ResultCode.NullItemInput));
        }

        [Test]
        public async Task BeginLogout_FailsWhenSessionNotFound()
        {
            var result = await _Module.BeginLogout(Guid.NewGuid().ToString(), null);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ResultCode, Is.EqualTo(ResultCode.NotFound));
        }

        [Test]
        public async Task BeginLogout_FailsWhenIdpLogoutUrlNotConfigured()
        {
            var settings = CreateSettings();
            settings.SingleLogoutURL = "";
            settings.Authority = ""; // logout url falls back to Authority, so clear both
            var module = BuildModule(settings);

            var result = await module.BeginLogout(_SessionId, null);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ResultCode, Is.EqualTo(ResultCode.Invalid));
        }

        [Test]
        public async Task BeginLogout_FallsBackToAuthorityWhenNoDedicatedSloUrl()
        {
            var settings = CreateSettings();
            settings.SingleLogoutURL = "";
            var module = BuildModule(settings);

            var result = await module.BeginLogout(_SessionId, null);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data.RedirectUrl, Does.StartWith($"{SsoUrl}?SAMLRequest="));
        }

        // ---------- CompleteLogout (IdP LogoutResponse) ----------

        [Test]
        public async Task CompleteLogout_SucceedsOnSuccessStatus()
        {
            var wrapper = new SAMLWrapper { Response = EncodeLogoutResponse(SAMLLogoutResponse.StatusSuccess) };

            var result = await _Module.CompleteLogout(wrapper);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Data, Is.True);
        }

        [Test]
        public async Task CompleteLogout_FailsOnNonSuccessStatus()
        {
            var wrapper = new SAMLWrapper
            {
                Response = EncodeLogoutResponse("urn:oasis:names:tc:SAML:2.0:status:Requester")
            };

            var result = await _Module.CompleteLogout(wrapper);

            Assert.That(result.Success, Is.False);
            Assert.That(result.ResultCode, Is.EqualTo(ResultCode.Invalid));
        }

        [Test]
        public async Task CompleteLogout_RejectsEmptyResponse()
        {
            var result = await _Module.CompleteLogout(new SAMLWrapper { Response = "" });

            Assert.That(result.Success, Is.False);
            Assert.That(result.ResultCode, Is.EqualTo(ResultCode.NullItemInput));
        }

        // ---------- helpers ----------

        private SAMLModule BuildModule(AuthenticationSettings settings)
        {
            var logProviderFactory = new LogProviderFactory(
                new LogProviderSettings() { log_provider = LogProviderType.MOCK }, _Context, null);
            var authenticationFactory = new AuthenticationFactory(settings, _Context);

            return new SAMLModule(_Context, authenticationFactory, settings, logProviderFactory);
        }

        private static AuthenticationSettings CreateSettings()
        {
            return new AuthenticationSettings
            {
                AuthenticationProvider = AuthenticiationProviders.SAML,
                APIPrivateKey = "unit-test-private-key-unit-test-private-key",
                Authority = SsoUrl,
                SingleLogoutURL = SloUrl,
                BaseURL = BaseUrl,
                Audience = EntityId,
                IdPCertificate = "",
            };
        }

        /// <summary>Raw-DEFLATE inflate + base64 decode, the inverse of the HTTP-Redirect encoding.</summary>
        private static string Inflate(string encoded)
        {
            var bytes = Convert.FromBase64String(encoded);

            using var input = new MemoryStream(bytes);
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            using var reader = new StreamReader(deflate, Encoding.UTF8);

            return reader.ReadToEnd();
        }

        /// <summary>Builds an (unsigned) LogoutResponse with the given status and base64-encodes it.</summary>
        private static string EncodeLogoutResponse(string statusCode)
        {
            var xml =
                "<samlp:LogoutResponse " +
                    "xmlns:samlp=\"urn:oasis:names:tc:SAML:2.0:protocol\" " +
                    "ID=\"_response1\" Version=\"2.0\" " +
                    "IssueInstant=\"2026-09-18T00:00:00Z\" " +
                    "InResponseTo=\"_request1\">" +
                    "<samlp:Status>" +
                        $"<samlp:StatusCode Value=\"{statusCode}\" />" +
                    "</samlp:Status>" +
                "</samlp:LogoutResponse>";

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));
        }
    }
}
