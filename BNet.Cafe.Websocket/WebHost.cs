using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace BNet.Cafe.Websocket
{
    [Serializable]
    public class HostResponse
    {
        public int StatusCode = 200;
        public string StatusDescription = "OK";
        public List<string[]> Headers = new List<string[]>();
        public byte[] Body = new byte[0];
    }

    public class WebHost : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public HostResponse ProcessRequest(string virtualDir, string pagePath, string queryString, string httpMethod,
            NameValueCollection headers, byte[] postData, string remoteAddress, int localPort)
        {
            if (!string.IsNullOrEmpty(queryString) && queryString.StartsWith("?"))
                queryString = queryString.Substring(1);

            var result = new HostResponse();
            var request = new CustomWorkerRequest(virtualDir, pagePath, queryString, httpMethod,
                headers, postData, remoteAddress, localPort, result);

            HttpRuntime.ProcessRequest(request);

            request.Done.WaitOne(TimeSpan.FromSeconds(60));
            request.Finish();
            return result;
        }
    }

    public class CustomWorkerRequest : SimpleWorkerRequest
    {
        private readonly string _virtualDir;
        private readonly string _pagePath;
        private readonly string _queryString;
        private readonly string _httpMethod;
        private readonly string _remoteAddress;
        private readonly int _localPort;
        private readonly NameValueCollection _headers;
        private readonly byte[] _postData;
        private readonly HostResponse _result;
        private readonly MemoryStream _body = new MemoryStream();

        public readonly ManualResetEvent Done = new ManualResetEvent(false);

        public CustomWorkerRequest(string virtualDir, string pagePath, string queryString, string httpMethod,
            NameValueCollection headers, byte[] postData, string remoteAddress, int localPort,
            HostResponse result)
            : base(virtualDir, pagePath, TextWriter.Null)
        {
            _virtualDir = virtualDir.EndsWith("/") ? virtualDir : virtualDir + "/";
            if (_virtualDir == "//") _virtualDir = "/";

            _pagePath = pagePath;
            _queryString = queryString ?? "";
            _httpMethod = httpMethod ?? "GET";
            _headers = headers ?? new NameValueCollection();
            _postData = postData;
            _remoteAddress = remoteAddress ?? "127.0.0.1";
            _localPort = localPort;
            _result = result;
        }

        public void Finish()
        {
            lock (_body)
            {
                _result.Body = _body.ToArray();
            }
        }

        // ---------------- Request ----------------
        public override string GetQueryString() { return _queryString; }
        public override string GetHttpVerbName() { return _httpMethod; }

        public override string GetFilePath()
        {
            return _virtualDir == "/" ? "/" + _pagePath : _virtualDir.TrimEnd('/') + "/" + _pagePath;
        }

        public override string GetUriPath()
        {
            return GetFilePath();
        }

        public override string GetPathInfo() { return ""; }

        public override string GetRawUrl()
        {
            string path = GetFilePath();
            return path + (_queryString.Length > 0 ? "?" + _queryString : "");
        }

        public override string GetAppPath() { return _virtualDir; }
        public override string GetAppPathTranslated() { return HttpRuntime.AppDomainAppPath; }
        public override string GetProtocol() { return "http"; }
        public override string GetHttpVersion() { return "HTTP/1.1"; }
        public override string GetRemoteAddress() { return _remoteAddress; }
        public override string GetLocalAddress() { return "127.0.0.1"; }
        public override int GetLocalPort() { return _localPort; }
        public override int GetRemotePort() { return 0; }
        public override string GetServerName() { return "localhost"; }
        public override bool IsSecure() { return false; }

        public override string GetKnownRequestHeader(int index)
        {
            if (index == HeaderContentLength && _postData != null)
                return _postData.Length.ToString();
            return _headers[GetKnownRequestHeaderName(index)];
        }

        public override string GetUnknownRequestHeader(string name)
        {
            return _headers[name];
        }

        public override string[][] GetUnknownRequestHeaders()
        {
            var list = new List<string[]>();
            foreach (string key in _headers.AllKeys)
            {
                if (key == null || GetKnownRequestHeaderIndex(key) != -1) continue;
                list.Add(new[] { key, _headers[key] });
            }
            return list.ToArray();
        }

        public override byte[] GetPreloadedEntityBody() { return _postData; }
        public override bool IsEntireEntityBodyIsPreloaded() { return true; }

        // ---------------- Response (captured) ----------------
        public override void SendStatus(int statusCode, string statusDescription)
        {
            _result.StatusCode = statusCode;
            _result.StatusDescription = statusDescription;
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            _result.Headers.Add(new[] { GetKnownResponseHeaderName(index), value });
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            _result.Headers.Add(new[] { name, value });
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            lock (_body) { _body.Write(data, 0, length); }
        }

        public override void SendResponseFromMemory(IntPtr data, int length)
        {
            var buf = new byte[length];
            Marshal.Copy(data, buf, 0, length);
            lock (_body) { _body.Write(buf, 0, length); }
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            using (var fs = File.OpenRead(filename))
            {
                if (length <= 0) length = fs.Length - offset;
                fs.Seek(offset, SeekOrigin.Begin);
                var buf = new byte[length];
                int read = fs.Read(buf, 0, buf.Length);
                lock (_body) { _body.Write(buf, 0, read); }
            }
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length) { }

        public override void FlushResponse(bool finalFlush) { }

        public override void EndOfRequest()
        {
            Finish();
            Done.Set();
        }
    }
}