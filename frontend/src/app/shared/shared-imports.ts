import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AsciiOnlyDirective } from '../core/directives/ascii-only.directive';

import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';

export const SHARED_IMPORTS = [
  CommonModule,
  FormsModule,
  AsciiOnlyDirective,
  InputTextModule,
  ButtonModule,
  CardModule,
  TableModule
];
