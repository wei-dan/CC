using System.ComponentModel;

public static class SingBoxConfigTemplate
{
    /// <summary>
    /// sing-box 基础配置模板（混合入站 + 直连/Shadowsocks 出口）
    /// </summary>
    public const string BasicMixedTemplate = """
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
          "server": "120.236.197.17-ccc-mmm.mmy220.top",
          "server_port": 28080,
          "uuid": "e9efd4d0-0c6f-398d-838d-ce34ee92d3b1",
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
              "v2ray.exe",
              "xray.exe",
              "mihomo-windows-amd64-v1.exe",
              "mihomo-windows-amd64-compatible.exe",
              "mihomo-windows-amd64.exe",
              "mihomo-windows-arm64.exe",
              "clash.exe",
              "mihomo.exe",
              "hysteria.exe",
              "naive.exe",
              "naiveproxy.exe",
              "tuic-client.exe",
              "tuic.exe",
              "juicity-client.exe",
              "juicity.exe",
              "hysteria-windows-amd64.exe",
              "hysteria-linux-amd64.exe",
              "brook_windows_amd64.exe",
              "brook_linux_amd64.exe",
              "brook.exe",
              "overtls-bin.exe",
              "overtls.exe",
              "shadowquic.exe",
              "mieru.exe"
            ],
            "action": "hijack-dns"
          },
          {
            "outbound": "direct",
            "process_name": [
              "v2ray.exe",
              "xray.exe",
              "mihomo-windows-amd64-v1.exe",
              "mihomo-windows-amd64-compatible.exe",
              "mihomo-windows-amd64.exe",
              "mihomo-windows-arm64.exe",
              "clash.exe",
              "mihomo.exe",
              "hysteria.exe",
              "naive.exe",
              "naiveproxy.exe",
              "tuic-client.exe",
              "tuic.exe",
              "sing-box-client.exe",
              "sing-box.exe",
              "juicity-client.exe",
              "juicity.exe",
              "hysteria-windows-amd64.exe",
              "hysteria-linux-amd64.exe",
              "brook_windows_amd64.exe",
              "brook_linux_amd64.exe",
              "brook.exe",
              "overtls-bin.exe",
              "overtls.exe",
              "shadowquic.exe",
              "mieru.exe"
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
            "type": "local",
            "format": "binary",
            "path": "C:\\Users\\Ying Tao\\v2rayN-windows-64\\bin\\srss\\geosite-google.srs"
          },
          {
            "tag": "geosite-private",
            "type": "local",
            "format": "binary",
            "path": "C:\\Users\\Ying Tao\\v2rayN-windows-64\\bin\\srss\\geosite-private.srs"
          },
          {
            "tag": "geosite-cn",
            "type": "local",
            "format": "binary",
            "path": "C:\\Users\\Ying Tao\\v2rayN-windows-64\\bin\\srss\\geosite-cn.srs"
          },
          {
            "tag": "geoip-cn",
            "type": "local",
            "format": "binary",
            "path": "C:\\Users\\Ying Tao\\v2rayN-windows-64\\bin\\srss\\geoip-cn.srs"
          }
        ],
        "final": "proxy"
      },
      "experimental": {
        "cache_file": {
          "enabled": true,
          "path": "C:\\Users\\Ying Tao\\v2rayN-windows-64\\bin\\cache.db",
          "store_fakeip": false
        },
        "clash_api": {
          "external_controller": "127.0.0.1:10814"
        }
      }
    }
    """;

    [Description("获取 sing-box 基础配置模板")]
    public static string GetBasicMixedTemplate() => BasicMixedTemplate;
}
