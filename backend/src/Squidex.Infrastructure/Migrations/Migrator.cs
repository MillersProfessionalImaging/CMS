﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Log;

namespace Squidex.Infrastructure.Migrations
{
    public sealed class Migrator
    {
        private readonly ISemanticLog log;
        private readonly IMigrationStatus migrationStatus;
        private readonly IMigrationPath migrationPath;

        public int LockWaitMs { get; set; } = 500;

        public Migrator(IMigrationStatus migrationStatus, IMigrationPath migrationPath, ISemanticLog log)
        {
            this.migrationStatus = migrationStatus;
            this.migrationPath = migrationPath;

            this.log = log;
        }

        public async Task MigrateAsync(
            CancellationToken ct = default)
        {
            if (!await TryLockAsync(ct))
            {
                return;
            }

            try
            {
                var version = await migrationStatus.GetVersionAsync(ct);

                while (!ct.IsCancellationRequested)
                {
                    var (newVersion, migrations) = migrationPath.GetNext(version);

                    if (migrations == null || !migrations.Any())
                    {
                        break;
                    }

                    foreach (var migration in migrations)
                    {
                        var name = migration.ToString()!;

                        log.LogInformation(w => w
                            .WriteProperty("action", "Migration")
                            .WriteProperty("status", "Started")
                            .WriteProperty("migrator", name));

                        try
                        {
                            using (log.MeasureInformation(w => w
                                .WriteProperty("action", "Migration")
                                .WriteProperty("status", "Completed")
                                .WriteProperty("migrator", name)))
                            {
                                await migration.UpdateAsync(ct);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.LogFatal(ex, w => w
                                .WriteProperty("action", "Migration")
                                .WriteProperty("status", "Failed")
                                .WriteProperty("migrator", name));

                            throw new MigrationFailedException(name, ex);
                        }
                    }

                    version = newVersion;

                    await migrationStatus.CompleteAsync(newVersion, ct);
                }
            }
            finally
            {
                await UnlockAsync();
            }
        }

        private async Task<bool> TryLockAsync(
            CancellationToken ct)
        {
            try
            {
                while (!await migrationStatus.TryLockAsync(ct))
                {
                    log.LogInformation(w => w
                        .WriteProperty("action", "Migrate")
                        .WriteProperty("mesage", $"Waiting {LockWaitMs}ms to acquire lock."));

                    await Task.Delay(LockWaitMs, ct);
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            return true;
        }

        private Task UnlockAsync()
        {
            return migrationStatus.UnlockAsync();
        }
    }
}
