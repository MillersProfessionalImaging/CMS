/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component } from '@angular/core';
import { FormControl } from '@angular/forms';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppsState, getCategoryTree, LocalStoreService, SchemaCategory, SchemasState, Settings, UIOptions, value$ } from '@app/shared';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
})
export class SchemasPageComponent {
    public schemasFilter = new FormControl();

    public isEmbedded = false;

    public schemas =
        this.schemasState.schemas.pipe(
            map(schemas => {
                const app = this.appsState.snapshot.selectedApp!;

                return schemas.filter(schema =>
                    schema.canReadContents &&
                    schema.isPublished &&
                    schema.type !== 'Component' &&
                    !app.roleProperties[Settings.AppProperties.HIDE_CONTENTS(schema.name)],
                );
            }));

    public categories =
        combineLatest([
            value$(this.schemasFilter),
            this.schemas,
            this.schemasState.addedCategories,
        ], (filter, schemas, categories) => {
            return getCategoryTree(schemas, categories, filter);
        });

    public isCollapsed = false;

    public get width() {
        return this.isCollapsed ? '4rem' : '16rem';
    }

    constructor(uiOptions: UIOptions,
        public readonly schemasState: SchemasState,
        private readonly appsState: AppsState,
        private readonly localStore: LocalStoreService,
    ) {
        this.isCollapsed = localStore.getBoolean(Settings.Local.SCHEMAS_COLLAPSED);
        this.isEmbedded = uiOptions.get('embedded');
    }

    public toggle() {
        this.isCollapsed = !this.isCollapsed;

        this.localStore.setBoolean(Settings.Local.SCHEMAS_COLLAPSED, this.isCollapsed);
    }

    public trackByCategory(_index: number, category: SchemaCategory) {
        return category.name;
    }
}
