public class PlayerData {

	public readonly TriviaGameManagerClient triviaGameManagerClient;

	public readonly int playerId;

	public readonly string playerName;
	
	public int chosenAnswerIndex = -1;

	public int score = 0;

	public PlayerData(int playerId, string playerName, TriviaGameManagerClient triviaGameManagerClient) {
		this.triviaGameManagerClient = triviaGameManagerClient;
		this.playerId = playerId;
		this.playerName = playerName;
	}
}
