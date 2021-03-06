﻿using OneAndOne.Client;
using OneAndOne.Client.RESTHelpers;
using OneAndOne.POCO.Response.Servers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OneAndOne.UnitTests
{
    public class Config
    {
        public static Client.RESTHelpers.Configuration Configuration
        {
            get
            {
                return new Client.RESTHelpers.Configuration
                {
                    ApiKey = ConfigurationManager.AppSettings["APIToken"]

                };
            }
        }

        public static void waitServerReady(string ServerId)
        {
            Thread.Sleep(5000);
            var client = OneAndOneClient.Instance(Configuration);
            var server = client.Servers.Show(ServerId);
            while (server != null && ((server.Status.State != POCO.Response.Servers.ServerState.POWERED_ON && server.Status.State != POCO.Response.Servers.ServerState.POWERED_OFF) || (server.Status.Percent != 0 && server.Status.Percent != 99)))
            {
                Thread.Sleep(10000);
                server = client.Servers.Show(ServerId);
            }
        }

        public static void waitServerTurnedOff(string ServerId)
        {
            Thread.Sleep(5000);
            var client = OneAndOneClient.Instance(Configuration);
            var server = client.Servers.Show(ServerId);
            while (server != null && (server.Status.State != POCO.Response.Servers.ServerState.POWERED_OFF || (server.Status.Percent != 0 && server.Status.Percent != 99)))
            {
                Thread.Sleep(10000);
                server = client.Servers.Show(ServerId);
            }
        }

        public static void waitPrivateNetworkReady(string pnId)
        {
            var client = OneAndOneClient.Instance(Configuration);
            var pn = client.PrivateNetworks.Show(pnId);
            while (pn.State != "ACTIVE")
            {
                Thread.Sleep(10000);
                pn = client.PrivateNetworks.Show(pnId);
            }
        }

        public static void waitSharedStorageReady(string shareStorageId)
        {
            var client = OneAndOneClient.Instance(Configuration);
            var sharedStorage = client.SharedStorages.Show(shareStorageId);
            while (sharedStorage.State == "CONFIGURING")
            {
                Thread.Sleep(2000);
                sharedStorage = client.SharedStorages.Show(sharedStorage.Id);
            }
        }

        public static void waitFirewallPolicyReady(string fwId)
        {
            var client = OneAndOneClient.Instance(Configuration);
            var fw = client.FirewallPolicies.Show(fwId);
            while (fw.State == "CONFIGURING")
            {
                Thread.Sleep(2000);
                fw = client.FirewallPolicies.Show(fw.Id);
            }
        }

        public static void waitLoadBalancerReady(string lbId)
        {
            var client = OneAndOneClient.Instance(Configuration);
            var lb = client.LoadBalancer.Show(lbId);
            while (lb.State == "CONFIGURING")
            {
                Thread.Sleep(2000);
                lb = client.LoadBalancer.Show(lb.Id);
            }
        }

        public static void waitMonitoringPolicyReady(string mpId)
        {
            var client = OneAndOneClient.Instance(Configuration);
            var mp = client.MonitoringPolicies.Show(mpId);
            while (mp.State != "ACTIVE")
            {
                Thread.Sleep(2000);
                mp = client.MonitoringPolicies.Show(mp.Id);
            }
        }

        public static void waitIpRemoved(string ipId)
        {

            try
            {
                var client = OneAndOneClient.Instance(Configuration);
                var publicIP = client.PublicIPs.Show(ipId);
                while (publicIP != null && publicIP.State == "REMOVING")
                {
                    Thread.Sleep(2000);
                    publicIP = client.PublicIPs.Show(publicIP.Id);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != "{\"type\":\"ELEMENT_NOT_FOUND\",\"message\":\"\",\"errors\":null}")
                {
                    throw ex;
                }
            }
        }

        public static ServerResponse CreateTestServer(string serverName, bool powerON=true)
        {
            var client = OneAndOneClient.Instance(Configuration);
            int vcore = 4;
            int CoresPerProcessor = 2;
            var appliances = client.ServerAppliances.Get(null, null, null, "ubuntu", null);
            POCO.Response.ServerAppliances.ServerAppliancesResponse appliance = null;
            if (appliances == null || appliances.Count() == 0)
            {
                appliance = client.ServerAppliances.Get().FirstOrDefault();
            }
            else
            {
                appliance = appliances.FirstOrDefault();
            }
            var result = client.Servers.Create(new POCO.Requests.Servers.CreateServerRequest()
            {
                ApplianceId = appliance != null ? appliance.Id : null,
                Name = serverName,
                Description = "desc",
                Hardware = new POCO.Requests.Servers.HardwareRequest()
                {
                    CoresPerProcessor = CoresPerProcessor,
                    Hdds = new List<POCO.Requests.Servers.HddRequest>()
                        {
                            {new POCO.Requests.Servers.HddRequest()
                            {
                                IsMain=true,
                                Size=20,
                            }}
                        },
                    Ram = 4,
                    Vcore = vcore
                },
                PowerOn = powerON,
            });
            return client.Servers.Show(result.Id);
        }
    }


}
