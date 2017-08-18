using Jose;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Axinom.Toolkit
{
    public static partial class JoseHelpers
    {
        /// <summary>
        /// Encrypts a blob of data and digitally signs it, returning the resulting JOSE object in compact form.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="data">The data to encrypt and sign.</param>
        /// <param name="encryptFor">Certificate of the recipient. Must contain RSA public key for a 2048-bit key pair or larger.</param>
        /// <param name="signWith">Certificate of the signer. Must be linked to RSA private key at least 2048 bits in length.</param>
        public static string EncryptAndSign(this HelpersContainerClasses.Jose container, byte[] data, X509Certificate2 encryptFor, X509Certificate2 signWith)
        {
            Helpers.Argument.ValidateIsNotNull(data, nameof(data));
            Helpers.Argument.ValidateIsNotNull(encryptFor, nameof(encryptFor));
            Helpers.Argument.ValidateIsNotNull(signWith, nameof(signWith));

            VerifyCertificateIsSaneAndUsable(encryptFor);
            VerifyCertificateAndPrivateKeyIsSaneAndUsable(signWith);

            // Part 1: encrypt.
            var encryptionHeaders = new Dictionary<string, object>
            {
				// Yes, it really is not meant to be base64url for some reason, like the rest of the JOSE object.
                { "x5c", new[] { Convert.ToBase64String(encryptFor.GetRawCertData()) } },
                { "typ", JoseEncryptedType }
            };

            var encrypted = JWT.EncodeBytes(data, encryptFor.GetRSAPublicKey(), JweAlgorithm.RSA_OAEP, JweEncryption.A256CBC_HS512, extraHeaders: encryptionHeaders);

            // Part 2: sign.
            var signingHeaders = new Dictionary<string, object>
            {
				// Yes, it really is not meant to be base64url for some reason, like the rest of the JOSE object.
				{ "x5c", new[] { Convert.ToBase64String(signWith.GetRawCertData()) } },
                { "typ", JoseSignedType }
            };

            return JWT.Encode(encrypted, signWith.GetRSAPrivateKey(), JwsAlgorithm.RS512, extraHeaders: signingHeaders);
        }

        /// <summary>
        /// Decrypts a blob of data encrypted using <see cref="EncryptAndSign"/> after verifying the digital signature on it.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="joseObject">The compact form of the encrypted and signed JOSE object.</param>
        /// <param name="signedBy">The certificate of the signer will be output into this variable.</param>
        /// <param name="decryptionCertificates">The set of certificates whose key pairs may be used for decryption. The correct certificate and key pair will be selected automatically (if it is among the set).</param>
        /// <exception cref="CryptographicException">
        /// Thrown if a cryptographic operation fails (e.g. because you do not have the correct decryption key).
        /// </exception>
        public static byte[] VerifyAndDecrypt(this HelpersContainerClasses.Jose container, string joseObject, out X509Certificate2 signedBy, params X509Certificate2[] decryptionCertificates)
        {
            Helpers.Argument.ValidateIsNotNull(joseObject, nameof(joseObject));
            Helpers.Argument.ValidateIsNotNull(decryptionCertificates, nameof(decryptionCertificates));

            foreach (var decryptionCertificate in decryptionCertificates)
            {
                if (decryptionCertificate == null)
                    throw new ArgumentException("Decryption certificate list cannot contain null values.", nameof(decryptionCertificates));

                VerifyCertificateAndPrivateKeyIsSaneAndUsable(decryptionCertificate);
            }

            // Part 1: verify signature.
            var signedHeader = JWT.Headers(joseObject);

            if (!signedHeader.ContainsKey("typ") || signedHeader["typ"] as string != JoseSignedType)
                throw new ArgumentException("The JOSE object was not produced by " + nameof(EncryptAndSign), nameof(joseObject));

            signedBy = GetCertificateFromJoseHeader(signedHeader);

            VerifyCertificateIsSaneAndUsable(signedBy);

            var encrypted = JWT.Decode(joseObject, signedBy.GetRSAPublicKey(), JwsAlgorithm.RS512);

            // Part 2: decrypt.
            var encryptedHeader = JWT.Headers(encrypted);

            if (!encryptedHeader.ContainsKey("typ") || encryptedHeader["typ"] as string != JoseEncryptedType)
                throw new ArgumentException("The JOSE object was not produced by " + nameof(EncryptAndSign), nameof(joseObject));

            var encryptedFor = GetCertificateFromJoseHeader(encryptedHeader);

            var decryptWith = decryptionCertificates.FirstOrDefault(c => c.Thumbprint == encryptedFor.Thumbprint);

            if (decryptWith == null)
                throw new CryptographicException("None of the available decryption keys matched the one used to encrypt the JOSE object.");

            return JWT.DecodeBytes(encrypted, decryptWith.GetRSAPrivateKey(), JweAlgorithm.RSA_OAEP, JweEncryption.A256CBC_HS512);
        }

        private static X509Certificate2 GetCertificateFromJoseHeader(IDictionary<string, object> header)
        {
            if (!header.ContainsKey("x5c"))
                throw new ArgumentException("The JOSE object header did not reference a X.509 certificate.");

            // Headers dictionary values are different per-platform. Defect?
            // See https://github.com/dvsekhvalnov/jose-jwt/issues/72

            string encodedCertificate;

            switch (header["x5c"])
            {
                case object[] o:
                    if (o.Length != 1)
                        throw new ArgumentException("The JOSE object header did not reference exactly one X.509 certificate.");

                    encodedCertificate = (string)o.Single();
                    break;
                case ICollection<JToken> c:
                    if (c.Count != 1)
                        throw new ArgumentException("The JOSE object header did not reference exactly one X.509 certificate.");

                    encodedCertificate = c.Single().Value<string>();
                    break;
                default:
                    throw new ContractException("Unexpected header data type received from jose-jwt: " + header["x5c"]?.GetType().Name);
            }

            return new X509Certificate2(Convert.FromBase64String(encodedCertificate));
        }

        private const string JoseEncryptedType = "AxinomEncryptedEnvelope";
        private const string JoseSignedType = "AxinomSignedEnvelope";
        private const string Sha1RsaOid = "1.3.14.3.2.29";
        private const int MinimumRsaKeySizeInBits = 2048;

        private static void VerifyCertificateIsSaneAndUsable(X509Certificate2 certificate)
        {
            if (certificate.SignatureAlgorithm.Value == Sha1RsaOid)
                throw new ArgumentException("Weak certificates (signed using SHA-1) cannot be used with this library.");

            using (var rsaKey = certificate.GetRSAPublicKey())
            {
                if (rsaKey == null)
                    throw new ArgumentException("Only RSA keys are currently supported for this operation.");

                if (rsaKey.KeySize < MinimumRsaKeySizeInBits)
                    throw new ArgumentException($"The RSA key must be at least {MinimumRsaKeySizeInBits} bits long.");
            }
        }

        private static void VerifyCertificateAndPrivateKeyIsSaneAndUsable(X509Certificate2 certificate)
        {
            VerifyCertificateIsSaneAndUsable(certificate);

            if (!certificate.HasPrivateKey)
                throw new ArgumentException("The private key associated with the supplied certificate is not available.");
        }
    }
}
