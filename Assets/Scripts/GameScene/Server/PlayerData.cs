public class PlayerData {

	public TriviaGameManagerClient triviaGameManagerClient;

	public LeaderboardData leaderboardData = new LeaderboardData();

	public float timeTaken = -1;

	public int playerId {
		get {
			return leaderboardData.playerId;
		}
	}

	public string username {
		get {
			return leaderboardData.username;
		}

		set {
			leaderboardData.username = value;
		}
	}
	
	public int chosenAnswerIndex = -1;

	public int score {
		get {
			return leaderboardData.score;
		}

		set {
			leaderboardData.score = value;
		}
	}

	public PlayerData(int playerId, string username, TriviaGameManagerClient triviaGameManagerClient) {
		this.triviaGameManagerClient = triviaGameManagerClient;

		leaderboardData.playerId = playerId;

		leaderboardData.username = username;
	}
}
