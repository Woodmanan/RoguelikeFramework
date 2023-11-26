using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum Specialization
{
    Physical,
    Fire,
    Ice,
    Lightning
}

[System.Serializable]
public enum Job
{
    Swordsman,
    Tank,
    Ranger,
    Mage,
}

[System.Serializable]
public struct SpecializationToClass
{
    public Specialization specialization;
    public Class output;
}

[System.Serializable]
public struct JobToClass
{
    public Job job;
    public Class output;
}

[System.Serializable]
public struct PredefinedClass
{
    public Specialization specialization;
    public Job job;
    public Class output;
}


public class ClassGenerator : MonoBehaviour
{
    public Specialization specialization;
    public Job job;

    [SerializeField] List<SpecializationToClass> specializations;
    [SerializeField] List<JobToClass> jobs;
    [SerializeField] List<PredefinedClass> predefinedClasses;

    public Class GenerateClass()
    {
        foreach (PredefinedClass predefined in predefinedClasses)
        {
            if (predefined.specialization == specialization && predefined.job == job)
            {
                if (!HasUnlockedClass(predefined.output))
                {
                    UnlockClass(predefined.output);
                }
                return Instantiate(predefined.output);
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Assert(specializations.Exists(x => x.specialization == specialization), $"No class found for specialization {specialization}");
        Debug.Assert(jobs.Exists(x => x.job == job), $"No class found for job {job}");
#endif
        return RemixClasses(
            Instantiate(specializations.Find(x => x.specialization == specialization).output),
            Instantiate(jobs.Find(x => x.job == job).output)
            );
    }

    public Class RemixClasses(Class mod, Class baseClass)
    {
        mod.CombineWith(baseClass);
        return baseClass;
    }

    public void SetSpecialization(SpecializationSetter setter)
    {
        this.specialization = setter.specialization;
    }

    public void SetJob(JobSetter setter)
    {
        this.job = setter.job;
    }

    public bool IsChoicePendingUnlock()
    {
        foreach (PredefinedClass predefined in predefinedClasses)
        {
            if (predefined.specialization == specialization && predefined.job == job)
            {
                return !HasUnlockedClass(predefined.output);
            }
        }

        return false;
    }

    public bool HasUnlockedClass(Class toCheck)
    {
        return PlayerPrefs.GetInt("Unlocked_" + toCheck.friendlyName, 0) != 0;
    }

    public void UnlockClass(Class toUnlock)
    {
        //TODO: Big UI fanfare!
        PlayerPrefs.SetInt("Unlocked_" + toUnlock.friendlyName, 1);
    }

    public string GetCurrentChoiceName()
    {
        foreach (PredefinedClass predefined in predefinedClasses)
        {
            if (predefined.specialization == specialization && predefined.job == job)
            {
                if (HasUnlockedClass(predefined.output))
                {
                    return predefined.output.name;
                }
            }
        }

        return $"{specialization} {job}";
    }
}
