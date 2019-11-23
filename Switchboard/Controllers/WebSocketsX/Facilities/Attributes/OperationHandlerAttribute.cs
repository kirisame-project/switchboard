﻿using System;

namespace Switchboard.Controllers.WebSocketsX.Facilities.Attributes
{
    public class OperationHandlerAttribute : Attribute
    {
        public OperationHandlerAttribute(int opCode, Type messageType)
        {
            OperationCode = opCode;
            MessageType = messageType;
        }

        public int OperationCode { get; }

        public Type MessageType { get; }
    }
}