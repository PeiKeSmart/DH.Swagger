https://mp.weixin.qq.com/s/CAd6ozTlEUprmcp_MrgMxw

URI 路径版本控制：版本号包含在 URI 路径中，如 https://api.example.com/v1/users 和 https://api.example.com/v2/users。
查询参数版本控制：版本号包含在 URL 的查询参数中，如 https://api.example.com/users?version=1 和 https://api.example.com/users?version=2。
Header 版本控制：版本号包含在 HTTP 头信息中，如 X-API-Version: 2。
内容协商（媒体类型版本控制）：使用 Accept 头信息来指定 API 的版本，如 Accept: application/json;version=1。