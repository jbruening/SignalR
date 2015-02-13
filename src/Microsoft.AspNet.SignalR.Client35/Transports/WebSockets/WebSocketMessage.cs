// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;
using WebSocketSharp;

namespace Microsoft.AspNet.SignalR.WebSockets
{
    internal sealed class WebSocketMessage
    {
        public static readonly WebSocketMessage EmptyTextMessage = new WebSocketMessage(String.Empty, Opcode.Text);
        public static readonly WebSocketMessage EmptyBinaryMessage = new WebSocketMessage(new byte[0], Opcode.Binary);
        public static readonly WebSocketMessage CloseMessage = new WebSocketMessage(null, Opcode.Close);

        public readonly object Data;
        public readonly Opcode MessageType;

        public WebSocketMessage(object data, Opcode messageType)
        {
            Data = data;
            MessageType = messageType;
        }
    }
}
