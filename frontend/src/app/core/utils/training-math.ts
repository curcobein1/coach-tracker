export function epleyE1RM(kg: number, reps: number): number {
  if (kg <= 0 || reps <= 0) return 0;
  return kg * (1 + reps / 30);
}

export function round1(x: number): number {
  return Math.round(x * 10) / 10;
}

export function todayKey(d = new Date()): string {
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  const dd = String(d.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
}

/** 1 = Monday, 7 = Sunday. Use this for plan days and today so Plan and Today stay in sync. */
export function getDayOfWeek(d = new Date()): number {
  const jsDay = d.getDay(); // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
  return jsDay === 0 ? 7 : jsDay;
}
