using PlusMusic;
using UnityEngine;
using static PlusMusic_DJ;

public class CurveInDownTriggerSoundtrackSwitch : MonoBehaviour
{
    [SerializeField] private TransitionInfo transition;
    [SerializeField] private PlusMusic_DJ theDJ;

    void Start()
    {
        if (!theDJ) theDJ = Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        theDJ.PlaySoundPM(transition);
    }
}
