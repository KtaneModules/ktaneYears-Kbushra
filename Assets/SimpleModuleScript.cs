using System.Collections.Generic;
using UnityEngine;
using KModkit;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;

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

		Invoke ("Modifier", 0);
	}

	void Modifier()
	{
		textMessage2 = textMessage2 + textMessage1;
		textMessage2 = textMessage2 * textMessage1;
		textMessage2 = textMessage2 - textMessage1;
		textMessage2 = textMessage2 + 5;
		textMessage2 = textMessage2 % textMessage1;

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
	}

	void YearChecker(KMSelectable pressedButton)
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransformWithRef(KMSoundOverride.SoundEffect.ButtonPress, transform);
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
			switch (buttonPosition) 
			{
			case 0:
				if (textMessage2 % 2 == 1) 
				{
					incorrect = true;
					Debug.LogFormat ("It isnt odd! The year number is {0}", textMessage2);
				}
				break;
			case 1:
				if (textMessage2 % 2 == 0) 
				{
					incorrect = true;
					Debug.LogFormat ("It isnt even! The year number is {0}", textMessage2);
				}
				break;
			}
			if (incorrect) 
			{
				module.HandleStrike ();

				int time = System.DateTime.UtcNow.ToLocalTime().Year;
				int textMessage2 = time;
				textFinder2 = textMessage2.ToString();
				screenTexts[1].text = textFinder2;

				textMessage1 = Rnd.Range(1, 10);
				textFinder1 = textMessage1.ToString();
				screenTexts[0].text = textFinder1;

				Invoke ("Modifier", 0);

				incorrect = false;
			}
			else
			{
				correct.Play ();
				module.HandlePass ();
			}
		}
	}

	void Log(string message)
	{
		Debug.LogFormat("[The Year #{0}] {1}", ModuleId, message);
	}
}

