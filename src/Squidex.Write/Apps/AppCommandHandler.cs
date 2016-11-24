﻿// ==========================================================================
//  AppCommandHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Read.Apps.Repositories;
using Squidex.Read.Users.Repositories;
using Squidex.Write.Apps.Commands;

namespace Squidex.Write.Apps
{
    public class AppCommandHandler : CommandHandler<AppDomainObject>
    {
        private readonly IAppRepository appRepository;
        private readonly IUserRepository userRepository;
        private readonly ClientKeyGenerator keyGenerator;

        public AppCommandHandler(
            IDomainObjectFactory domainObjectFactory, 
            IDomainObjectRepository domainObjectRepository, 
            IUserRepository userRepository,
            IAppRepository appRepository,
            ClientKeyGenerator keyGenerator) 
            : base(domainObjectFactory, domainObjectRepository)
        {
            Guard.NotNull(keyGenerator, nameof(keyGenerator));
            Guard.NotNull(appRepository, nameof(appRepository));
            Guard.NotNull(userRepository, nameof(userRepository));

            this.keyGenerator = keyGenerator;
            this.appRepository = appRepository;
            this.userRepository = userRepository;
        }

        protected Task On(CreateApp command, CommandContext context)
        {
            return CreateAsync(command, async x =>
            {
                if (await appRepository.FindAppByNameAsync(command.Name) != null)
                {
                    var error = new ValidationError($"A app with name '{command.Name}' already exists", nameof(CreateApp.Name));

                    throw new ValidationException("Cannot create a new app", error);
                }

                x.Create(command);

                context.Succeed(command.AggregateId);
            });
        }

        protected Task On(AssignContributor command, CommandContext context)
        {
            return UpdateAsync(command, async x =>
            {
                if (await userRepository.FindUserByIdAsync(command.ContributorId) == null)
                {
                    var error = new ValidationError($"Cannot find contributor '{command.ContributorId ?? "UNKNOWN"}'", nameof(AssignContributor.ContributorId));

                    throw new ValidationException("Cannot assign contributor to app", error);
                }

                x.AssignContributor(command);
            });
        }

        protected Task On(AttachClient command, CommandContext context)
        {
            return UpdateAsync(command, x =>
            {
                var clientKey = keyGenerator.GenerateKey();

                x.AttachClient(command, clientKey);

                context.Succeed(x.Clients[command.ClientName]);
            });
        }

        protected Task On(RemoveContributor command, CommandContext context)
        {
            return UpdateAsync(command, x => x.RemoveContributor(command));
        }

        protected Task On(RevokeClient command, CommandContext context)
        {
            return UpdateAsync(command, x => x.RevokeClient(command));
        }

        protected Task On(ConfigureLanguages command, CommandContext context)
        {
            return UpdateAsync(command, x => x.ConfigureLanguages(command));
        }

        public override Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? Task.FromResult(false) : this.DispatchActionAsync(context.Command, context);
        }
    }
}
