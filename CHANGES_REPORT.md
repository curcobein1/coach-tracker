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

---

# Coach Tracker – Plan/Today Backend Wiring & Future Redesign

**Report date:** March 2026  
**Scope:** Wiring Plan/Today to the .NET backend, fixing persistence/race issues, and capturing the agreed future redesign for exercises, splits, and Today-as-gateway behavior.

> **Important:** Some sections below describe **implemented code** (already in this repo) and others describe the **agreed design for the next iteration** that is **not yet implemented**. They are clearly labeled.

---

## A. Implemented Changes (as of March 2026)

### A.1 Backend – Plan DTOs and Controller

**Files:**
- `backend/CoachTracker.api/Features/Plans/PlanDtos.cs`
- `backend/CoachTracker.api/Features/Plans/PlansController.cs`

**Goals:**
- Allow the frontend Plan page to send/receive a **human exercise name** for each planned item (not just an `ExerciseId`).
- Ensure every `TrainingPlanItem` rows in the DB has a **valid `Exercise`** behind it, so saves do not silently fail due to FK constraints.

**Key changes:**

- **`TrainingPlanItemDto` now includes `ExerciseName`:**
  - Before:
    - DTO exposed only `Id`, `ExerciseId`, `OrderIndex`, `TargetSets`, `TargetReps`, `TargetRestSeconds`, `Notes`.
  - After:
    - Added `public string ExerciseName { get; set; } = "";`.

- **`GET /api/plans/active` now returns exercise names:**
  - Includes `ThenInclude(i => i.Exercise)` when loading:
    - `TrainingPlans.Include(p => p.Days).ThenInclude(d => d.Items).ThenInclude(i => i.Exercise)`.
  - Maps each `TrainingPlanItem` to:
    - `ExerciseId = i.ExerciseId`.
    - `ExerciseName = i.Exercise?.Name ?? string.Empty`.

- **`PUT /api/plans/active` now resolves/creates `Exercise` entities:**
  - Previous behavior:
    - Deleted all existing days/items and then rebuilt them using only `ExerciseId`.
    - When the frontend had `ExerciseId = 0` (for template or free‑text exercises), EF tried to save `TrainingPlanItem` rows that pointed to a non‑existent `Exercise` → save failed.
  - New behavior:
    - For each `TrainingPlanDayDto` and `TrainingPlanItemDto`:
      - Takes `ExerciseName` (string) and `ExerciseId` (int).
      - Tries to find `Exercise`:
        - If `ExerciseId != 0`, look up by Id.
        - If not found or `ExerciseId == 0`, normalize `ExerciseName` and search by lowercase name.
        - If still not found, **creates a new `Exercise`**:
          - `Name = exerciseName`.
          - `Group = "Unknown"`.
          - `Category = "Plan"`.
      - Creates a `TrainingPlanItem` with:
        - `Exercise = exercise` (navigation property).
        - `OrderIndex`, `TargetSets`, `TargetReps`, `TargetRestSeconds`, `Notes` copied from DTO.
    - Replaces `plan.Days` with the newly built list of `TrainingPlanDay` each containing `TrainingPlanItem` entities that all have valid `Exercise` references.

**Net effect:**
- Plan saves **always** result in valid `TrainingPlanItems` with valid `ExerciseId`s.
- Template‑based and free‑text plan items no longer silently fail; the corresponding exercises are auto‑created when necessary.

---

### A.2 Frontend – Plan store and Plan API contracts

**Files:**
- `frontend/src/app/core/services/plans-api.service.ts`
- `frontend/src/app/core/services/plan.store.ts`

**Goals:**
- Surface `ExerciseName` to the frontend so the UI can show real names instead of “Exercise 3”.
- Stop sending string rep ranges into a numeric backend field (`TargetReps`) which caused model‑binding failures.

**Key changes:**

- **`TrainingPlanItemDto` (frontend) now matches backend:**
  - Added `exerciseName?: string | null;` to the interface.
  - This is used both when receiving and sending plans.

- **PlanStore.mapFromDto**:
  - When mapping `TrainingPlanItemDto` → `PlanItem`:
    - Uses `i.exerciseName` if present/nonempty; else falls back to `"Exercise {i.exerciseId}"`.
    - Keeps existing legacy fields (`sets`, `repRange`, `group`, etc.) in sync with backend fields:
      - `sets = i.targetSets`.
      - `repRange = i.targetReps ?? null`.

- **PlanStore.mapToDto**:
  - When sending the plan back to the backend:
    - Includes both:
      - `exerciseId: i.exerciseId`.
      - `exerciseName: i.exerciseName`.
    - Sets:
      - `targetSets: i.targetSets ?? i.sets`.
      - `targetReps: null` (see below).
  - **Important behavior change:**
    - Previously, we tried to send string rep ranges (e.g. `"6-10"`) into `TargetReps` (which is `int?` in the DB).
    - With `[ApiController]`, that type mismatch causes model binding to fail → the entire `UpdateActive` request returns HTTP 400 and **no plan changes are persisted**.
    - To avoid this, we now **deliberately send `TargetReps = null`** and keep the human‑readable `repRange` purely on the frontend for now.

**Net effect:**
- Plan saves now **succeed** instead of failing model binding due to `TargetReps`.
- The UI can display exercise names for plan items based on the backend `Exercise.Name` (when available).

---

### A.3 Frontend – Today planned split UI and background cleanup

**Files:**
- `frontend/src/app/pages/today/today.ts`
- `frontend/src/app/pages/today/today.html`

**Goals:**
- Make Today show a clearer “planned split” using exercise names.
- Simplify and stabilize Today by removing the heavy UnicornStudio background integration (which was not essential to core functionality and complicated lifecycle).

**Key changes:**

- **Today planned split table now shows names:**
  - In `today.html` the header changed from:
    - `"Exercise (id)"` → `"Exercise"`.
  - Each row now shows:
    - `{{ it.exerciseName || ('#' + it.exerciseId) }}`.
  - This uses the new DTO field `exerciseName` when available, and falls back to showing the numeric id if for some reason the name is missing.

- **UnicornStudio background removed from Today:**
  - In `today.ts`, the fields and methods related to background animation were removed:
    - `usEmbedScriptEl`, `usStyleEl`, `usBrandingInterval`.
    - `initBackgroundAnimation()` and `teardownBackgroundAnimation()`.
  - Corresponding calls:
    - `this.initBackgroundAnimation();` in `ngOnInit`.
    - `this.teardownBackgroundAnimation();` in `ngOnDestroy`.
    - were removed.
  - This makes Today behave as a clean, pure Angular component without external script injection / DOM hacking.

**Net effect:**
- The “Today’s planned split (from plan)” card now renders more meaningfully using exercise names.
- The Today page is simpler and less fragile (no extra background animation side‑effects).

---

### A.4 Frontend – Library removal and exercise library cleanup

**Files removed:**
- `frontend/src/app/pages/library/library.ts`
- `frontend/src/app/pages/library/library.html`
- `frontend/src/app/pages/library/library.scss`
- `frontend/src/app/core/models/exercise-library.models.ts`
- `frontend/src/app/core/services/exercise-library.store.ts`

**Context:**
- The previous “Library” page and its local‑storage backed `ExerciseLibraryStore` were an earlier experiment for ad‑hoc exercise definitions.
- With the move toward a **backend‑driven Exercises table** and plan splits, this page was redundant and could confuse the mental model (two different exercise concepts).

**Behavioral implications:**
- The navigation no longer includes a Library page.
- Exercise‑related behavior will be consolidated around:
  - Backend `Exercises` table (for the canonical library).
  - Plan/Today flows described below in the **Design** section.

---

## B. Agreed Future Redesign (Not Yet Implemented)

This section documents the **architecture we agreed to implement next** for Exercises, Splits, Plan, and Today. It is a design spec, not current behavior.

### B.1 Exercises – single master table

**Target model (backend):**

- `Exercise`
  - `Id` (PK).
  - `Name` – e.g. “Bench Press”.
  - `Equipment` – e.g. “Barbell”, “Dumbbells”, “Machine”.
  - `MovementPattern` – e.g. “horizontal_press”, “vertical_pull”, “squat”, “hinge”.
  - `PrimaryMuscleGroup` – e.g. “chest”, “back”, “quads”.
  - `SecondaryMuscleGroup` – optional.
  - `DefaultSets` – optional default number of sets.

**Spontaneous exercise creation (Today page):**
- When the user wants to log a set for an exercise that does **not** exist yet:
  - Today shows a small “New exercise” form with exactly the fields from `Exercise`.
  - On save:
    - Inserts a new `Exercise` row in the DB.
    - Uses that new `Exercise.Id` immediately in `WorkoutSets`.
  - Result: every random one‑off exercise the user adds is promoted into the **global exercise library**, available later on Plan as well.

---

### B.2 Splits and split days – grouping exercises

**Target model:**

- `Split`
  - `Id`
  - `Name` – e.g. “PPL v1”, “Upper/Lower v2”, “Arms Focus”.

- `SplitDay`
  - `Id`
  - `SplitId` (FK → `Split`).
  - `Name` – e.g. “Push A”, “Pull B”, “Legs C”.
  - `OrderIndex` – 0,1,2… order inside the split.

- `SplitDayExercise`
  - `Id`
  - `SplitDayId` (FK → `SplitDay`).
  - `ExerciseId` (FK → `Exercise`).
  - `OrderIndex` – order within the day.
  - `TargetSets` (int).
  - `TargetRepRange` (string) – e.g. “6–10”.
  - `Notes` – optional.

**Intended behavior:**
- Splits are reusable templates built from the **same master Exercises table**.
- Each split day is just a named, ordered grouping of exercises with planned sets/rep ranges.
- This replaces the current “templates hard‑coded in `plan.ts`” approach with a proper backend representation.

---

### B.3 Calendar assignment and Today gateway behavior

**Planning layer (what’s on the shelf vs what’s scheduled):**

- `PlannedCalendarDay` (new table)
  - `Date` (or `DayKey` string).
  - `SplitDayId` (FK → `SplitDay`).
  - Optionally, additional **extra planned exercises** for that date (direct exercise assignments outside the normal split).

**Runtime layer (what Today writes as “done”):**

- `WorkoutSession`
  - `Id`
  - `Date`
  - `PlannedSplitDayId` – nullable link to the `SplitDay` that was scheduled for that date, if any.

- `WorkoutSet`
  - `Id`
  - `WorkoutSessionId`
  - `ExerciseId`
  - `SetNumber`
  - `Weight`
  - `Reps`
  - `RPE`
  - `RIR`
  - `FormQuality` – enum/int for how good the form felt.

- `WorkoutNote`
  - `Id`
  - `WorkoutSessionId`
  - `Text`

**Today page as gateway:**
- On load:
  - Computes **today’s date**.
  - Looks up `PlannedCalendarDay` for that date to find `SplitDayId`.
  - Loads that split day’s exercises from `SplitDayExercise` + `Exercise`.
  - Loads or creates `WorkoutSession` for today and shows any existing `WorkoutSets`/`WorkoutNotes`.
- Logging UI:
  - A **choice control** (e.g. segmented toggle) that lets the user choose:
    - “Log exercise set”.
    - “Add note”.
  - If **exercise**:
    - The user can choose:
      - One of today’s planned exercises, or
      - Search the `Exercises` table for a spontaneous exercise.
    - Then inputs `weight`, `reps`, `RPE`, `RIR`, `formQuality`.
    - This writes a `WorkoutSet` row tied to today’s `WorkoutSession`.
  - If **note**:
    - The user types arbitrary text.
    - This writes a `WorkoutNote` row tied to today’s `WorkoutSession`.

**No assignment to past days:**
- UI and backend will both enforce that `PlannedCalendarDay` assignments are only allowed for:
  - Today and future days in the current week.
  - Past days are read‑only: they can have `WorkoutSessions` and `WorkoutSets`, but not new **plan assignments**.

---

### B.4 Plan page UI redesign (target)

**High‑level layout:**

- **Top left:** Split dropdown.
  - Source: `GET /api/splits`.
  - User picks which split they’re editing (e.g. “PPL v1”).

- **Middle column:** Split days for the selected split.
  - Source: `GET /api/splits/{id}` (returns `Split` + `SplitDays` + `SplitDayExercises`).
  - Clicking a split day (`Push A`, `Pull B`, etc.) sets `selectedSplitDayId`.

- **Right column:** Exercises for the selected split day.
  - Displays each `SplitDayExercise` with resolved `Exercise` details.
  - User can:
    - Add exercises from the **global exercise list**.
    - Remove/reorder exercises.

- **Bottom/side panel:** Calendar assignment controls.
  - Shows the current week (Mon–Sun).
  - For each weekday button:
    - Disabled if the day is in the past.
    - Enabled if the day is today or in the future.
  - Two actions:
    1. **“Apply split to week”**:
       - Cycles through this split’s `SplitDays` and writes `PlannedCalendarDay` rows for all remaining days of this week.
    2. **“Assign this day only”**:
       - Assigns the currently selected `SplitDay` to a specific future weekday (one `PlannedCalendarDay` row).

**Exercise roster in Plan:**
- In addition to split days, Plan will expose a list of **all `Exercises`**:
  - Sourced from the master `Exercises` table (including ones created on Today).
  - User can:
    - Attach any exercise to a split day.
    - Or, in future iterations, attach extra planned exercises directly to specific calendar days without them belonging to a split template.

---

## C. Summary for “Home Cursor” Instance

- **Current code in this repo (as of this commit) provides:**
  - Plan ↔ Today wiring via `PlansController`, `PlanDtos`, `PlanStore`, and a Today planned split card.
  - Backend enforcement that all plan items resolve to real `Exercises`, auto‑creating them if needed.
  - A cleaned‑up Today page without the UnicornStudio background.
  - Removal of the old Library page and its local exercise store so future work can center around a single backend‑driven `Exercises` table.

- **The design sections above specify the target model and UI behavior** we agreed to for:
  - A single master `Exercises` table.
  - Split/SplitDay/SplitDayExercise entities.
  - Calendar‑based assignment of split days to dates.
  - Today as the single gateway that turns **plans** into **logged sessions + sets + notes**.

Your home machine can safely treat this document as the **authoritative spec** for how to continue the Plan/Today/Exercises redesign, even though not all of that design is implemented yet.
