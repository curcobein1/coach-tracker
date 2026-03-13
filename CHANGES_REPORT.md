# Coach Tracker – Detailed Changes Report

**Report date:** March 2025  
**Scope:** Plan page fixes, Today page planned-split display, day-of-week sync, and database-backed splits.

---

## 1. Executive Summary

This report documents all code and behavior changes made to the coach-tracker project for:

1. **Plan page:** Splits (per-day exercises) are persisted to the database; a single “Save plan to backend” action writes the full week. Deleting one exercise from a day no longer causes the whole day/UI to disappear.
2. **Day selection:** The Plan page now selects days by **day of week** (1 = Monday … 7 = Sunday) instead of by database id, so selection survives after save (when the backend returns new ids).
3. **Today ↔ Plan sync:** A shared **getDayOfWeek()** helper and a guarantee of **seven plan days** (Mon–Sun) ensure that “today” is the same on both Plan and Today pages and that Today can always show the current day’s planned split.
4. **Today page:** Displays “Today’s planned split” from the active plan in the database, in addition to the existing “Today sets” (logged workout).

All changes are in the **frontend** (Angular); no backend API or database schema changes were required.

---

## 2. Files Changed

| File | Role |
|------|------|
| `frontend/src/app/core/utils/training-math.ts` | New helper `getDayOfWeek()` for consistent weekday (1–7). |
| `frontend/src/app/core/services/plan.store.ts` | Always build 7 days from API response; filler days for missing weekdays. |
| `frontend/src/app/pages/plan/plan.ts` | Selection by `dayOfWeek`; guards; use `getDayOfWeek()` for initial selection. |
| `frontend/src/app/pages/plan/plan.html` | Day buttons and “Save plan to backend” button. |
| `frontend/src/app/pages/today/today.ts` | Load and show today’s plan; use `getDayOfWeek()`. |
| `frontend/src/app/pages/today/today.html` | “Today’s planned split” card and table. |

---

## 3. Change Details by File

### 3.1 `frontend/src/app/core/utils/training-math.ts`

**Purpose:** Single source of truth for “current weekday” so Plan and Today stay in sync.

**Additions:**

- **`getDayOfWeek(d?: Date): number`**
  - Returns **1 = Monday** through **7 = Sunday** (same convention as backend `TrainingPlanDay.DayOfWeek`).
  - Implementation: `const jsDay = d.getDay(); return jsDay === 0 ? 7 : jsDay;`
  - Default argument: `new Date()` so callers can use `getDayOfWeek()` for “today”.

**Why:** Previously, Today computed “plan day” with an inline `jsDay === 0 ? 7 : jsDay` and Plan used database day ids. Using one function avoids drift and matches the backend.

---

### 3.2 `frontend/src/app/core/services/plan.store.ts`

**Purpose:** (1) Guarantee exactly 7 days so Today always has a row for the current weekday. (2) Keep mapping logic in one place.

**Changes in `mapFromDto(dto)`:**

- **Before:** `days` was exactly `dto.days` mapped to `PlanDay` (so if API returned 0 or 3 days, the plan had 0 or 3 days).
- **After:**
  - Map `dto.days` as before into a list `fromDto`.
  - Build a `Map<dayOfWeek, PlanDay>` from that.
  - Loop `dow = 1..7` and for each weekday either use the day from the API or a **filler day**: `{ id: 0, dayOfWeek: dow, name/title: dayNames[dow], items: [] }`.
  - Result: `state.days` always has length 7, with `dayOfWeek` 1–7 and correct names (Monday–Sunday).

**Filler days:**

- `id: 0` so the backend will create new rows when we send them in `mapToDto` (EF treats 0 as “new”).
- No other behavior change; `mapToDto` still sends all `state.days`, so the backend receives 7 days and can persist them.

**No changes to:**

- `addItem`, `removeItem`, `moveItem`, `updateItem`, `reset`, `reindex`, or `mapToDto` logic (only the input to `mapToDto` can now include filler days).

---

### 3.3 `frontend/src/app/pages/plan/plan.ts`

**Purpose:** Fix “delete one exercise → everything disappears” and align selection with “today” when opening the plan.

**Selection model:**

- **Before:** `selectedDayId: number` (database id of the selected day). After `saveActivePlan()`, the backend replaced all days and returned new ids, so `selectedDayId` no longer matched any day → `selectedDay` became `null` → the whole editing card was hidden.
- **After:** `selectedDayOfWeek: number` (1–7). `selectedDay` is derived as `plan.days.find(d => d.dayOfWeek === selectedDayOfWeek) ?? null`. Selection survives save because weekday is stable.

**Code changes:**

- **State:** Replaced `selectedDayId` with `selectedDayOfWeek`.
- **Import:** `import { getDayOfWeek } from '../../core/utils/training-math';`
- **Constructor:** `selectedDayOfWeek = getDayOfWeek();` (open Plan on today).
- **ngOnInit (after load):** `selectedDayOfWeek = getDayOfWeek();` so after loading from API we still default to today.
- **getter `selectedDay`:** `return this.plan.days.find((d: any) => d.dayOfWeek === this.selectedDayOfWeek) ?? null;`
- **selectDay(dayOfWeek: number):** `this.selectedDayOfWeek = dayOfWeek;`
- **addItem:** Guard `const day = this.selectedDay; if (!day) return;` and use `day.id` in `store.addItem(day.id, ...)`.
- **remove:** Guard, then `store.removeItem(day.id, itemId)`.
- **moveUp / moveDown:** Guard, then `store.moveItem(day.id, ...)`.
- **edit:** Guard, then `store.updateItem(day.id, ...)`.
- **clearDay:** Already used `selectedDay`; now uses `day.id` for each `removeItem(day.id, id)`.
- **reset:** `selectedDayOfWeek = getDayOfWeek();` instead of resetting by first day id.
- **applyTemplate:** Guard `const day = this.selectedDay; if (!day) return;` and use `day.id` in `store.addItem(day.id, ...)`.

**Result:** Any action that modifies the plan uses the current day’s id from `selectedDay`, and the UI never “loses” the day after save because selection is by weekday.

---

### 3.4 `frontend/src/app/pages/plan/plan.html`

**Purpose:** Wire selection to day-of-week and add an explicit save button.

**Changes:**

- **Day buttons:**  
  - **Before:** `[class.active]="d.id === selectedDayId"` and `(click)="selectDay(d.id)"`.  
  - **After:** `[class.active]="d.dayOfWeek === selectedDayOfWeek"` and `(click)="selectDay(d.dayOfWeek)"`.
- **Plan actions row:** Added a row with “Plan actions:”, “Clear day” button, and **“Save plan to backend”** button that calls `saveActivePlan()`. The “Clear day” button was moved into this row (no duplicate).

No other template logic changed (filter, list, add form, etc.).

---

### 3.5 `frontend/src/app/pages/today/today.ts`

**Purpose:** Show today’s planned split from the database and use the same weekday convention as Plan.

**Additions / changes:**

- **Import:** `import { getDayOfWeek, todayKey } from '../../core/utils/training-math';`
- **Property:** `todayPlanDay: TrainingPlanDayDto | null = null;` (today’s day from the active plan).
- **Constructor:** Injected `PlansApiService` as `plansApi`.
- **ngOnInit:** Added `this.loadTodayPlan();` next to existing `refreshTodayWorkout()`, etc.
- **loadTodayPlan():**
  - Replaced inline weekday calculation with `const planDayOfWeek = getDayOfWeek(new Date());`
  - `this.plansApi.getActivePlan().subscribe({ next: (plan) => { this.todayPlanDay = plan.days.find((d) => d.dayOfWeek === planDayOfWeek) ?? null; }, error: ... });`

So Today loads the active plan once and sets `todayPlanDay` to the day whose `dayOfWeek` matches today (1–7). The template shows that day’s items.

---

### 3.6 `frontend/src/app/pages/today/today.html`

**Purpose:** Display “Today’s planned split” from the active plan.

**Additions:**

- **New card** (above the “Log set” form):
  - Title: “Today’s planned split (from plan)” and the current `date`.
  - Short description: exercises and target sets/reps from the active plan in the database.
  - If no `todayPlanDay` or no items: message “No planned split found for today. Set it on the Plan page and click ‘Save plan to backend’.”
  - If there are items: a **table** with columns: #, Exercise (id), Target sets, Target reps. Rows: `*ngFor="let it of todayPlanDay!.items; let idx = index"` with `idx + 1`, `it.exerciseId`, `it.targetSets`, `it.targetReps || '-'`.
- **Removed** invalid pipe: previously `todayPlanDay!.items | orderBy:'orderIndex'` (Angular does not ship `OrderByPipe`). Replaced with `todayPlanDay!.items`; order is whatever the API returns.

**Existing “Today sets (from backend)”** (logged sets) is unchanged; it still comes from the workouts API.

---

## 4. Behavioral Summary

| Before | After |
|--------|--------|
| Plan selected day by DB id; after save, id changed → selection lost, card disappeared. | Plan selects by `dayOfWeek` (1–7); selection and card stay correct after save. |
| Deleting one exercise and saving made the whole day/UI disappear. | Deleting one exercise and saving keeps the same weekday selected and shows the updated list. |
| Today and Plan could disagree on “today” (different weekday logic). | Both use `getDayOfWeek()`; “today” is identical. |
| Plan could have &lt; 7 days → Today might have no matching day for “today’s split”. | Plan always has 7 days (Mon–Sun); Today always finds a day for `getDayOfWeek()`. |
| Today showed only logged sets. | Today shows “Today’s planned split” (from plan) and “Today sets” (from workouts). |
| orderBy pipe in Today template could cause runtime error. | orderBy removed; list order is from API. |

---

## 5. Data Flow (for reference)

- **Plan page → DB:** User edits days/items in `PlanStore`. On “Save plan to backend”, `saveActivePlan()` builds `TrainingPlanDto` from `state` and sends `PUT /api/plans/active`. Backend replaces all `TrainingPlanDays` and `TrainingPlanItems` for the active plan and returns the new plan (with new ids). Store runs `mapFromDto(saved)` so `state` is updated; selection stays correct because it’s by `dayOfWeek`.
- **Today page ← DB:** On init, `loadTodayPlan()` calls `GET /api/plans/active`, gets `plan.days`, and sets `todayPlanDay = plan.days.find(d => d.dayOfWeek === getDayOfWeek(new Date())) ?? null`. The template shows `todayPlanDay.items` (and “Today sets” still from `GET /api/workouts/today`).

No new API endpoints or DB schema; only frontend logic and UI.

---

## 6. How to Verify

1. **Plan – delete then save:** On Plan, pick a day, add several exercises, click “Save plan to backend”. Delete one exercise, click “Save plan to backend” again. The same day should stay selected and the list should show the remaining exercises (not disappear).
2. **Plan – today default:** Open Plan; the weekday that matches today should be selected by default.
3. **Today – planned split:** On Plan, add exercises to today’s weekday and click “Save plan to backend”. Open Today; the “Today’s planned split” card should list those exercises (by exercise id, target sets, target reps).
4. **Today – no plan:** Clear the plan for today’s weekday and save, or open Today on a day that has no items; “No planned split found for today” (or empty table) should appear.
5. **Weekday sync:** Change the system date (or test with different weekdays) and confirm that Plan’s default selection and Today’s “planned split” both match the same weekday.

---

## 7. Commit Pushed to GitHub

- **Commit message:**  
  `Plan/Today: persist splits to DB, fix delete selection, sync day-of-week`
- **Files in commit:**  
  `plan.store.ts`, `training-math.ts`, `plan.ts`, `plan.html`, `today.ts`, `today.html`  
- This report is included in the repo as `CHANGES_REPORT.md` (and can be exported to PDF from your editor or viewer if needed).
