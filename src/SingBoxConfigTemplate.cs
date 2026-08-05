using System.ComponentModel;

public static class SingBoxConfigTemplate
{
    /// <summary>
    /// sing-box 基础配置模板（混合入站 + 直连/Shadowsocks 出口）
    /// </summary>
    public const string BasicMixedTemplate = """
    {
      "log": {
        "level": "info",
        "timestamp": true
      },
      "dns": {
        "servers": [
          {
            "address": "https://1.1.1.1/dns-query"
          }
        ]
      },
      "inbounds": [
        {
          "type": "mixed",
          "tag": "mixed-in",
          "listen": "127.0.0.1",
          "listen_port": 1080
        }
      ],
      "outbounds": [
        {
          "type": "direct",
          "tag": "direct"
        },
        {
          "type": "shadowsocks",
          "tag": "ss-proxy",
          "server": "your-server.example.com",
          "server_port": 8388,
          "method": "2022-blake3-aes-128-gcm",
          "password": "your-password"
        },
        {
          "type": "selector",
          "tag": "select",
          "outbounds": ["direct", "ss-proxy"]
        }
      ],
      "route": {
        "rules": [
          {
            "outbound": "direct",
            "ip_is_private": true
          }
        ],
        "final": "select"
      }
    }
    """;

    [Description("获取 sing-box 基础配置模板")]
    public static string GetBasicMixedTemplate() => BasicMixedTemplate;
}
