
using System.Security.Cryptography.X509Certificates;

namespace KosmosERP.BusinessLayer.AuthenticiationProviders.Models;

public class SAMLCertificate
{
    public X509Certificate2? Certificate { get; private set; }

    public void LoadCertificate(string certificate)
    {
        Certificate = X509CertificateLoader.LoadCertificate(StringToByteArray(certificate));
    }

    public void LoadCertificate(byte[] certificate)
    {
        Certificate = X509CertificateLoader.LoadCertificate(certificate);
    }

    private byte[] StringToByteArray(string st)
    {
        var array = new byte[st.Length];

        for (var i = 0; i < st.Length; i++)
            array[i] = (byte)st[i];

        return array;
    }
}
