using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SyShScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public List<KMSelectable> buttons;
    public Transform[] bpos;
    public Transform[] boxes;
    public GameObject[] tubes;
    public Renderer[] fluids;
    public Material[] fmats;
    public Renderer[] symbols;
    public Material[] smats;
    public Renderer lamp;
    public Material[] lmats;
    public Light lamplight;
    public Transform fwindow;
    public GameObject matstore;

    private List<int>[] shuff = new List<int>[3] { new List<int> { 0, 1, 2, 3, 4}, new List<int> { 0, 1, 2, 3, 4 }, new List<int> { 0, 1, 2, 3, 4 }};
    private List<int>[] perms = new List<int>[3] { new List<int> { 0, 1, 2, 3, 4}, new List<int> { 0, 1, 2, 3, 4 }, new List<int> { 0, 1, 2, 3, 4 }};
    private int[] order = new int[3] { 0, 1, 2};
    private bool[] hflips = new bool[3];
    private bool[] vflips = new bool[3];
    private int[] inputs;
    private List<int> ans;
    private bool press;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        Debug.LogFormat("[Symmetry Shuffle #{0}] The boxes perform the permutations:", moduleID);
        for(int i = 0; i < 3; i++)
        {
            shuff[i] = shuff[i].Shuffle();
            Debug.LogFormat("[Symmetry Shuffle #{0}] Box {1} - (0 1 2 3 4) \u2192 ({2})", moduleID, i + 1, string.Join(" ", shuff[i].Select(x => x.ToString()).ToArray()));
            for(int j = 0; j < 5; j++)
            {
                int c = shuff[i][j];
                perms[i][j] = c;
                int d = 5 * ((5 * i) + j);
                for(int k = 0; k < 5; k++)
                    if (k != c)
                        tubes[d + k].SetActive(false);
            }
        }
        Generate();
        foreach(KMSelectable button in buttons)
        {
            int b = buttons.IndexOf(button);
            button.OnInteract += delegate ()
            {
                if (!press)
                {
                    press = true;
                    StartCoroutine(new string[] { "SwapTop", "Horiz", "Vert", "SwapBottom", "Submit"}[b]);
                }
                return false;
            };
        }
        matstore.SetActive(false);
    }

    private void Start()
    {
        float scale = transform.lossyScale.x;
        lamplight.range *= scale;
    }

    private void Generate()
    {
        inputs = Arrangements.arg.PickRandom();
        int[] ord = new int[] { 0, 1, 2 }.Shuffle().ToArray();
        List<int>[] sh = new List<int>[3];
        string[] sol = new string[3];
        for (int i = 0; i < 3; i++)
        {
            sol[i] = string.Format("Box {0}", ord[i] + 1);
            int r = Random.Range(0, 4);
            sol[i] += new string[4] { "", " flipped horizontally", " flipped vertically", " flipped horizontally & vertically"}[r];
            sh[i] = shuff[ord[i]];
            if (r % 2 == 1)
                sh[i] = HFlip(sh[i]);
            if (r / 2 == 1)
                sh[i] = VFlip(sh[i]);
        }
        List<int> c = Combine(sh[0], Combine(sh[1], sh[2]));
        ans = inputs.ToList();
        inputs = Enumerable.Range(0, 5).Select(x => inputs[c[x]]).ToArray();
        for (int i = 0; i < 5; i++)
        {
            symbols[i].material = smats[inputs[i]];
            symbols[i].enabled = true;
        }
        Debug.LogFormat("[Symmetry Shuffle #{0}] The inputs have the arrangement: {1}", moduleID, string.Join(", ", inputs.Select(x => Arrangements.suits[x]).ToArray()));
        Debug.LogFormat("[Symmetry Shuffle #{0}] The target output arrangement is {1}", moduleID, string.Join(", ", ans.Select(x => Arrangements.suits[x]).ToArray()));
        Debug.LogFormat("[Symmetry Shuffle #{0}] Solution: {1}", moduleID, string.Join(" \u2192 ", sol));
    }

    private List<int> HFlip(List<int> x)
    {
        int h = x.Count() - 1;
        List<int> z = new List<int> { };
        for (int i = 0; i <= h; i++)
            z.Add(h - x[h - i]);
        Debug.LogFormat("H({0}) = ({1})", string.Join(" ", x.Select(k => k.ToString()).ToArray()), string.Join(" ", z.Select(k => k.ToString()).ToArray()));
        return z;
    }

    private List<int> VFlip(List<int> x)
    {
        int h = x.Count() - 1;
        List<int> z = new List<int> { };
        for (int i = 0; i <= h; i++)
            z.Add(x.IndexOf(i));
        Debug.LogFormat("V({0}) = ({1})", string.Join(" ", x.Select(k => k.ToString()).ToArray()), string.Join(" ", z.Select(k => k.ToString()).ToArray()));
        return z;
    }

    private List<int> Combine(List<int> a, List<int> b)
    {
        List<int> c = new List<int> { };
        for (int i = 0; i < a.Count(); i++)
            c.Add(b[a[i]]);
        Debug.LogFormat("({0}) + ({1}) = ({2})", string.Join(" ", a.Select(k => k.ToString()).ToArray()), string.Join(" ", b.Select(k => k.ToString()).ToArray()), string.Join(" ", c.Select(k => k.ToString()).ToArray()));
        return c;
    }

    private IEnumerator SwapTop()
    {
        if (!moduleSolved)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[0].transform);
            Audio.PlaySoundAtTransform("BoxFlip", transform);
            bpos[0].localPosition -= new Vector3(0, 0.58f, 0);
            float e = 0;
            while (e < 1)
            {
                e += Time.deltaTime;
                float x = Mathf.Sin(e * Mathf.PI);
                float z = Mathf.Cos(e * Mathf.PI);
                boxes[order[0]].localPosition = new Vector3(0.075f * x , 0.04131f, 0.00953f + (0.01723f * z));
                boxes[order[1]].localPosition = new Vector3(-0.075f * x, 0.04131f, 0.00953f - (0.01723f * z));
                yield return null;
            }
            int t = order[0];
            order[0] = order[1];
            order[1] = t;
            boxes[order[0]].localPosition = new Vector3(0, 0.02822f, 0.02676f);
            boxes[order[1]].localPosition = new Vector3(0, 0.02822f, -0.0077f);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, boxes[order[1]].transform);
            bpos[0].localPosition += new Vector3(0, 0.58f, 0);
        }
        press = false;
    }

    private IEnumerator Horiz()
    {
        if (!moduleSolved)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[1].transform);
            Audio.PlaySoundAtTransform("BoxFlip", transform);
            bpos[1].localPosition -= new Vector3(0, 0.58f, 0);
            float e = 0;
            Vector3 rot = boxes[order[1]].localEulerAngles;
            while (e < 1)
            {
                float d = Time.deltaTime;
                e += d;
                float a = Mathf.Sin(e * Mathf.PI);
                boxes[order[1]].Rotate(0, 0, 180 * d);
                boxes[order[1]].localPosition = new Vector3(0, 0.04131f + (0.02979f * a), -0.0077f);
                yield return null;
            }
            perms[order[1]] = HFlip(perms[order[1]]);
            boxes[order[1]].localPosition = new Vector3(0, 0.02822f, -0.0077f);
            boxes[order[1]].localEulerAngles = new Vector3(rot.x, rot.y, 180 - rot.z);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, buttons[1].transform);
            bpos[1].localPosition += new Vector3(0, 0.58f, 0);
            hflips[order[1]] ^= true;
        }
        press = false;
    }

    private IEnumerator Vert()
    {
        if (!moduleSolved)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[2].transform);
            Audio.PlaySoundAtTransform("BoxFlip", transform);
            bpos[2].localPosition -= new Vector3(0, 0.58f, 0);
            float e = 0;
            Vector3 rot = boxes[order[1]].localEulerAngles;
            boxes[order[1]].localPosition = new Vector3(0, 0.04131f, -0.0077f);
            while (e < 1)
            {
                float d = Time.deltaTime;
                e += d;
                boxes[order[1]].Rotate(180 * d, 0, 0);
                yield return null;
            }
            perms[order[1]] = VFlip(perms[order[1]]);
            boxes[order[1]].localPosition = new Vector3(0, 0.02822f, -0.0077f);
            boxes[order[1]].localEulerAngles = new Vector3(180 - rot.x, rot.y, rot.z);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, buttons[2].transform);
            bpos[2].localPosition += new Vector3(0, 0.58f, 0);
            vflips[order[1]] ^= true;
        }
        press = false;
    }

    private IEnumerator SwapBottom()
    {
        if (!moduleSolved)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[0].transform);
            Audio.PlaySoundAtTransform("BoxFlip", transform);
            bpos[3].localPosition -= new Vector3(0, 0.58f, 0);
            float e = 0;
            while (e < 1)
            {
                e += Time.deltaTime;
                float x = Mathf.Sin(e * Mathf.PI);
                float z = Mathf.Cos(e * Mathf.PI);
                boxes[order[1]].localPosition = new Vector3(0.075f * x, 0.04131f, -0.02479f + (0.01723f * z));
                boxes[order[2]].localPosition = new Vector3(-0.075f * x, 0.04131f, -0.02479f - (0.01723f * z));
                yield return null;
            }
            int t = order[1];
            order[1] = order[2];
            order[2] = t;
            boxes[order[1]].localPosition = new Vector3(0, 0.02822f, -0.0077f);
            boxes[order[2]].localPosition = new Vector3(0, 0.02822f, -0.04188f);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, boxes[order[1]].transform);
            bpos[3].localPosition += new Vector3(0, 0.58f, 0);
        }
        press = false;
    }

    private IEnumerator Submit()
    {
        Audio.PlaySoundAtTransform("Crank", buttons[4].transform);
        float e = 0;
        while(e < 0.5f)
        {
            e += Time.deltaTime;
            bpos[4].localEulerAngles = new Vector3((e * 360) - 90, 0, 0);
            yield return null;
        }
        bpos[4].localEulerAngles = new Vector3(90, 0, 0);
        Audio.PlaySoundAtTransform("Spark", bpos[4]);
        lamp.material = lmats[1];
        lamplight.color = new Color(1, 0.7f, 0);
        lamplight.enabled = true;
        yield return new WaitForSeconds(0.25f);
        Audio.PlaySoundAtTransform("Submit", transform);
        if (moduleSolved)
            inputs = new int[] { 0, 1, 2, 3, 4, 5, 6, 7}.Shuffle().Take(5).ToArray();
        List<int> outputs = inputs.ToList();
        for(int i = 0; i < 3; i++)
        {
            int o = order[i];
            List<int> inv = VFlip(perms[o]);
            for(int j = 0; j < 5; j++)
            {
                int c = (5 * ((5 * o) + j)) + shuff[o][j];
                int t = vflips[o] ? (hflips[o] ? inv[4 - j] : inv[j]) : (hflips[o] ? 4 - j : j);
                fluids[c].material = fmats[outputs[t]];
            }
            outputs = Combine(inv, outputs);
        }
        yield return new WaitForSeconds(0.25f);
        e = 0;
        while(e < 6)
        {
            e += Time.deltaTime;
            fwindow.localPosition = new Vector3(-0.032f, 0.0285f, Mathf.Lerp(0.0404f, -0.0563f, e / 6));
            fwindow.localScale = new Vector3(0.043f, 0.048f * (1 - (Mathf.Abs(e - 3) / 3)), 0.00575f);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        lamp.material = lmats[0];
        lamplight.enabled = false;
        for (int i = 0; i < 5; i++)
            symbols[i].enabled = false;
        yield return new WaitForSeconds(0.5f);
        if(!moduleSolved)
             Debug.LogFormat("[Symmetry Shuffle #{0}] Submitted: {1}", moduleID, string.Join(", ", outputs.Select(x => Arrangements.suits[x]).ToArray()));
        if (moduleSolved || outputs.SequenceEqual(ans))
        {
            if (!moduleSolved)
            {
                moduleSolved = true;
                module.HandlePass();
                Audio.PlaySoundAtTransform("Pass", transform);
            }
            lamp.material = lmats[2];
            lamplight.color = new Color(0.5f, 1, 0);
        }
        else
        {
            Audio.PlaySoundAtTransform("Strike", transform);
            module.HandleStrike();
            lamp.material = lmats[3];
            lamplight.color = new Color(1, 0.2f, 0);
        }
        lamplight.enabled = true;
        e = 0.5f;
        Audio.PlaySoundAtTransform("Crank", bpos[4]);
        while (e > 0)
        {
            e -= Time.deltaTime;
            bpos[4].localEulerAngles = new Vector3((e * 360) - 90, 0, 0);
            yield return null;
        }
        bpos[4].localEulerAngles = new Vector3(-90, 0, 0);
        if (!moduleSolved)
        {
            lamp.material = lmats[0];
            lamplight.enabled = false;
            Generate();
        }
        press = false;
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} swap <up/down> [Swaps middle box with the box above/below] | !{0} flip <h/v> [Flips middle box]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (press)
        {
            yield return "sendtochaterror!f Halt commands until after the previous action has been completed.";
            yield break;
        }
        string[] commands = command.ToLowerInvariant().Split(' ');
        switch (commands[0])
        {
            case "swap":
                switch (commands.Length)
                {
                    case 1:
                        yield return "sendtochaterror!f Target box must be specified.";
                        yield break;
                    case 2:
                        int d = new List<string> { "up", "down", "u", "d", "top", "bottom", "t", "b"}.IndexOf(commands[1]);
                        if(d < 0)
                        {
                            yield return "sendtochaterror!f Invalid target box.";
                            yield break;
                        }
                        d %= 2;
                        d *= 3;
                        yield return null;
                        buttons[d].OnInteract();
                        yield break;
                    default:
                        yield return "sendtochaterror!f Only one target box may be selected at a time.";
                        yield break;
                }
            case "flip":
                switch (commands.Length)
                {
                    case 1:
                        yield return "sendtochaterror!f Flip direction must be specified.";
                        yield break;
                    case 2:
                        int d = new List<string> { "horizontal", "vertical", "horiz", "vert", "h", "v" }.IndexOf(commands[1]);
                        if (d < 0)
                        {
                            yield return "sendtochaterror!f Invalid target box.";
                            yield break;
                        }
                        d %= 2;
                        d += 1;
                        yield return null;
                        buttons[d].OnInteract();
                        yield break;
                    default:
                        yield return "sendtochaterror!f Only one flip may be performed at a time.";
                        yield break;
                }
            case "submit":
                yield return null;
                buttons[4].OnInteract();
                yield return "solve";
                yield return "strike";
                yield break;
            default:
                yield return "sendtochaterror!f Invalid command.";
                yield break;
        }
    }
}
