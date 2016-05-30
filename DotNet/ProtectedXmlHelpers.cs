using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Axinom.Toolkit
{
	public static partial class DotNetHelpers
	{
		/// <summary>
		/// Encrypts the entire XML document and digitally signs it.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="document">The document to encrypt and sign. It will be modified in-place by this method.</param>
		/// <param name="encryptFor">Certificate of the document's recipient. Must contain RSA public key for a 2048-bit key pair or larger.</param>
		/// <param name="signWith">Certificate of the document's signer. Must be linked to RSA private key at least 2048 bits in length.</param>
		/// <remarks>
		/// The XML document may be left in a corrupted state if an exception is thrown.
		/// </remarks>
		public static void EncryptAndSign(this HelpersContainerClasses.ProtectedXml container, XmlDocument document, X509Certificate2 encryptFor, X509Certificate2 signWith)
		{
			Helpers.Argument.ValidateIsNotNull(document, nameof(document));
			Helpers.Argument.ValidateIsNotNull(encryptFor, nameof(encryptFor));
			Helpers.Argument.ValidateIsNotNull(signWith, nameof(signWith));

			VerifyCertificateIsSaneAndUsable(encryptFor);
			VerifyCertificateAndPrivateKeyIsSaneAndUsable(signWith);

			RSAPKCS1SHA512SignatureDescription.Register();

			// Part 1: encrypt. Default settings are secure and nice, surprisingly.
			var encryptor = new EncryptedXml();
			var encryptedData = encryptor.Encrypt(document.DocumentElement, encryptFor);
			EncryptedXml.ReplaceElement(document.DocumentElement, encryptedData, false);

			// Part 2: sign.
			using (var signingKey = signWith.GetRSAPrivateKey())
			{
				var signedXml = new SignedXml(document)
				{
					SigningKey = signingKey
				};

				var whatToSign = new Reference
				{
					// The entire document is signed.
					Uri = "",

					// A nice strong algorithm without known weaknesses that are easily exploitable.
					DigestMethod = Sha512Algorithm
				};

				// This signature (and other signatures) are inside the signed data, so exclude them.
				whatToSign.AddTransform(new XmlDsigEnvelopedSignatureTransform());

				signedXml.AddReference(whatToSign);

				// A nice strong algorithm without known weaknesses that are easily exploitable.
				signedXml.SignedInfo.SignatureMethod = RSAPKCS1SHA512SignatureDescription.Name;

				// Canonical XML 1.0 (omit comments); I suppose it works fine, no deep thoughts about this.
				signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigCanonicalizationUrl;

				// Signer certificate must be delivered with the signature.
				signedXml.KeyInfo.AddClause(new KeyInfoX509Data(signWith));

				// Ready to sign! Let's go!
				signedXml.ComputeSignature();

				// Now stick the Signature element it generated back into the document and we are done.
				var signature = signedXml.GetXml();
				document.DocumentElement.AppendChild(document.ImportNode(signature, true));
			}
		}

		/// <summary>
		/// Decrypts an XML document encrypted using <see cref="EncryptAndSign"/> after verifying the digital signature on it.
		/// </summary>
		/// <param name="container"></param>
		/// <param name="document">The encrypted and signed document. It will be modified in-place by this method.</param>
		/// <param name="signedBy">The certificate of the document's signer will be output into this variable.</param>
		/// <param name="decryptionCertificates">The set of certificates whose key pairs may be used for decryption. The correct certificate and key pair will be selected automatically (if it is among the set).</param>
		/// <exception cref="CryptographicException">
		/// Thrown if a cryptographic operation fails (e.g. because you do not have the correct decryption key).
		/// </exception>
		/// <remarks>
		/// The XML document may be left in a corrupted state if an exception is thrown.
		/// </remarks>
		public static void VerifyAndDecrypt(this HelpersContainerClasses.ProtectedXml container, XmlDocument document, out X509Certificate2 signedBy, params X509Certificate2[] decryptionCertificates)
		{
			Helpers.Argument.ValidateIsNotNull(document, nameof(document));
			Helpers.Argument.ValidateIsNotNull(decryptionCertificates, nameof(decryptionCertificates));

			foreach (var decryptionCertificate in decryptionCertificates)
			{
				if (decryptionCertificate == null)
					throw new ArgumentException("Decryption certificate list cannot contain null values.", nameof(decryptionCertificates));

				VerifyCertificateAndPrivateKeyIsSaneAndUsable(decryptionCertificate);
			}

			RSAPKCS1SHA512SignatureDescription.Register();

			var namespaces = new XmlNamespaceManager(document.NameTable);
			namespaces.AddNamespace("ds", XmlDigitalSignatureNamespace);
			namespaces.AddNamespace("enc", XmlEncryptionNamespace);

			if (document.SelectSingleNode("/enc:EncryptedData", namespaces) == null)
				throw new ArgumentException("The document is not an encrypted XML document.", nameof(document));

			var signatureNodes = document.SelectNodes("/enc:EncryptedData/ds:Signature", namespaces);
			if (signatureNodes.Count != 1)
				throw new ArgumentException("The document not carry exactly 1 XML digital signature.", nameof(document));

			// Verify signature.
			var signatureNode = (XmlElement)signatureNodes[0];

			var signedXml = new SignedXml(document);
			signedXml.LoadXml(signatureNode);

			if (!signedXml.CheckSignature())
				throw new SecurityException("Signature failed to verify - the XML document has been tampered with!");

			var referenceUris = signedXml.SignedInfo.References.Cast<Reference>().Select(r => r.Uri).ToArray();

			// It must be a whole-document signature.
			if (referenceUris.Length != 1 || referenceUris.Single() != "")
				throw new SecurityException("The digital signature was not scoped to the entire XML document.");

			// The signature must include a certificate for the signer in order to be categorized.
			var certificateElement = signatureNode.SelectSingleNode("ds:KeyInfo/ds:X509Data/ds:X509Certificate", namespaces);

			if (certificateElement == null)
				throw new SecurityException("The digital signature did not contain the certificate of the signer.");

			signedBy = new X509Certificate2(Convert.FromBase64String(certificateElement.InnerText));

			// Decrypt.
			var decryptor = new EncryptedXmlWithCustomDecryptionCertificates(document)
			{
				DecryptionCertificates = decryptionCertificates
			};

			decryptor.DecryptDocument();
		}

		private const string Sha512Algorithm = "http://www.w3.org/2001/04/xmlenc#sha512";
		private static readonly string Sha1RsaOid = "1.3.14.3.2.29";
		private const int MinimumRsaKeySizeInBits = 2048;
		private const string XmlDigitalSignatureNamespace = "http://www.w3.org/2000/09/xmldsig#";
		private const string XmlEncryptionNamespace = "http://www.w3.org/2001/04/xmlenc#";

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

		/// <summary>
		/// EncryptedXml just loads certificates from the Windows store. Often okay but not always.
		/// </summary>
		private sealed class EncryptedXmlWithCustomDecryptionCertificates : EncryptedXml
		{
			public EncryptedXmlWithCustomDecryptionCertificates(XmlDocument doc) : base(doc)
			{
			}

			public IEnumerable<X509Certificate2> DecryptionCertificates { get; set; }

			public override byte[] DecryptEncryptedKey(EncryptedKey encryptedKey)
			{
				if (encryptedKey == null)
					throw new ArgumentNullException("encryptedKey");

				if (encryptedKey.KeyInfo == null)
					return null;

				IEnumerator keyInfoEnum = encryptedKey.KeyInfo.GetEnumerator();

				while (keyInfoEnum.MoveNext())
				{
					KeyInfoX509Data keyInfo = keyInfoEnum.Current as KeyInfoX509Data;

					if (keyInfo == null)
						continue;

					// We only support KeyInfo with exactly one embedded Certificate entry.
					if (keyInfo.Certificates.Count != 1)
						continue;

					var targetCertificate = keyInfo.Certificates[0] as X509Certificate;
					if (targetCertificate == null)
						continue;

					var targetCertificate2 = new X509Certificate2(targetCertificate);

					foreach (X509Certificate2 certificate in DecryptionCertificates)
					{
						if (certificate.Thumbprint != targetCertificate2.Thumbprint)
							continue;

						RSA privateKey = certificate.PrivateKey as RSA;

						if (privateKey == null)
							continue;

						bool fOAEP = (encryptedKey.EncryptionMethod != null && encryptedKey.EncryptionMethod.KeyAlgorithm == XmlEncRSAOAEPUrl);
						return DecryptKey(encryptedKey.CipherData.CipherValue, privateKey, fOAEP);
					}

					break;
				}

				return null;
			}
		}

		// EVERYTHING BELOW IS A HORRIBLE HACK FOR .NET 4.6 COMPATIBILITY! Delete it all when upgrading to 4.6.2 or newer!

		/// <summary>
		/// INTERNAL ONLY - DO NOT USE.
		/// </summary>
		public sealed class RSAPKCS1SHA512SignatureDescription : SignatureDescription
		{
			public const string Name = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha512";

			/// <summary>
			/// Registers the http://www.w3.org/2001/04/xmldsig-more#rsa-sha512 algorithm
			/// with the .NET CrytoConfig registry. This needs to be called once per
			/// appdomain before attempting to validate SHA512 signatures.
			/// </summary>
			public static void Register()
			{
				if (CryptoConfig.CreateFromName(Name) == null)
					CryptoConfig.AddAlgorithm(typeof(RSAPKCS1SHA512SignatureDescription), Name);
			}

			public RSAPKCS1SHA512SignatureDescription()
			{
				KeyAlgorithm = typeof(RSA).FullName;
				DigestAlgorithm = typeof(SHA512Managed).FullName;
				FormatterAlgorithm = typeof(CustomRSAPKCS1SignatureFormatter).FullName;
				DeformatterAlgorithm = typeof(RSAPKCS1SignatureDeformatter).FullName;
			}

			public override AsymmetricSignatureDeformatter CreateDeformatter(AsymmetricAlgorithm key)
			{
				var asymmetricSignatureDeformatter = new RSAPKCS1SignatureDeformatter();
				asymmetricSignatureDeformatter.SetKey(key);
				asymmetricSignatureDeformatter.SetHashAlgorithm("SHA512");
				return asymmetricSignatureDeformatter;
			}

			public override AsymmetricSignatureFormatter CreateFormatter(AsymmetricAlgorithm key)
			{
				var asymmetricSignatureFormatter = new CustomRSAPKCS1SignatureFormatter();
				asymmetricSignatureFormatter.SetKey(key);
				asymmetricSignatureFormatter.SetHashAlgorithm("SHA512");
				return asymmetricSignatureFormatter;
			}
		}

		/// <summary>
		/// INTERNAL ONLY - DO NOT USE.
		/// </summary>
		private sealed class CustomRSAPKCS1SignatureFormatter : AsymmetricSignatureFormatter
		{
			private RSACng _rsaKey;

			public CustomRSAPKCS1SignatureFormatter() { }

			public CustomRSAPKCS1SignatureFormatter(AsymmetricAlgorithm key)
			{
				if (key == null)
					throw new ArgumentNullException("key");

				_rsaKey = (RSACng)key;
			}

			public override void SetKey(AsymmetricAlgorithm key)
			{
				if (key == null)
					throw new ArgumentNullException("key");

				_rsaKey = (RSACng)key;
			}

			public override void SetHashAlgorithm(String strName)
			{
			}

			public override byte[] CreateSignature(byte[] rgbHash)
			{
				return ((RSACng)_rsaKey).SignHash(rgbHash, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
			}
		}
	}
}
