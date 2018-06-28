using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Axinom.Toolkit
{
    /// <summary>
    /// Represents a host[:authority][:port] format address for gRPC service addressing.
    /// Authority defaults to host if absent. Port defaults to 82 if absent.
    /// </summary>
    public sealed class GrpcAddress : IEquatable<GrpcAddress>
    {
        private const int DefaultPort = 82;

        public string Host { get; }
        public string Authority { get; }
        public int Port { get; }

        public string HostAndPort => Host + ":" + Port;

        public GrpcAddress(string host, int port) : this(host, host, port)
        {
        }

        public GrpcAddress(string host, string authority, int port)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host must be specified in gRPC address.");

            Helpers.Argument.ValidateRange(port, nameof(port), min: 1, max: ushort.MaxValue);

            if (string.IsNullOrWhiteSpace(authority))
                authority = host;

            Host = host;
            Authority = authority;
            Port = port;
        }

        private const string BadAddressFormatMessage = "Not a valid gRPC address of the form host[:authority][:port].";

        public static GrpcAddress Parse(string address)
        {
            var components = address.Split(':');

            if (components.Length > 3)
                throw new ArgumentException(BadAddressFormatMessage, nameof(address));

            // Last one may be port.
            bool hasPort = ushort.TryParse(components.Last(), out var port);

            // Port defaults to 82.
            if (!hasPort)
                port = 82;

            // If there is no port, we can only have max 2 parts.
            if (!hasPort && components.Length > 2)
                throw new ArgumentException(BadAddressFormatMessage, nameof(address));

            // First part is always host.
            if (string.IsNullOrWhiteSpace(components[0]))
                throw new ArgumentException(BadAddressFormatMessage, nameof(address));

            // Authority defaults to host.
            string authority = components[0];

            // If there is an explicit authority, find it and use it.
            if (components.Length == 3)
            {
                // host:authority:port
                authority = components[1];
            }
            else if (components.Length == 2 && !hasPort)
            {
                // host:authority
                authority = components[1];
            }

            if (string.IsNullOrWhiteSpace(authority))
                throw new ArgumentException(BadAddressFormatMessage, nameof(address));

            return new GrpcAddress(components[0], authority, port);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Host);

            if (Host != Authority)
            {
                sb.Append(':');
                sb.Append(Authority);
            }

            if (Port != DefaultPort)
            {
                sb.Append(':');
                sb.Append(Port);
            }

            return sb.ToString();
        }

        #region Operators
        public override bool Equals(object obj)
        {
            return Equals(obj as GrpcAddress);
        }

        public bool Equals(GrpcAddress other)
        {
            return other != null &&
                   Host == other.Host &&
                   Authority == other.Authority &&
                   Port == other.Port;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 1096767131;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Host);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Authority);
                hashCode = hashCode * -1521134295 + Port.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(GrpcAddress address1, GrpcAddress address2)
        {
            return EqualityComparer<GrpcAddress>.Default.Equals(address1, address2);
        }

        public static bool operator !=(GrpcAddress address1, GrpcAddress address2)
        {
            return !(address1 == address2);
        }
        #endregion
    }
}
