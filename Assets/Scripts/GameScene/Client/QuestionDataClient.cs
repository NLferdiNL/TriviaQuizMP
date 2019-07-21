[System.Serializable]
public class QuestionDataClient {

	public string Question;

	public string[] Answers;

	public QuestionDataClient() {
		Question = "Error, no question found!";
		Answers = new string[0];
	}

	public QuestionDataClient(string question, string[] answers) {
		Question = question;
		Answers = answers;
	}
}
