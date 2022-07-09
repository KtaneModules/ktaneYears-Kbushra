using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Collections;

public class SimpleModuleScript : MonoBehaviour {

	public KMAudio audio;
	public KMBombInfo info;
	public KMBombModule module;
	public KMSelectable[] EvenOrOdd;
	static int ModuleIdCounter = 1;
	int ModuleId;

	public TextMesh[] screenTexts;

	public AudioSource correct;

	public int textMessage1;
	public string textFinder1;
	public int textMessage2;
	public string textFinder2;

	bool _isSolved = false;
	bool incorrect = false;


	void Awake() 
	{
		ModuleId = ModuleIdCounter++;

		foreach (KMSelectable button in EvenOrOdd)
		{
			KMSelectable pressedButton = button;
			button.OnInteract += delegate () { YearChecker(pressedButton); return false; };
		}
	}

	void Start ()
	{
		textMessage1 = Rnd.Range(1, 10);
		textFinder1 = textMessage1.ToString();
		screenTexts[0].text = textFinder1;

		int time = System.DateTime.UtcNow.ToLocalTime().Year;
		textMessage2 = time;
		textFinder2 = textMessage2.ToString();
		screenTexts[1].text = textFinder2;

		Log ("The small display number is " + textFinder1);
		Log ("Your device's current year is " + textFinder2);

		Invoke ("Modifier", 0);
	}

	void Modifier()
	{
		textMessage2 = textMessage2 + textMessage1;
		textMessage2 = textMessage2 * textMessage1;
		textMessage2 = textMessage2 - textMessage1;
		textMessage2 = textMessage2 + 5;
		textMessage2 = Mod (textMessage2, textMessage1);

		if (info.GetSolvedModuleIDs ().Contains ("blackScreens") || info.GetSolvedModuleIDs ().Contains ("blackScreensNot") || info.GetModuleIDs ().Contains ("calendar")) 
		{
			textMessage2 = textMessage2 * textMessage2;
		}

		if (info.CountDuplicatePorts() == textMessage1 || info.CountUniquePorts() == textMessage1 || info.GetPortCount() == textMessage1) 
		{
			textMessage2 = textMessage2 * info.GetPortCount ();
		}

		textMessage2 = textMessage2 + textMessage1;
		textMessage2 = textMessage2 - 9;
		Log ("The modified year number is " + textMessage2 + ", which is " + (Mod (textMessage2, 2) == 0 ? "even" : "odd"));
	}

	void YearChecker(KMSelectable pressedButton)
	{
		audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressedButton.transform);
		int buttonPosition = new int();
		for(int i = 0; i < EvenOrOdd.Length; i++)
		{
			if (pressedButton == EvenOrOdd[i])
			{
				buttonPosition = i;
				break;
			}
		}

		if (_isSolved == false) 
		{
			Log ("Pressed " + (buttonPosition % 2 == 0 ? "even" : "odd"));
			switch (buttonPosition) 
			{
			case 0:
				if (Mod(textMessage2, 2) == 1) 
				{
					incorrect = true;
					Log ("It isn't even! Striking and resetting...");
				}
				break;
			case 1:
				if (Mod(textMessage2, 2) == 0) 
				{
					incorrect = true;
					Log("It isn't odd! Striking and resetting...");
				}
				break;
			}
			if (incorrect) 
			{
				module.HandleStrike ();

				int time = System.DateTime.UtcNow.ToLocalTime().Year;
				textMessage2 = time;
				textFinder2 = textMessage2.ToString();
				screenTexts[1].text = textFinder2;

				textMessage1 = Rnd.Range(1, 10);
				textFinder1 = textMessage1.ToString();
				screenTexts[0].text = textFinder1;

				Log ("The small display number is " + textFinder1);
				Log ("Your device's current year is " + textFinder2);

				Invoke ("Modifier", 0);

				incorrect = false;
			}
			else
			{
				correct.Play ();
				module.HandlePass ();
				_isSolved = true;
				Log ("Module solved");
			}
		}
	}

	void Log(string message)
	{
		Debug.LogFormat("[The Year #{0}] {1}", ModuleId, message);
	}

	int Mod(int x, int m)
	{
		int r = x % m;
		return r < 0 ? r + m : r;
	}

	//Twitch Plays support

	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} even/e [Presses the button labelled E] | !{0} odd/o [Presses the button labelled O]";
	#pragma warning restore 414

	IEnumerator ProcessTwitchCommand(string command)
	{
		if (command.EqualsIgnoreCase("even") || command.EqualsIgnoreCase("e"))
        {
			yield return null;
			EvenOrOdd[0].OnInteract();
		}
		else if (command.EqualsIgnoreCase("odd") || command.EqualsIgnoreCase("o"))
		{
			yield return null;
			EvenOrOdd[1].OnInteract();
		}
	}

	IEnumerator TwitchHandleForcedSolve()
    {
		EvenOrOdd[Mod (textMessage2, 2)].OnInteract();
		yield return new WaitForSeconds(.1f);
	}
}