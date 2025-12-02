using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Web.Http;
using IQPowerContentManager.Api.Models;

namespace IQPowerContentManager.Api.Controllers
{
    /// <summary>
    /// Kontroler do pobierania informacji o serwerze i sieci
    /// </summary>
    [RoutePrefix("api/server-info")]
    public class ServerInfoController : ApiController
    {
        /// <summary>
        /// Pobiera informacje o serwerze i konfiguracji sieci (podobne do ipconfig)
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetServerInfo()
        {
            try
            {
                var serverInfo = new ServerInfo
                {
                    HostName = Environment.MachineName,
                    OperatingSystem = Environment.OSVersion.ToString(),
                    NetworkInterfaces = GetNetworkInterfaces()
                };

                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SERVER-INFO] Pobrano informacje o serwerze");

                return Ok(ApiResponse<ServerInfo>.Ok(serverInfo));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [SERVER-INFO] Błąd pobierania informacji o serwerze: {ex.Message}");
                return Ok(ApiResponse<ServerInfo>.Error(ex.Message));
            }
        }

        private List<NetworkInterfaceInfo> GetNetworkInterfaces()
        {
            var interfaces = new List<NetworkInterfaceInfo>();

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Pomiń interfejsy, które nie są aktywne lub są pętlą zwrotną
                if (ni.OperationalStatus != OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    continue;
                }

                var interfaceInfo = new NetworkInterfaceInfo
                {
                    Name = ni.Name,
                    Description = ni.Description,
                    Type = ni.NetworkInterfaceType.ToString(),
                    Status = ni.OperationalStatus.ToString(),
                    PhysicalAddress = FormatMacAddress(ni.GetPhysicalAddress()),
                    Speed = ni.Speed > 0 ? $"{ni.Speed / 1000000} Mbps" : "Unknown"
                };

                // Pobierz właściwości IP
                var ipProperties = ni.GetIPProperties();

                // Adresy IPv4
                var ipv4Addresses = ipProperties.UnicastAddresses
                    .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .ToList();

                if (ipv4Addresses.Any())
                {
                    interfaceInfo.IPv4Addresses = ipv4Addresses.Select(addr => new IPAddressInfo
                    {
                        Address = addr.Address.ToString(),
                        SubnetMask = FormatSubnetMask(addr.IPv4Mask),
                        PrefixLength = addr.PrefixLength
                    }).ToList();
                }

                // Adresy IPv6
                var ipv6Addresses = ipProperties.UnicastAddresses
                    .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    .ToList();

                if (ipv6Addresses.Any())
                {
                    interfaceInfo.IPv6Addresses = ipv6Addresses.Select(addr => new IPAddressInfo
                    {
                        Address = addr.Address.ToString(),
                        PrefixLength = addr.PrefixLength
                    }).ToList();
                }

                // Brama domyślna (IPv4)
                var defaultGateway = ipProperties.GatewayAddresses
                    .Where(gw => gw.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .FirstOrDefault();

                if (defaultGateway != null)
                {
                    interfaceInfo.DefaultGateway = defaultGateway.Address.ToString();
                }

                // Serwery DNS
                var dnsServers = ipProperties.DnsAddresses
                    .Where(dns => dns.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Select(dns => dns.ToString())
                    .ToList();

                if (dnsServers.Any())
                {
                    interfaceInfo.DnsServers = dnsServers;
                }

                // DHCP
                if (ipProperties.GetIPv4Properties() != null)
                {
                    var ipv4Props = ipProperties.GetIPv4Properties();
                    interfaceInfo.DhcpEnabled = ipv4Props.IsDhcpEnabled;
                    if (ipv4Props.IsDhcpEnabled)
                    {
                        var dhcpServer = ipProperties.DhcpServerAddresses
                            .Where(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            .FirstOrDefault();
                        if (dhcpServer != null)
                        {
                            interfaceInfo.DhcpServer = dhcpServer.ToString();
                        }
                    }
                }

                interfaces.Add(interfaceInfo);
            }

            return interfaces;
        }

        private string FormatMacAddress(PhysicalAddress address)
        {
            if (address == null)
                return "N/A";

            var bytes = address.GetAddressBytes();
            return string.Join("-", bytes.Select(b => b.ToString("X2")));
        }

        private string FormatSubnetMask(IPAddress mask)
        {
            if (mask == null)
                return "N/A";

            return mask.ToString();
        }
    }

    public class ServerInfo
    {
        public string HostName { get; set; }
        public string OperatingSystem { get; set; }
        public List<NetworkInterfaceInfo> NetworkInterfaces { get; set; }
    }

    public class NetworkInterfaceInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string PhysicalAddress { get; set; }
        public string Speed { get; set; }
        public List<IPAddressInfo> IPv4Addresses { get; set; }
        public List<IPAddressInfo> IPv6Addresses { get; set; }
        public string DefaultGateway { get; set; }
        public List<string> DnsServers { get; set; }
        public bool? DhcpEnabled { get; set; }
        public string DhcpServer { get; set; }
    }

    public class IPAddressInfo
    {
        public string Address { get; set; }
        public string SubnetMask { get; set; }
        public int? PrefixLength { get; set; }
    }
}

