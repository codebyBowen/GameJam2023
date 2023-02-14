using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace PlusMusic
{
    public class PlusMusicSimpleExample : MonoBehaviour
    {
        public TransitionInfo transition;

        [Header("Example Curves")]
        public AnimationCurve linear;
        public AnimationCurve exponential;
        public AnimationCurve logarithmic;
        public AnimationCurve sinusoidal;

        public TMP_InputField inputField;
        public TMP_Dropdown curve;
        public TMP_Dropdown stinger;
        public TMP_Dropdown timing;

        public void Start()
        {
            inputField.text = transition.durationTransition.ToString();
            timing.SetValueWithoutNotify((int)transition.timing);
            if (int.TryParse(transition.stingerId, out int stingerValue))
            {
                stinger.SetValueWithoutNotify(stingerValue);
            }
            else
            {
                stinger.SetValueWithoutNotify(3);
            }
        }
        public void SetDuration(string durationTxt)
        {
            if (float.TryParse(durationTxt, out float duration))
            {
                transition.durationTransition = duration;
            }
        }

        public void SetStinger(int stingerID)
        {
            transition.stingerId = stingerID.ToString();
        }
        public void SetTiming(int timing)
        {
            transition.timing = (PlusMusic_DJ.PMTimings)timing;
        }
        public void SetCurve(int curve)
        {
            switch (curve)
            {
                case 0:
                    transition.curve = linear;
                    break;
                case 1:
                    transition.curve = exponential;
                    break;
                case 2:
                    transition.curve = logarithmic;
                    break;
                case 3:
                    transition.curve = sinusoidal;
                    break;
                default:
                    break;
            }
        }

        public void PlayHighBacking()
        {
            transition.tag = PlusMusic_DJ.PMTags.high_backing;
            PlusMusic_DJ.Instance.PlaySoundPM(transition);
        }

        public void PlayLowBacking()
        {
            transition.tag = PlusMusic_DJ.PMTags.low_backing;
            PlusMusic_DJ.Instance.PlaySoundPM(transition);
        }
        public void PlayBackingTrack()
        {
            transition.tag = PlusMusic_DJ.PMTags.backing_track;
            PlusMusic_DJ.Instance.PlaySoundPM(transition);
        }

        public void PlayPreview()
        {
            transition.tag = PlusMusic_DJ.PMTags.preview;
            PlusMusic_DJ.Instance.PlaySoundPM(transition);
        }

        public void PlayVictory()
        {
            transition.tag = PlusMusic_DJ.PMTags.victory;
            PlusMusic_DJ.Instance.PlaySoundPM(transition);
        }

        public void PlayFailure()
        {
            transition.tag = PlusMusic_DJ.PMTags.failure;
            PlusMusic_DJ.Instance.PlaySoundPM(transition);
        }

        public void PlayHighlight()
        {
            transition.tag = PlusMusic_DJ.PMTags.highlight;
            PlusMusic_DJ.Instance.PlaySoundPM(transition);
        }

        public void PlayLowlight()
        {
            transition.tag = PlusMusic_DJ.PMTags.lowlight;
            PlusMusic_DJ.Instance.PlaySoundPM(transition);
        }

    }

}
