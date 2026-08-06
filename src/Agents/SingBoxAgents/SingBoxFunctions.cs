using System.ComponentModel;

namespace CC.Agents.SingBoxAgents;

public static class SingBoxFunctions
{
    /// <summary>
    /// sing-box 基础配置模板（混合入站 + 直连/Shadowsocks 出口）
    /// </summary>
    private const string BasicMixedTemplate = """
    {
      "log": {
        "level": "warn",
        "timestamp": true
      },
      "dns": {
        "servers": [
          {
            "server": "223.5.5.5",
            "type": "udp",
            "tag": "local_local"
          },
          {
            "server": "cloudflare-dns.com",
            "domain_resolver": "hosts_dns",
            "path": "/dns-query",
            "type": "https",
            "tag": "remote_dns",
            "detour": "proxy"
          },
          {
            "server": "dns.alidns.com",
            "domain_resolver": "hosts_dns",
            "path": "/dns-query",
            "type": "https",
            "tag": "direct_dns"
          },
          {
            "predefined": {
              "dns.google": [
                "8.8.8.8",
                "8.8.4.4",
                "2001:4860:4860::8888",
                "2001:4860:4860::8844"
              ],
              "dns.alidns.com": [
                "223.5.5.5",
                "223.6.6.6",
                "2400:3200::1",
                "2400:3200:baba::1"
              ],
              "one.one.one.one": [
                "1.1.1.1",
                "1.0.0.1",
                "2606:4700:4700::1111",
                "2606:4700:4700::1001"
              ],
              "1dot1dot1dot1.cloudflare-dns.com": [
                "1.1.1.1",
                "1.0.0.1",
                "2606:4700:4700::1111",
                "2606:4700:4700::1001"
              ],
              "cloudflare-dns.com": [
                "104.16.249.249",
                "104.16.248.249",
                "2606:4700::6810:f8f9",
                "2606:4700::6810:f9f9"
              ],
              "dns.cloudflare.com": [
                "104.16.132.229",
                "104.16.133.229",
                "2606:4700::6810:84e5",
                "2606:4700::6810:85e5"
              ],
              "dot.pub": [
                "1.12.12.12",
                "120.53.53.53"
              ],
              "doh.pub": [
                "1.12.12.12",
                "120.53.53.53"
              ],
              "dns.quad9.net": [
                "9.9.9.9",
                "149.112.112.112",
                "2620:fe::fe",
                "2620:fe::9"
              ],
              "dns.yandex.net": [
                "77.88.8.8",
                "77.88.8.1",
                "2a02:6b8::feed:0ff",
                "2a02:6b8:0:1::feed:0ff"
              ],
              "dns.sb": [
                "185.222.222.222",
                "2a09::"
              ],
              "dns.umbrella.com": [
                "208.67.220.220",
                "208.67.222.222",
                "2620:119:35::35",
                "2620:119:53::53"
              ],
              "dns.sse.cisco.com": [
                "208.67.220.220",
                "208.67.222.222",
                "2620:119:35::35",
                "2620:119:53::53"
              ],
              "engage.cloudflareclient.com": [
                "162.159.192.1"
              ]
            },
            "type": "hosts",
            "tag": "hosts_dns"
          }
        ],
        "rules": [
          {
            "server": "hosts_dns",
            "ip_accept_any": true
          },
          {
            "server": "direct_dns",
            "domain": [
              "120.236.197.17-ccc-mmm.mmy220.top"
            ]
          },
          {
            "server": "remote_dns",
            "clash_mode": "Global"
          },
          {
            "server": "direct_dns",
            "clash_mode": "Direct"
          },
          {
            "action": "predefined",
            "rcode": "NOERROR",
            "query_type": [
              64,
              65
            ]
          },
          {
            "server": "remote_dns",
            "rule_set": [
              "geosite-google"
            ]
          },
          {
            "server": "direct_dns",
            "rule_set": [
              "geosite-private"
            ]
          },
          {
            "server": "direct_dns",
            "domain_suffix": [
              "alidns.com",
              "doh.pub",
              "dot.pub",
              "360.cn",
              "onedns.net"
            ]
          },
          {
            "server": "direct_dns",
            "rule_set": [
              "geosite-cn"
            ]
          }
        ],
        "final": "remote_dns",
        "independent_cache": true
      },
      "inbounds": [
        {
          "type": "mixed",
          "tag": "socks",
          "listen": "127.0.0.1",
          "listen_port": 10808
        },
        {
          "type": "tun",
          "tag": "tun-in",
          "interface_name": "singbox_tun",
          "address": [
            "172.18.0.1/30"
          ],
          "mtu": 9000,
          "auto_route": true,
          "strict_route": true,
          "stack": "gvisor"
        }
      ],
      "outbounds": [
        {
          "server": "{{server}}",
          "server_port": {{server_port}},
          "uuid": "{{uuid}}",
          "security": "auto",
          "alter_id": 0,
          "type": "vmess",
          "tag": "proxy"
        },
        {
          "type": "direct",
          "tag": "direct"
        }
      ],
      "endpoints": [],
      "route": {
        "default_domain_resolver": {
          "server": "direct_dns"
        },
        "auto_detect_interface": true,
        "rules": [
          {
            "port": [
              53
            ],
            "process_name": [
            
            ],
            "action": "hijack-dns"
          },
          {
            "outbound": "direct",
            "process_name": [
              
            ]
          },
          {
            "action": "sniff"
          },
          {
            "protocol": [
              "dns"
            ],
            "action": "hijack-dns"
          },
          {
            "outbound": "direct",
            "clash_mode": "Direct"
          },
          {
            "outbound": "proxy",
            "clash_mode": "Global"
          },
          {
            "network": [
              "udp"
            ],
            "port": [
              443
            ],
            "action": "reject"
          },
          {
            "outbound": "proxy",
            "rule_set": [
              "geosite-google"
            ]
          },
          {
            "outbound": "direct",
            "ip_is_private": true
          },
          {
            "outbound": "direct",
            "rule_set": [
              "geosite-private"
            ]
          },
          {
            "outbound": "direct",
            "ip_cidr": [
              "223.5.5.5",
              "223.6.6.6",
              "2400:3200::1",
              "2400:3200:baba::1",
              "119.29.29.29",
              "1.12.12.12",
              "120.53.53.53",
              "2402:4e00::",
              "2402:4e00:1::",
              "180.76.76.76",
              "2400:da00::6666",
              "114.114.114.114",
              "114.114.115.115",
              "114.114.114.119",
              "114.114.115.119",
              "114.114.114.110",
              "114.114.115.110",
              "180.184.1.1",
              "180.184.2.2",
              "101.226.4.6",
              "218.30.118.6",
              "123.125.81.6",
              "140.207.198.6",
              "1.2.4.8",
              "210.2.4.8",
              "52.80.66.66",
              "117.50.22.22",
              "2400:7fc0:849e:200::4",
              "2404:c2c0:85d8:901::4",
              "117.50.10.10",
              "52.80.52.52",
              "2400:7fc0:849e:200::8",
              "2404:c2c0:85d8:901::8",
              "117.50.60.30",
              "52.80.60.30"
            ]
          },
          {
            "outbound": "direct",
            "domain_suffix": [
              "alidns.com",
              "doh.pub",
              "dot.pub",
              "360.cn",
              "onedns.net"
            ]
          },
          {
            "outbound": "direct",
            "rule_set": [
              "geoip-cn"
            ]
          },
          {
            "outbound": "direct",
            "rule_set": [
              "geosite-cn"
            ]
          }
        ],
        "rule_set": [
          {
            "tag": "geosite-google",
            "type": "remote",
            "format": "binary",
            "url": "https://raw.githubusercontent.com/SagerNet/sing-geosite/rule-set/geosite-google.srs",
            "download_detour": "proxy"
          },
          {
            "tag": "geosite-private",
            "type": "remote",
            "format": "binary",
            "url": "https://raw.githubusercontent.com/SagerNet/sing-geosite/rule-set/geosite-private.srs",
            "download_detour": "proxy"
          },
          {
            "tag": "geosite-cn",
            "type": "remote",
            "format": "binary",
            "url": "https://raw.githubusercontent.com/SagerNet/sing-geosite/rule-set/geosite-cn.srs",
            "download_detour": "proxy"
          },
          {
            "tag": "geoip-cn",
            "type": "remote",
            "format": "binary",
            "url": "https://raw.githubusercontent.com/SagerNet/sing-geoip/rule-set/geoip-cn.srs",
            "download_detour": "proxy"
          }
        ],
        "final": "proxy"
      },
      "experimental": {
        "cache_file": {
          "enabled": true,
          "path": "{{cashPath}}/cache.db",
          "store_fakeip": false
        },
        "clash_api": {
          "external_controller": "127.0.0.1:10814"
        }
      }
    }
    """;

    [Description("生成一个基础 sing-box mixed + tun 模式配置文件。需要提供代理服务器地址、端口、UUID以及配置文件保存路径")]
    public static string GetSingBoxConfig(
        [Description("代理服务器地址，例如域名或IP地址")]string server,
        [Description("代理服务器端口")]string serverPort,
        [Description("VMess UUID")] string uuid,
        [Description("sing-box缓存文件所在目录，例如 /var/lib/sing-box 或 C:\\sing-box")] string cashPath)
    {
       return BasicMixedTemplate.Replace("{{server}}", server)
                                .Replace("{{server_port}}", serverPort)
                                .Replace("{{uuid}}", uuid)
                                .Replace("{{cashPath}}", cashPath);
    } 
}
