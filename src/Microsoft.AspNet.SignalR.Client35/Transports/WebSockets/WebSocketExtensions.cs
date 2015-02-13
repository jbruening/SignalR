using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using NetworkCredential = WebSocketSharp.Net.NetworkCredential;

namespace Microsoft.AspNet.SignalR.Client.Transports.WebSockets
{
    static class WebSocketExtensions
    {
        public static async Task<MessageEventArgs> ReceiveAsync(this WebSocket webSocket, CancellationToken cancellationToken)
        {
            var completion = new TaskCompletionSource<MessageEventArgs>();
            EventHandler<MessageEventArgs> del = null;
            del = (sender, args) =>
            {
                webSocket.OnMessage -= del;
                completion.SetResult(args);
            };
            webSocket.OnMessage += del;
            var cTask = completion.Task;
            var cancelTask = TaskEx.Delay(webSocket.WaitTime, cancellationToken);
            await TaskEx.WhenAny(cancelTask, cTask);
            return cTask.IsCompleted ? cTask.Result : null;
        }

        public static async Task CloseTaskAsync(this WebSocket socket, CloseStatusCode code, string reason)
        {
            var completion = new TaskCompletionSource<CloseEventArgs>();
            EventHandler<CloseEventArgs> del = null;
            del = (sender, args) =>
            {
                socket.OnClose -= del;
                completion.SetResult(args);
            };
            socket.OnClose += del;
            socket.CloseAsync(code, "");
            await completion.Task;
        }

        public static ICredentials ToICredentials(this NetworkCredential credential)
        {
            return new System.Net.NetworkCredential
            {
                Domain = credential.Domain,
                Password = credential.Password,
                UserName = credential.UserName
            };
        }

        public static void SetCredentials(this WebSocket webSocket, ICredentials credentials)
        {
            //todo
            var credential = credentials.GetCredential(webSocket.Url, "");
            webSocket.SetCredentials(credential.UserName, credential.Password, false);
        }

        public static CookieContainer ToContainer(this IEnumerable<Cookie> cookies)
        {
            var container = new CookieContainer();
            foreach(var cookie in cookies)
                container.Add(cookie);
            return container;
        }

        public static List<Cookie> List(this CookieContainer container)
        {
            var cookies = new List<Cookie>();

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    container,
                                                                    new object[] { });

            foreach (var key in table.Keys)
            {

                Uri uri = null;

                var domain = key as string;

                if (domain == null)
                    continue;

                if (domain.StartsWith("."))
                    domain = domain.Substring(1);

                var address = string.Format("http://{0}/", domain);

                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out uri) == false)
                    continue;

                foreach (Cookie cookie in container.GetCookies(uri))
                {
                    cookies.Add(cookie);
                }
            }
            return cookies;
        }
    }
}
