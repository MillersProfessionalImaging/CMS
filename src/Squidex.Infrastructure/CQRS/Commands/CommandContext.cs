﻿// ==========================================================================
//  CommandContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class CommandContext
    {
        private readonly ICommand command;
        private Exception exception;
        private Tuple<object> result;
        
        public ICommand Command
        {
            get { return command; }
        }

        public bool IsHandled
        {
            get { return result != null || exception != null; }
        }

        public bool IsSucceeded
        {
            get { return result != null; }
        }

        public Exception Exception
        {
            get { return exception; }
        }

        public CommandContext(ICommand command)
        {
            Guard.NotNull(command, nameof(command));

            this.command = command;
        }

        public void Succeed(object resultValue = null)
        {
            if (IsHandled)
            {
                return;
            }

            result = Tuple.Create(resultValue);
        }

        public void Fail(Exception handlerException)
        {
            if (IsHandled)
            {
                return;
            }

            exception = handlerException;
        }

        public T Result<T>()
        {
            return result != null ? (T)result.Item1 : default(T);
        }
    }
}