export type MicroTarget = { key: string; label: string; unit: string; target: number };

// Default daily targets (adult, general-purpose). Adjust as needed later.
export const DEFAULT_MICRO_TARGETS: MicroTarget[] = [
  { key: 'fiber', label: 'Fiber', unit: 'g', target: 30 },
  { key: 'sodium', label: 'Sodium', unit: 'mg', target: 2300 },
  { key: 'potassium', label: 'Potassium', unit: 'mg', target: 3400 },

  { key: 'vitamin_a', label: 'Vitamin A', unit: 'mcg', target: 900 },
  { key: 'vitamin_c', label: 'Vitamin C', unit: 'mg', target: 90 },
  { key: 'vitamin_d', label: 'Vitamin D', unit: 'mcg', target: 20 },
  { key: 'vitamin_e', label: 'Vitamin E', unit: 'mg', target: 15 },
  { key: 'vitamin_k', label: 'Vitamin K', unit: 'mcg', target: 120 },
  { key: 'thiamin', label: 'Thiamin (B1)', unit: 'mg', target: 1.2 },
  { key: 'riboflavin', label: 'Riboflavin (B2)', unit: 'mg', target: 1.3 },
  { key: 'niacin', label: 'Niacin (B3)', unit: 'mg', target: 16 },
  { key: 'pantothenic_acid', label: 'Pantothenic Acid (B5)', unit: 'mg', target: 5 },
  { key: 'vitamin_b6', label: 'Vitamin B6', unit: 'mg', target: 1.3 },
  { key: 'biotin', label: 'Biotin (B7)', unit: 'mcg', target: 30 },
  { key: 'folate', label: 'Folate (B9)', unit: 'mcg', target: 400 },
  { key: 'vitamin_b12', label: 'Vitamin B12', unit: 'mcg', target: 2.4 },

  { key: 'calcium', label: 'Calcium', unit: 'mg', target: 1000 },
  { key: 'magnesium', label: 'Magnesium', unit: 'mg', target: 400 },
  { key: 'phosphorus', label: 'Phosphorus', unit: 'mg', target: 700 },
  { key: 'iron', label: 'Iron', unit: 'mg', target: 8 },
  { key: 'zinc', label: 'Zinc', unit: 'mg', target: 11 },
  { key: 'copper', label: 'Copper', unit: 'mg', target: 0.9 },
  { key: 'manganese', label: 'Manganese', unit: 'mg', target: 2.3 },
  { key: 'iodine', label: 'Iodine', unit: 'mcg', target: 150 },
  { key: 'selenium', label: 'Selenium', unit: 'mcg', target: 55 },
  { key: 'chromium', label: 'Chromium', unit: 'mcg', target: 35 },
  { key: 'molybdenum', label: 'Molybdenum', unit: 'mcg', target: 45 },
  { key: 'fluoride', label: 'Fluoride', unit: 'mg', target: 4 },
];

