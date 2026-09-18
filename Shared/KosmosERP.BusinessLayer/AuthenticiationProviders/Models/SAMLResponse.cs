using KosmosERP.Models.Interfaces;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace KosmosERP.BusinessLayer.AuthenticiationProviders.Models
{
    public class SAMLResponse
    {
        public XmlDocument SAMLDocument { get; private set; }
        private IAuthenticationSettings _AuthenticationSettings;

        public SAMLResponse() { }

        public SAMLResponse(IAuthenticationSettings authenticationSettings)
        {
            _AuthenticationSettings = authenticationSettings;
        }

        public void LoadXml(string xml)
        {
            SAMLDocument = new XmlDocument
            {
                PreserveWhitespace = true,
                XmlResolver = null
            };

            SAMLDocument.LoadXml(xml);
        }

        public void LoadXmlFromBase64(string response)
        {
            var aSCIIEncoding = new ASCIIEncoding();
            LoadXml(aSCIIEncoding.GetString(Convert.FromBase64String(response)));
        }

        public bool IsValid()
        {
            var certificate = new SAMLCertificate();
            certificate.LoadCertificate(_AuthenticationSettings.IdPCertificate);
            var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            var xmlNodeList = SAMLDocument.SelectNodes("//ds:Signature", xmlNamespaceManager);
            var signedXml = new SignedXml(SAMLDocument);
            signedXml.LoadXml((XmlElement)xmlNodeList[0]);
            return signedXml.CheckSignature(certificate.Certificate, true);
        }

        public bool IsValid(string ssoCertificate)
        {
            var certificate = new SAMLCertificate();
            certificate.LoadCertificate(ssoCertificate);
            var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            var xmlNodeList = SAMLDocument.SelectNodes("//ds:Signature", xmlNamespaceManager);
            var signedXml = new SignedXml(SAMLDocument);
            signedXml.LoadXml((XmlElement)xmlNodeList[0]);
            return signedXml.CheckSignature(certificate.Certificate, true);
        }

        public string GetNameID()
        {
            var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            xmlNamespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            xmlNamespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            
            var xmlNode = SAMLDocument.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:NameID", xmlNamespaceManager);
            
            return xmlNode.InnerText;
        }

        public string EmployeeID()
        {
            var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            xmlNamespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            xmlNamespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            
            var xmlNode = SAMLDocument.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:EmployeeID", xmlNamespaceManager);
           
            return xmlNode.InnerText;
        }

        public string EmployeeRoles()
        {
            var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            xmlNamespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            xmlNamespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            
            var xmlNode = SAMLDocument.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:EmployeeRoles", xmlNamespaceManager);
            
            return xmlNode.InnerText;
        }

        public DateTime GetStartDateTime()
        {
            var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            xmlNamespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            xmlNamespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            
            var xmlNode = SAMLDocument.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Conditions", xmlNamespaceManager);
            
            var maxValue = DateTime.MaxValue;

            DateTime.TryParse(xmlNode.Attributes["NotBefore"].Value, out maxValue);

            return maxValue.AddSeconds(-5);
        }

        public DateTime GetEndDateTime()
        {
            var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            xmlNamespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            xmlNamespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            
            var xmlNode = SAMLDocument.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Conditions", xmlNamespaceManager);
            
            var minValue = DateTime.MinValue;

            DateTime.TryParse(xmlNode.Attributes["NotOnOrAfter"].Value, out minValue);

            return minValue;
        }

        public string GetSAMLKey()
        {
            var xmlNamespaceManager = new XmlNamespaceManager(SAMLDocument.NameTable);
            xmlNamespaceManager.AddNamespace("ds", "http://www.w3.org/2000/09/xmldsig#");
            xmlNamespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            xmlNamespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
           
            var xmlNode = SAMLDocument.SelectSingleNode("/samlp:Response/@InResponseTo", xmlNamespaceManager);

            return xmlNode != null ? xmlNode.Value : "";

        }
    }

}
