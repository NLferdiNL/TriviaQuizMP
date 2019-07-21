[System.Serializable]
public class QuestionDataServer {

	public string category;

	public string type;

	public string difficulty;

	public string question;

	public string correct_answer;

	public string[] incorrect_answers;

	public int correct_answer_index;

	public string[] shuffled_answers;
}
