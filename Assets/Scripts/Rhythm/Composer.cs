﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComposeScaleType {
  Minor,
  Major
}

public struct Settings
{
    public ComposeScaleType scaleType; // 1:minor / 2:major
    public int baseNote; // c2: 36 c8: 88
    public int numOctaves; // 3
}

class Composer
{
    public Scale scale;
    private Settings settings;

    public Composer(ComposeScaleType scaleType, int baseNote, int numOctaves)
    {
        settings.scaleType = scaleType;
        settings.baseNote = baseNote;
        settings.numOctaves = numOctaves;

        switch(scaleType) {
          case ComposeScaleType.Minor: {
            scale = new MinorScale(baseNote, numOctaves);
            break;
                  }
          case ComposeScaleType.Major: {
            scale = new MajorScale(baseNote, numOctaves);
            break;
                  }
          default: { Debug.Assert(false); break; }
        }
    }
}


public abstract class Scale
{

    protected Note[] notes;
    protected Chord[] chords;
    protected int[] chordProgression;
    protected int currentChordIndex;

    protected Settings settings;

    public Scale(ComposeScaleType type, int baseNote, int numOctaves)
    {
        settings.scaleType = type;
        settings.baseNote = baseNote;
        settings.numOctaves = numOctaves;
        
        currentChordIndex = 0;
    }

    public bool IsInScaleNotes(int noteNumber)
    {
        foreach (Note n in notes)
        {
            if ((noteNumber % 12) == n.number)
                return true;
        }

        return false;
    }

    public Note[] GetAllowedNotesInScale()
    {
        return Utility.ExpandNotesIntoOctaves(notes, settings.baseNote, settings.numOctaves);
    }

    public Note[] GetNextChordInChordProgression()
    {
        if (currentChordIndex + 1 >= chordProgression.Length) currentChordIndex = 0;

        //Debug.ClearDeveloperConsole();
        //Debug.Log("Current Chord ::::" + currentChordIndex + "  " + (char)(UnityEngine.Random.Range(50, 150)));

        return chords[chordProgression[currentChordIndex]].GetChordNotes(settings.baseNote + chordProgression[currentChordIndex++], settings.numOctaves);
    }

    public void GenerateNextChordProgression()
    {
        // TODO: Load from files.
        List<int[]> options = new List<int[]>();
        options.Add(new int[4] { 0, 2, 6, 4 });
        options.Add(new int[5] { 0, 2, 3, 2, 4 });

        int index = Random.Range(0, options.Count);

        //Debug.Log("Current Chord Progression ::::" + index);

        chordProgression = new int[options.ToArray()[index].Length];
        int i = 0;
        foreach (int n in options.ToArray()[index])
        {
            chordProgression[i] = n;
            i++;
        }
    }

    public Note[] GetRandomChordInScale()
    {
        int index = 0;
        float f = Random.Range(0f, 1f);
        if (f > 0.75f) index = 0;
        else if (f < 0.75 && f > 0.50) index = 3;
        else if (f < 0.50 && f > 0.35) index = 4;
        else
        {
            index = Random.Range(3, notes.Length);
            if (index == 3) index = 1;
            if (index == 4) index = 2;

        }

        return chords[index].GetChordNotes(settings.baseNote + index, settings.numOctaves);
    }

    public abstract void CalculateNotes();
    public abstract void CalculateChords();

}

public class MinorScale : Scale
{
    public MinorScale(int baseNote, int numOctaves) : base(ComposeScaleType.Minor, baseNote, numOctaves)
    {
        CalculateNotes();
        CalculateChords();
        GenerateNextChordProgression();
    }

    override public void CalculateNotes()
    {
        notes = new Note[7];
        notes[0] = new Note(0);
        notes[1] = new Note(2);
        notes[2] = new Note(3);
        notes[3] = new Note(5);
        notes[4] = new Note(7);
        notes[5] = new Note(8);
        notes[6] = new Note(10);
    }

    override public void CalculateChords()
    {
        chords = new Chord[7];
        chords[0] = new MinorChord(0, notes);
        chords[1] = new DimChord(1, notes);
        chords[2] = new MajorChord(2, notes);
        chords[3] = new MinorChord(3, notes);
        chords[4] = new MinorChord(4, notes);
        chords[5] = new MajorChord(5, notes);
        chords[6] = new MajorChord(6, notes);
    }

}

public class MajorScale : Scale
{
    public MajorScale(int baseNote, int numOctaves) : base(ComposeScaleType.Major, baseNote, numOctaves)
    {
        CalculateNotes();
        CalculateChords();
        GenerateNextChordProgression();
    }

    override public void CalculateNotes()
    {
        notes = new Note[7];
        notes[0] = new Note(0);
        notes[1] = new Note(2);
        notes[2] = new Note(4);
        notes[3] = new Note(5);
        notes[4] = new Note(7);
        notes[5] = new Note(9);
        notes[6] = new Note(11);
    }

    override public void CalculateChords()
    {
        chords = new Chord[7];
        chords[0] = new MajorChord(0, notes);
        chords[1] = new MinorChord(1, notes);
        chords[2] = new MinorChord(2, notes);
        chords[3] = new MajorChord(3, notes);
        chords[4] = new MajorChord(4, notes);
        chords[5] = new MinorChord(5, notes);
        chords[6] = new DimChord(6, notes);
    }

}

public abstract class Chord
{
    protected Note[] notes;
    protected string typeName;

    public Chord(string type, int baseNoteIndex, Note[] scale)
    {
        typeName = type;
    }

    public Note[] GetNotes()
    {
        return notes;
    }

    public bool ChordContainsNote(int noteIndex)
    {
        foreach (Note n in notes)
        {
            if (noteIndex == n.number)
                return true;
        }

        return false;
    }

    public Note[] GetChordNotes(int baseNote, int numOctaves)
    {
        return Utility.ExpandNotesIntoOctaves(notes, baseNote, numOctaves);
    }
}

public class MinorChord : Chord
{
    public MinorChord(int baseNoteIndex, Note[] scale) : base("m", baseNoteIndex, scale)
    {
        this.notes = new Note[3] { scale[(baseNoteIndex) % 7], scale[(baseNoteIndex + 2) % 7], scale[(baseNoteIndex + 4) % 7] };
    }
}


public class MajorChord : Chord
{
    public MajorChord(int baseNoteIndex, Note[] scale) : base("M", baseNoteIndex, scale)
    {
        this.notes = new Note[3] { scale[(baseNoteIndex) % 7], new Note((scale[(baseNoteIndex + 2) % 7].number - 1) % 12), scale[(baseNoteIndex + 4) % 7] };
    }
}

public class DimChord : Chord
{
    public DimChord(int baseNoteIndex, Note[] scale) : base("dim", baseNoteIndex, scale)
    {
        this.notes = new Note[3] { scale[(baseNoteIndex) % 7], scale[(baseNoteIndex + 2) % 7], scale[(baseNoteIndex + 4) % 7] };
    }
}


public class Note
{
    public int number;

    public Note(int n)
    {
        number = n;
    }

}

public static class Utility
{
    public static Note[] ExpandNotesIntoOctaves(Note[] notes, int baseNote, int numOctaves)
    {
        int size = notes.Length * numOctaves;
        Note[] result = new Note[size];
        for (int i = 0; i < size; i++)
        {
            result[i] = new Note(baseNote + notes[i % notes.Length].number + 12 * ((i / numOctaves)));
        }

        return result;
    }

    public static int NoteNameToNumber(string name)
    {
        int result = 0;

        int length = name.Length;
        int octave = int.Parse(name[length - 1].ToString());

        if (length == 3) // aka it's a sharp note (Because the name will be like As4 with the length of 3 chars.)
        {
            result = +1;
        }

        switch (name[0])
        {
            case 'c': result += 0; break;
            case 'd': result += 2; break;
            case 'e': result += 4; break;
            case 'f': result += 5; break;
            case 'g': result += 7; break;
            case 'a': result += 9; break;
            case 'b': result += 11; break;
        }

        result += 12 * octave;

        return result;
    }

    public static bool ArrayContains(Note[] chord, int number)
    {
        foreach (Note n in chord)
        {
            if (n.number == number)
            {
                return true;
            }
        }
        return false;
    }
}
