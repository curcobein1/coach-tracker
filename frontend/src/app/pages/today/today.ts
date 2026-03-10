import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { ExerciseStatsStore } from '../../core/services/exercise-stats.store';
import { LogsStore } from '../../core/services/logs.store';
import { ExerciseAnalyticsService } from '../../core/services/exercise-analytics.service';
import { NutritionStore } from '../../core/services/nutrition.store';
import { DayNutritionLog, LoggedFood } from '../../core/models/nutrition.models';
import { todayKey } from '../../core/utils/training-math';
import { parseKgInput } from '../../core/utils/weight-parser';
import {SHARED_IMPORTS} from '../../shared/shared-imports';
import { WorkoutsApiService } from '../../core/services/workouts-api.service';

@Component({
  imports: SHARED_IMPORTS,
  selector: 'app-today',
  standalone: true,
  templateUrl: './today.html',
  styleUrls: ['./today.scss'],
})
export class TodayComponent implements OnInit, OnDestroy {
  date = todayKey();
  exerciseName = '';
  kg = ''; // string input
  kgError: string | null = null;
  barKg = 20;

  reps = 0;
  rir: number | null = 2;

  // diet for today
  dietDay: DayNutritionLog | null = null;
  dietTotals = { grams: 0, kcal: 0, p: 0, c: 0, f: 0 };

  // background animation (UnicornStudio) wiring
  private usEmbedScriptEl: HTMLScriptElement | null = null;
  private usStyleEl: HTMLStyleElement | null = null;
  private usBrandingInterval: number | null = null;

  constructor(
    private workoutsApi: WorkoutsApiService,
    public stats: ExerciseStatsStore,
    private logs: LogsStore,
    public analytics: ExerciseAnalyticsService,
    private nutrition: NutritionStore
  ) {
    this.analytics.weeklyUpdateIfNeeded(new Date());
  }

  ngOnInit(): void {
    this.refreshDiet();
    document.addEventListener('nutrition:day-updated', this.onNutritionUpdate);
    this.initBackgroundAnimation();
    this.workoutsApi.getTodayWorkout().subscribe((data: any) => {
          console.log(data);
          });
  }

  private refreshDiet(): void {
    this.dietDay = this.nutrition.getDay(this.date);
    const foods = this.dietDay?.foods ?? [];
    this.dietTotals = foods.reduce(
      (acc, f) => ({
        grams: acc.grams + f.grams,
        kcal: acc.kcal + f.kcal,
        p: acc.p + f.p,
        c: acc.c + f.c,
        f: acc.f + f.f,
      }),
      { grams: 0, kcal: 0, p: 0, c: 0, f: 0 }
    );
  }

  private onNutritionUpdate = (e: Event) => {
    const anyEvent = e as CustomEvent<{ date: string }>;
    if (!anyEvent.detail || anyEvent.detail.date !== this.date) return;
    this.refreshDiet();
  };

  ngOnDestroy(): void {
    document.removeEventListener('nutrition:day-updated', this.onNutritionUpdate);
    this.teardownBackgroundAnimation();
  }

  deleteFood(f: LoggedFood): void {
    this.nutrition.removeFood(this.date, f.at);
  }

  onKgInput(v: string): void {
    let s = '';
    for (const ch of v ?? '') {
      if (ch.charCodeAt(0) <= 127) s += ch;
    }
    s = s.replace(/[^0-9.+ a-zA-Z]/g, '');
    s = s.toLowerCase();
    s = s.replace(/\s+/g, ' ');

    this.kg = s;
    this.kgError = null;
  }

  logSet(): void {
    const name = this.exerciseName.trim();
    if (!name || !this.reps) return;

    const parsed = parseKgInput(this.kg, this.barKg);
    if (!parsed.ok) {
      this.kgError = parsed.error ?? 'Invalid weight';
      return;
    }
    this.kgError = null;

    this.logs.addSet(this.date, name, {
      kg: parsed.kg!,
      reps: Number(this.reps),
      rir: this.rir === null ? undefined : Number(this.rir),
      createdAt: new Date().toISOString(),
    });

    this.reps = 0;
  }

  snap(name: string) {
    return this.analytics.snapshot(name, new Date());
  }

  private initBackgroundAnimation(): void {
    // Inject UnicornStudio loader script (idempotent)
    const embedScript = document.createElement('script');
    embedScript.type = 'text/javascript';
    embedScript.textContent = `
      !function(){
        if(!window.UnicornStudio){
          window.UnicornStudio={isInitialized:!1};
          var i=document.createElement("script");
          i.src="https://cdn.jsdelivr.net/gh/hiunicornstudio/unicornstudio.js@v1.4.33/dist/unicornStudio.umd.js";
          i.onload=function(){
            window.UnicornStudio.isInitialized||(UnicornStudio.init(),window.UnicornStudio.isInitialized=!0)
          };
          (document.head || document.body).appendChild(i)
        }
      }();
    `;
    document.head.appendChild(embedScript);
    this.usEmbedScriptEl = embedScript;

    // CSS to crop canvas + hide branding
    const style = document.createElement('style');
    style.textContent = `
      [data-us-project] {
        position: relative !important;
        overflow: hidden !important;
      }

      [data-us-project] canvas {
        clip-path: inset(0 0 10% 0) !important;
      }

      [data-us-project] * {
        pointer-events: none !important;
      }

      [data-us-project] a[href*="unicorn"],
      [data-us-project] button[title*="unicorn"],
      [data-us-project] div[title*="Made with"],
      [data-us-project] .unicorn-brand,
      [data-us-project] [class*="brand"],
      [data-us-project] [class*="credit"],
      [data-us-project] [class*="watermark"] {
        display: none !important;
        visibility: hidden !important;
        opacity: 0 !important;
        position: absolute !important;
        left: -9999px !important;
        top: -9999px !important;
      }
    `;
    document.head.appendChild(style);
    this.usStyleEl = style;

    const hideBranding = () => {
      const selectors = [
        '[data-us-project]',
        '[data-us-project="OMzqyUv6M3kSnv0JeAtC"]',
        '.unicorn-studio-container',
        'canvas[aria-label*="Unicorn"]',
      ];

      selectors.forEach((selector) => {
        const containers = document.querySelectorAll(selector);
        containers.forEach((container) => {
          const allElements = container.querySelectorAll('*');
          allElements.forEach((el) => {
            const text = (el.textContent || '').toLowerCase();
            const title = (el.getAttribute('title') || '').toLowerCase();
            const href = (el.getAttribute('href') || '').toLowerCase();

            if (
              text.includes('made with') ||
              text.includes('unicorn') ||
              title.includes('made with') ||
              title.includes('unicorn') ||
              href.includes('unicorn.studio')
            ) {
              (el as HTMLElement).style.display = 'none';
              (el as HTMLElement).style.visibility = 'hidden';
              (el as HTMLElement).style.opacity = '0';
              (el as HTMLElement).style.pointerEvents = 'none';
              (el as HTMLElement).style.position = 'absolute';
              (el as HTMLElement).style.left = '-9999px';
              (el as HTMLElement).style.top = '-9999px';
              try {
                el.remove();
              } catch {
                // ignore
              }
            }
          });
        });
      });
    };

    hideBranding();
    this.usBrandingInterval = window.setInterval(hideBranding, 50);
    window.setTimeout(hideBranding, 500);
    window.setTimeout(hideBranding, 1000);
    window.setTimeout(hideBranding, 2000);
    window.setTimeout(hideBranding, 5000);
    window.setTimeout(hideBranding, 10000);
  }

  private teardownBackgroundAnimation(): void {
    if (this.usBrandingInterval !== null) {
      clearInterval(this.usBrandingInterval);
      this.usBrandingInterval = null;
    }
    if (this.usEmbedScriptEl) {
      try {
        document.head.removeChild(this.usEmbedScriptEl);
      } catch {
        // ignore
      }
      this.usEmbedScriptEl = null;
    }
    if (this.usStyleEl) {
      try {
        document.head.removeChild(this.usStyleEl);
      } catch {
        // ignore
      }
      this.usStyleEl = null;
    }
  }
}



