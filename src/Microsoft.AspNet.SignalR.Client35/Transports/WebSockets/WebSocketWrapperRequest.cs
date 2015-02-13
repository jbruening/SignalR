// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNet.SignalR.Client.Transports.WebSockets;
using WebSocketSharp;

namespace Microsoft.AspNet.SignalR.Client.Transports
{
    internal class WebSocketWrapperRequest : IRequest
    {
        private readonly WebSocket _clientWebSocket;
        private IConnection _connection;

        public WebSocketWrapperRequest(WebSocket clientWebSocket, IConnection connection)
        {
            _clientWebSocket = clientWebSocket;
            _connection = connection;
            PrepareRequest();
        }

        public string UserAgent
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:No upstream or protected callers", Justification = "Keeping the get accessors for future use")]
        public ICredentials Credentials
        {
            get
            {
                return _clientWebSocket.Credentials.ToICredentials();
            }
            set
            {
                _clientWebSocket.SetCredentials(value);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:No upstream or protected callers", Justification = "Keeping the get accessors for future use")]
        public CookieContainer CookieContainer
        {
            get
            {
                return _clientWebSocket.Cookies.Select(c => new Cookie(c.Name, c.Value, c.Path, c.Domain)).ToContainer();
            }
            set
            {
                foreach (var cookie in value.List())
                {
                    _clientWebSocket.SetCookie(new WebSocketSharp.Net.Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:No upstream or protected callers", Justification = "Keeping the get accessors for future use")]
        public IWebProxy Proxy
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Accept
        {
            get
            {
                return null;
            }
            set
            {

            }
        }

        public void SetRequestHeaders(IDictionary<string, string> headers)
        {
            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            foreach (KeyValuePair<string, string> headerEntry in headers)
            {
                throw new NotImplementedException();
            }
        }

        public void AddClientCerts(X509CertificateCollection certificates)
        {
            if (certificates == null)
            {
                throw new ArgumentNullException("certificates");
            }

            throw new NotImplementedException();
        }

        public void Abort()
        {

        }

        /// <summary>
        /// Adds certificates, credentials, proxies and cookies to the request
        /// </summary>
        private void PrepareRequest()
        {
            if (_connection.CookieContainer != null)
            {
                CookieContainer = _connection.CookieContainer;
            }

            if (_connection.Credentials != null)
            {
                Credentials = _connection.Credentials;
            }

            if (_connection.Proxy != null)
            {
                Proxy = _connection.Proxy;
            }
        }
    }

}
