USE portfolio_test_db;

CREATE TABLE snake_highscores (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    PlayerName VARCHAR(50) NOT NULL,
    Score INT NOT NULL,
    SnakeLength INT NOT NULL,
    DateAchieved TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_score (Score DESC)
);

-- Insert test data
INSERT INTO snake_highscores (PlayerName, Score, SnakeLength) VALUES 
('TestPlayer', 100, 15),
('DevUser', 250, 20),
('Sample', 50, 10);