using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;

public static class QuestionReceiver {

	private static System.Random rng = new System.Random();

	private static readonly Regex preRemove = new Regex("{\"response_code\":0,\"results\":\\[");

	private static string currentSessionToken = null;

	private static IEnumerator GetKey() {
		UnityWebRequest webRequest = UnityWebRequest.Get("https://opentdb.com/api_token.php?command=request");

		yield return webRequest.SendWebRequest();

		KeyRequestObject keyRequestObject = JsonUtility.FromJson<KeyRequestObject>(webRequest.downloadHandler.text);

		if(keyRequestObject.response_code != 0) {
			Debug.LogError(keyRequestObject.response_code + ": " + keyRequestObject.response_message);
		} else {
			currentSessionToken = keyRequestObject.token;
		}
	}

	public static IEnumerator ResetKey() {
		UnityWebRequest webRequest = UnityWebRequest.Get("https://opentdb.com/api_token.php?command=request");

		yield return webRequest.SendWebRequest();

		KeyRequestObject keyRequestObject = JsonUtility.FromJson<KeyRequestObject>(webRequest.downloadHandler.text);

		if(keyRequestObject.response_code != 0) {
			yield return GetKey();
		} else {
			currentSessionToken = keyRequestObject.token;
		}
	}

	public static IEnumerator GetQuestion(Action<QuestionDataServer> callback) {
		if(string.IsNullOrEmpty(currentSessionToken)) {
			yield return GetKey();
		}

		UnityWebRequest webRequest = UnityWebRequest.Get("https://opentdb.com/api.php?amount=1&encode=base64&token=" + currentSessionToken);

		yield return webRequest.SendWebRequest();

		string cleanedRequest = preRemove.Replace(webRequest.downloadHandler.text, "").Replace("]}]}", "]}");
		
		QuestionDataServer questionData = JsonUtility.FromJson<QuestionDataServer>(cleanedRequest);

		questionData.question = DecodeBase64(questionData.question);
		questionData.correct_answer = DecodeBase64(questionData.correct_answer);

		for(int i = 0; i < questionData.incorrect_answers.Length; i++) {
			questionData.incorrect_answers[i] = DecodeBase64(questionData.incorrect_answers[i]);
		}
			
		/*new QuestionDataServer();

		questionData.question = "What is the meaning of life?";

		questionData.incorrect_answers = new string[3] { "Calculating...", "Lots of beer.", "We mortals cannot fanthom that." };
		questionData.correct_answer = "42";*/

		List<string> answers = new List<string>();

		for(int i = 0; i < questionData.incorrect_answers.Length + 1; i++) {
			if(i == 0) {
				answers.Add(questionData.correct_answer);
			} else {
				answers.Add(questionData.incorrect_answers[i - 1]);
			}
		}

		Shuffle(answers);

		questionData.shuffled_answers = answers.ToArray();

		questionData.correct_answer_index = answers.IndexOf(questionData.correct_answer);

		callback(questionData);
	}
	
	public static void Shuffle<T>(List<T> list) {
		int n = list.Count;
		while(n > 1) {
			n--;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	private static string DecodeBase64(string input) {
		try {
			byte[] decodedBytes = Convert.FromBase64String(input);
			return Encoding.UTF8.GetString(decodedBytes);
		} catch(Exception e) {
			Debug.Log(e);
			return input;
		}
	}
}
