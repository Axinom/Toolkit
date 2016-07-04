using Jose;
using Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

			Helpers.Jose.EnsureModernCryptographyIsEnabled();

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

			Helpers.Jose.EnsureModernCryptographyIsEnabled();

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

			var certificates = header["x5c"] as object[];

			if (certificates?.OfType<string>().Count() != 1)
				throw new ArgumentException("The JOSE object header did not reference exactly one X.509 certificate.");

			return new X509Certificate2(Convert.FromBase64String(certificates.OfType<string>().Single()));
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

		private static readonly object _joseJwtTouchingLock = new object();
		private static bool _joseJwtModernCryptoInitialized;

		/// <summary>
		/// Overrides jose-jwt library internals to include support for modern cryptography (RsaCng).
		/// This can be turned into a no-op if jose-jwt is eventually enhanced to do this natively.
		/// 
		/// Call this before ever touching jose-jwt, to avoid race conditions.
		/// </summary>
		public static void EnsureModernCryptographyIsEnabled(this HelpersContainerClasses.Jose container)
		{
			lock (_joseJwtTouchingLock)
			{
				if (_joseJwtModernCryptoInitialized)
					return;

				_joseJwtModernCryptoInitialized = true;

				var jwtType = typeof(JWT);
				var keyAlgField = jwtType.GetField("KeyAlgorithms", BindingFlags.Static | BindingFlags.NonPublic);
				var keyAlgorithms = (Dictionary<JweAlgorithm, IKeyManagement>)keyAlgField.GetValue(null);

				keyAlgorithms[JweAlgorithm.RSA1_5] = new BetterRsaKeyManagement(false, false);
				keyAlgorithms[JweAlgorithm.RSA_OAEP] = new BetterRsaKeyManagement(true, false);
				keyAlgorithms[JweAlgorithm.RSA_OAEP_256] = new BetterRsaKeyManagement(true, true);

				var hashAlgField = jwtType.GetField("HashAlgorithms", BindingFlags.Static | BindingFlags.NonPublic);
				var hashAlgorithms = (Dictionary<JwsAlgorithm, IJwsAlgorithm>)hashAlgField.GetValue(null);

				hashAlgorithms[JwsAlgorithm.RS256] = new BetterRsaUsingSha("SHA256");
				hashAlgorithms[JwsAlgorithm.RS384] = new BetterRsaUsingSha("SHA384");
				hashAlgorithms[JwsAlgorithm.RS512] = new BetterRsaUsingSha("SHA512");
			}
		}

		sealed class BetterRsaKeyManagement : IKeyManagement
		{
			private bool useRsaOaepPadding; //true for RSA-OAEP, false for RSA-PKCS#1 v1.5
			private bool useSha256; //true for RSA-OAEP-256

			public BetterRsaKeyManagement(bool useRsaOaepPadding, bool useSha256 = false)
			{
				this.useRsaOaepPadding = useRsaOaepPadding;
				this.useSha256 = useSha256;
			}

			public byte[] Unwrap(byte[] encryptedCek, object key, int cekSizeBits, IDictionary<string, object> header)
			{
				if (key is RSACng)
				{
					var privateKey = (RSACng)key;

					if (useRsaOaepPadding && useSha256)
						return privateKey.Decrypt(encryptedCek, RSAEncryptionPadding.OaepSHA256);
					else if (useRsaOaepPadding)
						return privateKey.Decrypt(encryptedCek, RSAEncryptionPadding.OaepSHA1);
					else
						return privateKey.Decrypt(encryptedCek, RSAEncryptionPadding.Pkcs1);
				}
				else if (key is RSACryptoServiceProvider)
				{
					var privateKey = (RSACryptoServiceProvider)key;

					if (useSha256)
						return RsaOaep.Decrypt(encryptedCek, RsaKey.New(privateKey.ExportParameters(true)), CngAlgorithm.Sha256);
					else
						return privateKey.Decrypt(encryptedCek, useRsaOaepPadding);
				}
				else
				{
					throw new ArgumentException("RSA key must be either RSACng or RSACryptoServiceProvider.");
				}
			}

			public byte[][] WrapNewKey(int cekSizeBits, object key, IDictionary<string, object> header)
			{
				var cek = Arrays.Random(cekSizeBits);

				if (key is RSACng)
				{
					var publicKey = (RSACng)key;

					if (useRsaOaepPadding && useSha256)
						return new[] { cek, publicKey.Encrypt(cek, RSAEncryptionPadding.OaepSHA256) };
					else if (useRsaOaepPadding)
						return new[] { cek, publicKey.Encrypt(cek, RSAEncryptionPadding.OaepSHA1) };
					else
						return new[] { cek, publicKey.Encrypt(cek, RSAEncryptionPadding.Pkcs1) };
				}
				else if (key is RSACryptoServiceProvider)
				{
					var publicKey = (RSACryptoServiceProvider)key;

					if (useSha256)
						return new[] { cek, RsaOaep.Encrypt(cek, RsaKey.New(publicKey.ExportParameters(false)), CngAlgorithm.Sha256) };
					else
						return new[] { cek, publicKey.Encrypt(cek, useRsaOaepPadding) };
				}
				else
				{
					throw new ArgumentException("RSA key must be either RSACng or RSACryptoServiceProvider.");
				}
			}
		}

		sealed class BetterRsaUsingSha : IJwsAlgorithm
		{
			private string hashMethod;

			public BetterRsaUsingSha(string hashMethod)
			{
				this.hashMethod = hashMethod;
			}

			public byte[] Sign(byte[] securedInput, object key)
			{
				using (var sha = HashAlgorithmInstance)
				{
					if (key is RSACng)
					{
						return ((RSACng)key).SignHash(sha.ComputeHash(securedInput), HashAlgorithmName, RSASignaturePadding.Pkcs1);
					}
					else if (key is RSACryptoServiceProvider)
					{
						var pkcs1 = new RSAPKCS1SignatureFormatter((RSACryptoServiceProvider)key);
						pkcs1.SetHashAlgorithm(hashMethod);

						return pkcs1.CreateSignature(sha.ComputeHash(securedInput));
					}
					else
					{
						throw new ArgumentException("RSA key must be either RSACng or RSACryptoServiceProvider.");
					}
				}
			}

			public bool Verify(byte[] signature, byte[] securedInput, object key)
			{
				using (var sha = HashAlgorithmInstance)
				{
					if (key is RSACng)
					{
						byte[] hash = sha.ComputeHash(securedInput);

						return ((RSACng)key).VerifyHash(hash, signature, HashAlgorithmName, RSASignaturePadding.Pkcs1);
					}
					else if (key is RSACryptoServiceProvider)
					{
						byte[] hash = sha.ComputeHash(securedInput);

						var pkcs1 = new RSAPKCS1SignatureDeformatter((RSACryptoServiceProvider)key);
						pkcs1.SetHashAlgorithm(hashMethod);

						return pkcs1.VerifySignature(hash, signature);
					}
					else
					{
						throw new ArgumentException("RSA key must be either RSACng or RSACryptoServiceProvider.");
					}
				}
			}

			private HashAlgorithmName HashAlgorithmName
			{
				get
				{
					if (hashMethod.Equals("SHA256"))
						return HashAlgorithmName.SHA256;
					if (hashMethod.Equals("SHA384"))
						return HashAlgorithmName.SHA384;
					if (hashMethod.Equals("SHA512"))
						return HashAlgorithmName.SHA512;

					throw new ArgumentException("Unsupported hashing algorithm: '{0}'", hashMethod);
				}
			}

			private HashAlgorithm HashAlgorithmInstance
			{
				get
				{
					if (hashMethod.Equals("SHA256"))
						return new SHA256Managed();
					if (hashMethod.Equals("SHA384"))
						return new SHA384Managed();
					if (hashMethod.Equals("SHA512"))
						return new SHA512Managed();

					throw new ArgumentException("Unsupported hashing algorithm: '{0}'", hashMethod);
				}
			}
		}
	}
}
