import {Injectable} from '@angular/core';
export type MuscleGroup = 'Back' | 'Chest' | 'Legs' | 'Shoulders' | 'Arms' | 'Core' ;

export type Exercise ={
    id: string;
    name: string;
    group: MuscleGroup;
    tags: string[];
    createdAt: string;
};

const KEY ='coach_tracker_exercise_library_v1';

function uid():string {
    return Math.random().toString(36).slice(2,9) + Date.now().toString(36);
}

function seed(): Exercise[]{
    const now= new Date().toISOString();
    return[
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},

        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},

        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},

        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},

        {id: uid(), name: 'Bench Press', group: 'Chest', tags: ['press_horizontal', 'compound'], createdAt: now},
    ];
}

@Injectable({providedIn: 'root'})
export class ExerciseLibraryStore {
    private items: Exercise[]=this.load();

    getAll(): Exercise[]{
        return this.items;
    }

    add(name:string, group: MuscleGroup ,tags:string[]) {
        const ex: Exercise = {
            id: uid(),
            name: name.trim(),
            group,
            tags,
            createdAt: new Date().toDateString(),
        };
        this.items =[ex, ...this.items];
        this.save();
    }

    resetSeed(){
        this.items=seed();
        this.save();
    }

    private save(){
        localStorage.setItem(KEY, JSON.stringify(this.items));
    }

    private load(): Exercise[] {
        try{
            const raw = localStorage.getItem(KEY);
            if (!raw) return seed();
            const parsed= JSON.parse(raw);
            if (!Array.isArray(parsed)) return seed();
            return parsed as Exercise[];
        }   catch {
            return seed();
        }
    }
  remove(id: string) {
    this.items = this.items.filter(x => x.id !== id);
    this.save();
  }
}
