# Lessons learned & steps to the new structure

## Lessons learned

1. **Don’t use older Angular (or mismatched tooling).**  
   Stick to one Angular major version across the app. If the repo is on Angular 21, keep `@angular/*` and `@angular/build` on the same major (e.g. 21.2.x). Mixing “latest” Angular with an older `@angular/build` or CLI leads to obscure build/runtime errors.

2. **Check tech alignment before adding or changing things.**  
   Before adding a feature, migration, or new package:
   - **Backend:** .NET version, EF Core version, SQLite provider – all consistent.
   - **Frontend:** Angular major, `@angular/build` vs `@angular/cli`, Node/npm – all consistent.
   - **APIs:** Frontend base URL and backend CORS match; DTOs and route names match.

3. **Keep migrations and the model snapshot in sync.**  
   After changing entities or `DbContext`, add/update a migration and ensure `AppDbContextModelSnapshot.cs` matches the current model. Run migrations (e.g. `dotnet ef database update` or apply on startup) so the DB schema matches before relying on new tables/columns.

---

## Tech checklist (verify before making structure changes)

| Layer   | What to check |
|--------|----------------|
| Backend | .NET SDK (e.g. 10), EF Core 10.x, `Microsoft.EntityFrameworkCore.Sqlite` same version. |
| Frontend | Angular 21.x everywhere (`package.json`: `@angular/*` and `@angular/build` same major). Run `npm install` (or `npm ci`) and `ng build` once to confirm. |
| Repo     | `git status` clean or known; branch matches where you want to apply changes. |

---

## Target structure (what we discussed)

- **Plan page:** Splits only – pick a split, see its days, add exercises from a list, “Assign this day to today”, “Apply split to this week”. No weekly plan by day-of-week.
- **Today page:** Today’s planned split comes from **calendar** (which split day is assigned to today’s date). Log sets, add/delete session notes. No Plans API.
- **Backend:** Splits (Split → SplitDay → SplitDayExercise), Calendar (PlannedCalendarDay: date → SplitDay), Workout notes on WorkoutSession. No TrainingPlan / TrainingPlanDay / TrainingPlanItem or Plans API.

---

## Steps to change the structure

Do these in order. After each backend step, run `dotnet build`; after frontend steps, run `ng build`. Fix any errors before moving on.

### Phase 1: Tech alignment

1. **Pin Angular versions**  
   In `frontend/package.json`, set all `@angular/*` and `@angular/build` to the same 21.2.x (e.g. `^21.2.0`). Run `npm install` (use `--legacy-peer-deps` only if needed). Run `ng build` and fix any build errors.

2. **Confirm backend**  
   In `backend/CoachTracker.api`, run `dotnet restore` and `dotnet build`. Confirm .NET and EF Core versions in the `.csproj` are consistent.

### Phase 2: Backend – new model and API

3. **Add new entities** (no removal yet)  
   - `Features/Splits/`: `Split.cs`, `SplitDay.cs`, `SplitDayExercise.cs`, `PlannedCalendarDay.cs`.  
   - `Features/Workouts/WorkoutNote.cs`.  
   - Extend `WorkoutSession` with `PlannedSplitDayId` (nullable), `PlannedSplitDay` (navigation), and `WorkoutNotes` (collection).  
   - Extend `Exercise` with optional fields if you need them (e.g. `PrimaryMuscleGroup`, `DefaultSets`).  
   - Register the new sets and relationships in `AppDbContext`.

4. **Add one migration**  
   - Add a migration that creates the new tables (Splits, SplitDays, SplitDayExercises, PlannedCalendarDays, WorkoutNotes) and adds new columns (e.g. `WorkoutSession.PlannedSplitDayId`, any new columns on Exercise/WorkoutSetLog).  
   - Do **not** drop old plan tables in this migration.  
   - Run `dotnet ef database update` (or apply on startup) and confirm the DB has the new tables/columns.  
   - Ensure `AppDbContextModelSnapshot.cs` is updated and matches the current model (EF does this when you add the migration).

5. **Add new API surface**  
   - `SplitsController`: list splits, get split by id (with days and exercises), create split, add day, add exercise to day, remove exercise from day.  
   - `CalendarController`: get plan for a date (which SplitDay is assigned), assign SplitDay to a date, optional “apply split to this week”.  
   - Extend `WorkoutsController`: `GetToday` includes `WorkoutNotes`; add `POST /notes` and `DELETE /notes/{id}`.

### Phase 3: Backend – remove old plan

6. **Second migration: drop old plan**  
   - Add a migration that drops `TrainingPlanItems`, `TrainingPlanDays`, `TrainingPlans`.  
   - Apply it. Snapshot should again match the model (no TrainingPlan* entities left in DbContext).

7. **Remove old plan code**  
   - Delete `Features/Plans/` (PlansController, TrainingPlan, TrainingPlanDay, TrainingPlanItem, DTOs).  
   - Remove their `DbSet`s and any references from `AppDbContext`.  
   - Remove or repurpose Sync if it depended on plans (e.g. delete `Features/Sync/` and SyncController if unused).

### Phase 4: Frontend – new services and Plan page

8. **New API services**  
   - `splits-api.service.ts`: list splits, get split detail, create split, add day, add/remove exercise on day.  
   - `calendar-api.service.ts`: get day assignment for a date, assign split day to date, optional apply-to-week.  
   - Extend `workouts-api.service.ts`: `addNote(text)`, `deleteNote(id)`.

9. **Plan page: splits only**  
   - Replace Plan page content with: split dropdown, list of days for selected split, add exercises from a list (from `exercises-api`), “Assign this day to today”, “Apply split to this week”.  
   - Remove dependency on `plan.store` and `plans-api.service`.  
   - Use only `splits-api` and `calendar-api` (and `exercises-api` for the exercise list).

### Phase 5: Frontend – Today and cleanup

10. **Today page: calendar + notes**  
    - Today’s planned split: load from `calendar-api.getDay(todayDate)` (not Plans API).  
    - Session notes: call `workouts-api.addNote` / `deleteNote`; show notes from `getTodayWorkout()`.  
    - Remove `PlansApiService` and any `TrainingPlanDayDto` usage from Today.

11. **Remove obsolete frontend**  
    - Delete or stop using: `plan.store.ts`, `plans-api.service.ts`, `backend-sync.service.ts` (if unused).  
    - Remove `getDayOfWeek` from `training-math.ts` if nothing uses it.  
    - Clean `app.config.ts` / `app.ts` (e.g. remove BackendSyncService if it was there).

### Phase 6: Seed and run

12. **Optional: seed data**  
    - In `Program.cs`, after `MigrateAsync()`, if `Exercises` is empty, insert a few exercises and one default Split with 2–3 SplitDays and some SplitDayExercises. So Plan has something to show without manual data.

13. **Smoke test**  
    - Backend: `dotnet run`; no migration errors; hit `/api/splits`, `/api/Exercises`, `/api/workouts/today`.  
    - Frontend: `ng serve`; open Plan (pick split, assign day to today), open Today (see planned split and notes).

---

## Summary

- **Lessons:** One Angular version; check backend/frontend/API alignment and migrations/snapshot before relying on new structure.  
- **Steps:** Align tech → add new backend entities and migration → new API (Splits, Calendar, notes) → drop old plan (migration + delete code) → frontend Splits/Calendar services and Plan page → Today from calendar + notes → remove old plan/sync from frontend → optional seed → test.

Doing it in this order keeps the app buildable and the DB consistent at each step.
