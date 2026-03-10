export type MuscleGroup = 'Back' | 'Chest' | 'Legs' | 'Shoulders' | 'Arms' ;

export interface ExerciseDef {
  id: string;
  name: string;

  group: MuscleGroup;          // Back
  subgroup?: string;           // Upper Back / Lower Back
  tags: string[];              // ["Vertical Pull", "Lats", "Compound"]
}
