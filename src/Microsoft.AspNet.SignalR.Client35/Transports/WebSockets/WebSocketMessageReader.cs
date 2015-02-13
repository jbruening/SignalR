// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client.Transports.WebSockets;
using Microsoft.AspNet.SignalR.Infrastructure;
using WebSocketSharp;

namespace Microsoft.AspNet.SignalR.WebSockets
{
    internal static class WebSocketMessageReader
    {
        private static readonly ArraySegment<byte> _emptyArraySegment = new ArraySegment<byte>(new byte[0]);

        private static byte[] BufferSliceToByteArray(byte[] buffer, int count)
        {
            byte[] newArray = new byte[count];
            Buffer.BlockCopy(buffer, 0, newArray, 0, count);
            return newArray;
        }

        private static string BufferSliceToString(byte[] buffer, int count)
        {
            return Encoding.UTF8.GetString(buffer, 0, count);
        }

        public static async Task<WebSocketMessage> ReadMessageAsync(WebSocket webSocket, int bufferSize, int? maxMessageSize, CancellationToken disconnectToken)
        {
            WebSocketMessage message;

            // Read the first time with an empty array
            var receiveResult = await webSocket.ReceiveAsync(disconnectToken).PreserveCultureNotContext();

            if (TryGetMessage(receiveResult, null, out message))
            {
                return message;
            }

            var buffer = new byte[bufferSize];

            // Now read with the real buffer
            var arraySegment = new ArraySegment<byte>(buffer);

            receiveResult = await webSocket.ReceiveAsync(disconnectToken).PreserveCultureNotContext();

            if (TryGetMessage(receiveResult, buffer, out message))
            {
                return message;
            }
            else
            {
                // for multi-fragment messages, we need to coalesce
                ByteBuffer bytebuffer = new ByteBuffer(maxMessageSize);
                bytebuffer.Append(BufferSliceToByteArray(buffer, receiveResult.RawData.Length));
                var originalMessageType = receiveResult.Type;

                while (true)
                {
                    // loop until an error occurs or we see EOF
                    receiveResult = await webSocket.ReceiveAsync(disconnectToken).PreserveCultureNotContext();

                    if (receiveResult.Type == Opcode.Close)
                    {
                        return WebSocketMessage.CloseMessage;
                    }

                    if (receiveResult.Type != originalMessageType)
                    {
                        throw new InvalidOperationException("Incorrect message type");
                    }

                    bytebuffer.Append(BufferSliceToByteArray(buffer, receiveResult.RawData.Length));

                    if (receiveResult.Type != Opcode.Cont)
                    {
                        switch (receiveResult.Type)
                        {
                            case Opcode.Binary:
                                return new WebSocketMessage(bytebuffer.GetByteArray(), Opcode.Binary);

                            case Opcode.Text:
                                return new WebSocketMessage(bytebuffer.GetString(), Opcode.Text);

                            default:
                                throw new InvalidOperationException("Unknown message type");
                        }
                    }
                }
            }
        }

        private static bool TryGetMessage(MessageEventArgs receiveResult, byte[] buffer, out WebSocketMessage message)
        {
            message = null;

            if (receiveResult.Type == Opcode.Close)
            {
                message = WebSocketMessage.CloseMessage;
            }
            else if (receiveResult.Type != Opcode.Cont)
            {
                // we anticipate that single-fragment messages will be common, so we optimize for them
                switch (receiveResult.Type)
                {
                    case Opcode.Binary:
                        if (buffer == null)
                        {
                            message = WebSocketMessage.EmptyBinaryMessage;
                        }
                        else
                        {
                            message = new WebSocketMessage(BufferSliceToByteArray(buffer, receiveResult.RawData.Length), Opcode.Binary);
                        }
                        break;
                    case Opcode.Text:
                        if (buffer == null)
                        {
                            message = WebSocketMessage.EmptyTextMessage;
                        }
                        else
                        {
                            message = new WebSocketMessage(BufferSliceToString(buffer, receiveResult.RawData.Length), Opcode.Text);
                        }
                        break;
                    default:
                        throw new InvalidOperationException("Unknown message type");
                }
            }

            return message != null;
        }
    }
}
