/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { FormControl, Validators } from '@angular/forms';
import { Form, ExtendedFormGroup, value$ } from '@app/framework';
import { AppLanguageDto, UpdateAppLanguageDto } from './../services/app-languages.service';

export class EditLanguageForm extends Form<ExtendedFormGroup, UpdateAppLanguageDto, AppLanguageDto> {
    public get isMaster() {
        return this.form.controls['isMaster'];
    }

    public get isOptional() {
        return this.form.controls['isOptional'];
    }

    constructor() {
        super(new ExtendedFormGroup({
            isMaster: new FormControl(false,
                Validators.nullValidator,
            ),
            isOptional: new FormControl(false,
                Validators.nullValidator,
            ),
        }));

        value$(this.isMaster)
            .subscribe(value => {
                if (value) {
                    this.isOptional.setValue(false);
                }
            });

        value$(this.isMaster)
            .subscribe(value => {
                if (value) {
                    this.isOptional.setValue(false);
                }
            });
    }
}

type AddLanguageFormType = { language: string };

export class AddLanguageForm extends Form<ExtendedFormGroup, AddLanguageFormType> {
    constructor() {
        super(new ExtendedFormGroup({
            language: new FormControl(null,
                Validators.required,
            ),
        }));
    }
}
