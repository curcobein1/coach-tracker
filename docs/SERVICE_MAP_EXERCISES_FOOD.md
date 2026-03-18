## Overview

This document maps how the **Exercise** and **Food/Nutrition** services are structured across backend and frontend: entities, controllers/services, HTTP contracts, and where they are used in the UI.

---

## Exercise service

### Backend

- **Entity**
  - `Exercise` (`Features/Exercises/Exercise.cs`)
    - `Id` `int`
    - `Name` `string`
    - `Equipment` `string?`
    - `MovementPatternTag` `string?`
    - `PrimaryMuscle` `string?`
    - `SecondaryMuscles` `string?`
    - `DefaultPlannedSets` `int?`

- **DbContext registration**
  - `AppDbContext` exposes `DbSet<Exercise> Exercises`.
  - Exercises are also referenced by:
    - **Splits**: `SplitDayExercise` links a planned day to an `Exercise`.
    - **Workout logs**: `WorkoutExerciseLog.ExerciseId` (actual performed sets reference an exercise).

- **API controller**
  - `ExercisesController` (`Features/Exercises/ExercisesController.cs`)
  - Route base: `api/Exercises`
  - Endpoints:
    - `GET /api/Exercises`
      - Returns all exercises ordered by `Name`.
      - Type: `IEnumerable<Exercise>` (entity is returned directly).
    - `POST /api/Exercises`
      - Body: `Exercise` (entity).
      - Persists new exercise and returns `201 Created` with the created entity.

- **How Exercises are consumed**
  - **Planning (Splits/Calendar)**:
    - `SplitDayExercise` rows reference `Exercise.Id` to build templates for training days.
  - **Execution (Workouts)**:
    - `WorkoutExerciseLog.ExerciseId` and `WorkoutSetLog` capture performed sets per exercise.
  - **Analytics (ExerciseStatsStore)**:
    - Uses logs to compute per‑exercise metrics such as e1RM, trends, etc.

### Frontend

- **API service**
  - `ExercisesApiService` (`src/app/core/services/exercises-api.service.ts`)
    - `base = 'http://localhost:5106/api/Exercises'` (direct URL, not `environment.apiUrl`).
    - `list(): Observable<ExerciseDto[]>`
      - Calls `GET /api/Exercises`.
      - `ExerciseDto` (UI-facing shape):
        - `id`, `name`, `group`, `category`, `equipment?`, `tags?`, `notes?`, `primaryMuscleGroup?`, `defaultSets?`.
        - Note: this is richer than the current backend `Exercise` entity, so some properties may be unused or reserved for future refactors.

- **State / analytics**
  - `ExerciseStatsStore` (`src/app/core/services/exercise-stats.store.ts`)
    - Aggregates workout history per exercise into metrics:
      - Baseline e1RM, last session e1RM, this week average, etc.
    - Feeds **Today page** exercise cards via `stats.list()` and `stats.get(name)`.

- **UI usage**
  - **Plan page**
    - Uses `ExercisesApiService.list()` to populate exercise selectors when building splits and days.
  - **Today page**
    - Execution:
      - Local “Log set locally” form pushes into `todayWorkout.sets`.
      - `Finalize & Save Workout` sends sets through `WorkoutsApiService` with `exerciseName` and (optionally) `exerciseId`.
    - Analytics:
      - The “Exercise cards” section uses `ExerciseStatsStore` snapshots to show:
        - Current avg e1RM, baseline, last session, this‑week averages.

---

## Food / Nutrition (Food service)

### Backend

There are two related concerns:

1. **USDA Food search & details** (external API, “Food service”).
2. **Daily nutrition log & usual foods** (internal persistence).

This section focuses on the **Food service** (USDA integration) while calling out where it connects to logs.

#### USDA integration service

- **`UsdaFoodService`** (`Features/Nutrition/UsdaFoodService.cs`)
  - Dependencies:
    - `HttpClient` (named client typically configured as `UsdaFoodService`).
    - `IConfiguration` for `Usda:ApiKey`.
  - Public methods:
    - `SearchFoodsAsync(string query, int pageSize)`
      - Validates presence of API key via `Usda:ApiKey`.
      - POSTs JSON payload `{ query, pageSize }` to
        - `https://api.nal.usda.gov/fdc/v1/foods/search?api_key={apiKey}`
      - Deserializes USDA `Search` response into `UsdaSearchResponse`.
      - Maps into internal DTO:
        - `FoodSearchResponseDto`:
          - `Query`, `PageSize`, `Foods: List<FoodSearchItemDto>`.
        - Each `FoodSearchItemDto` (`FoodSearchItemDto.cs`):
          - `FdcId`, `Description`, `FoodCategory?`, `BrandName?`.
          - Macro headline values:
            - `Calories` ← nutrient `"Energy"`.
            - `Protein` ← `"Protein"`.
            - `Carbs` ← `"Carbohydrate, by difference"`.
            - `Fat` ← `"Total lipid (fat)"`.
    - `GetFoodDetailAsync(int fdcId)`
      - Validates API key.
      - GETs:
        - `https://api.nal.usda.gov/fdc/v1/food/{fdcId}?api_key={apiKey}`.
      - On `404` returns `null`.
      - Otherwise deserializes `UsdaFoodItem` and maps to `FoodDetailDto`:
        - `FoodDetailDto` (`FoodDetailDto.cs`):
          - Identity & labeling:
            - `FdcId`, `Description`, `FoodCategory?`.
          - `Summary: FoodSummaryDto`:
            - `Calories` (`"Energy"`).
            - `Protein` (`"Protein"`).
            - `Carbs` (`"Carbohydrate, by difference"`).
            - `Fat` (`"Total lipid (fat)"`).
            - `Fiber` (`"Fiber, total dietary"`).
            - `Sugar` (`"Total Sugars"`).
            - `Sodium` (`"Sodium, Na"`).
            - `Potassium` (`"Potassium, K"`).
          - `Micronutrients: FoodMicronutrientsDto`:
            - `Vitamins`, `MajorMinerals`, `TraceMinerals`: lists of `NutrientItemDto`.
          - `Other: List<NutrientItemDto>` (currently populated only when mapping is extended).
        - Mapping uses:
          - `NutrientMapper.EssentialMicronutrients` dictionary
            - Keys: full USDA nutrient names (e.g. `"Vitamin A, RAE"`, `"Calcium, Ca"`, `"Potassium, K"`).
            - Values: `NutrientMapItem` with:
              - `Key` (stable identifier),
              - `Label` (user-facing text),
              - `Group` (`"vitamins"`, `"majorMinerals"`, `"traceMinerals"`),
              - `Unit` (`mg`, `mcg`, etc.).
          - For each nutrient in the food:
            - Match by `NutrientName` or `Nutrient.Name`.
            - Route it into the proper group list on the DTO.

#### Food API controller

- **`FoodsController`** (`Features/Nutrition/FoodsController.cs`)
  - `[Route("api/[controller]")]` → `api/Foods`.
  - Endpoints:
    - `POST /api/Foods/search`
      - Body: `FoodSearchRequest` (`FoodSearchRequest.cs`):
        - `Query` (required), `PageSize` (optional).
      - Validates `Query`; returns `400` if missing.
      - Calls `_usdaFoodService.SearchFoodsAsync` and returns `FoodSearchResponseDto`.
    - `GET /api/Foods/{fdcId:int}`
      - Calls `_usdaFoodService.GetFoodDetailAsync(fdcId)`.
      - Returns `404` with message if `null`, otherwise `FoodDetailDto`.

#### Nutrition logs (brief)

- **Models & controllers**:
  - `NutritionModels.cs`:
    - `NutritionFoodLog` (per‑food entries for a day).
    - `NutritionUsual` (saved “Usual foods” with macro snapshot).
  - `NutritionLogController.cs`:
    - CRUD for daily logs.
    - Endpoints to manage usual foods (upsert/delete).

- **DbContext**
  - `AppDbContext`:
    - `DbSet<NutritionFoodLog> NutritionFoodLogs`.
    - `DbSet<NutritionUsual> NutritionUsuals`.
    - `DbSet<KeyValueEntry> KeyValues` (used elsewhere).

### Frontend

- **Food API service**
  - `FoodApiService` (`src/app/core/services/food-api.service.ts`)
    - Base URL:
      - `baseUrl = \`\${environment.apiUrl}/api/foods\`;`
    - `searchFoods(request: FoodSearchRequest)`
      - POST to `\`\${baseUrl}/search\``.
      - Returns `FoodSearchResponse`:
        - `query`, `pageSize`, `foods: FoodSearchItem[]`.
    - `getFoodDetail(fdcId: number)`
      - GET `\`\${baseUrl}/\${fdcId}\``.
      - Returns `FoodDetail`.
    - Types mirror backend DTOs:
      - `FoodSearchItem`, `FoodSummary`, `NutrientItem`, `FoodDetail`.

- **Nutrition store**
  - `NutritionStore` (`src/app/core/services/nutrition.store.ts`)
    - Holds:
      - `dayCache: Map<string, DayNutritionLog>` for per‑day diet logs.
      - `usuals: signal<UsualFood[]>` for saved usual foods.
    - Uses `NutritionApiService` (separate from `FoodApiService`) for:
      - `getDay(date)` → loads `DayNutritionLog` from backend.
      - `addFood(...)`, `deleteFood(...)` → modify daily log and refresh.
      - `getUsuals()` → hydrate `usuals` signal from backend.
      - `upsertUsual(...)`, `deleteUsual(...)` → keep usuals in sync.
    - Dispatches `nutrition:day-updated` `CustomEvent` on document when a day’s log changes; `TodayComponent` listens to refresh its diet view.

- **UI usage: Nutrition page**
  - `NutritionComponent` (`src/app/pages/nutrition/nutrition.ts` / `.html`)
    - Search flow:
      - User types query → `searchFoods()`:
        - Calls `FoodApiService.searchFoods`.
        - Displays a **results list** from `FoodSearchResponse.foods`.
        - For each result:
          - Shows description, category, and headline macros (`kcal`, `P`, `C`, `F`).
          - Allows toggling “Usual” via a star badge; star uses `NutritionStore` to upsert/delete a `NutritionUsual`.
    - Detail flow:
      - Clicking a result calls `selectFood(food)`:
        - Uses `FoodApiService.getFoodDetail(fdcId)` to load `FoodDetail`.
        - Populates:
          - Summary macros (including `Fiber`, `Sugar`, `Sodium`, `Potassium`).
          - Micronutrient sections (Vitamins, Major minerals, Trace minerals, Other).
        - User can:
          - Toggle star in the detail header to save/remove from Usuals.
          - Enter grams and click **Add to today**:
            - Scales macros from the 100g USDA basis.
            - Calls `NutritionStore.addFood(...)` to log into the current day.
    - Usuals:
      - Right-hand “Usuals” list is backed by `NutritionStore.usuals`.
      - Each usual card:
        - Shows name and stored macros at its base grams.
        - Clicking the card logs it into today via `useUsual(u)` (scales macros).
        - Star badge on the card lets you remove it from Usuals without logging it.

---

## High-level request flows

### Exercise lookup for planning

1. User opens **Plan** page.
2. Plan UI calls `ExercisesApiService.list()` (`GET /api/Exercises`).
3. Backend `ExercisesController.GetAll` reads from `AppDbContext.Exercises` and returns the list.
4. Plan UI lets the user assign exercises to split days (`SplitDayExercise`) for future workouts.

### Workout execution & analytics

1. On **Today**:
   - Planned split comes from calendar API (not detailed here).
   - User logs sets locally in `todayWorkout.sets`.
   - On finalize:
     - `TodayComponent.finalizeWorkout()` posts payload to `WorkoutsApiService`.
     - Backend persists `WorkoutSession` + `WorkoutExerciseLog` + `WorkoutSetLog`.
2. `ExerciseStatsStore` reads historical workouts and computes per‑exercise metrics.
3. Today page displays metrics cards per exercise, referencing the same exercise names used in logs.

### Food search → detail → logging → usuals

1. User opens **Nutrition** page.
2. Types a query and triggers `searchFoods()`:
   - `FoodApiService.searchFoods` → `POST /api/Foods/search`.
   - `FoodsController.Search` → `UsdaFoodService.SearchFoodsAsync`.
   - Returns `FoodSearchResponseDto` mapped into `FoodSearchResponse`.
3. User clicks a result:
   - `FoodApiService.getFoodDetail` → `GET /api/Foods/{fdcId}`.
   - `FoodsController.GetById` → `UsdaFoodService.GetFoodDetailAsync`.
   - Returns `FoodDetailDto` mapped into `FoodDetail`.
4. User enters grams and clicks **Add to today**:
   - `NutritionComponent.addSelectedToToday()` scales macros from 100g to `grams`.
   - Calls `NutritionStore.addFood(date, ...)` → `NutritionApiService.addFood`.
   - Backend `NutritionLogController` saves `NutritionFoodLog`.
   - Store refreshes the day and emits `nutrition:day-updated`.
   - Today page’s Diet card refreshes totals.
5. User toggles star for Usuals:
   - In results list or detail:
     - `NutritionStore.addOrUpdateUsual` or `removeUsual`.
     - Backend upserts/deletes `NutritionUsual`.
   - Nutrition page immediately reflects saved Usuals; Today page diet logging is independent but can re‑use those macros.

