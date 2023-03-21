using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int width = 16;
    [SerializeField] private int height = 16;
    [SerializeField] private int mineCount = 32;
    [SerializeField] private GameObject settingsPanel;

    [SerializeField] private Board board;
    [SerializeField] private TextMeshProUGUI gameOverTitle;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highscoreText;

    private int score, highScore;
    private int numFlags;
    private float elapsedTime;

    public CanvasGroup gameOver;
    private bool isOver;

    private Cell[,] state;

    private void Start()
    {
        score = 0;
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        numFlags = 0;

        if (LevelSelector.selectedDifficulty == 1)
        {
            width = 9;
            height = 9;
            mineCount = 10;
        }
        else if (LevelSelector.selectedDifficulty == 2)
        {
            width = 16;
            height = 16;
            mineCount = 40;
        }
        else if (LevelSelector.selectedDifficulty == 3)
        {
            //NewGame(30, 16, 99);
            width = 30;
            height = 16;
            mineCount = 99;
        }
        NewGame();
    }

    private void NewGame()
    {
        elapsedTime = 0f;

        gameOver.alpha = 0f;
        gameOver.interactable = false;

        isOver = false;
        state = new Cell[width, height];

        SpawnCells();
        SpawnMines(mineCount);
        SpawnNumberedTiles();

        //Update camera offset
        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);

        board.Draw(state);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void OpenSettings()
    {
        this.enabled = false;
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        this.enabled = true;
    }

    private void SpawnCells()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                // working with tilemap expecting vector3int, not vector2int
                cell.position = new Vector3Int(x, y, 0);
                cell.type = Cell.Type.Empty;
                state[x, y] = cell;
            }
        }
    }

    private void SpawnMines(int mineCount)
    {
        for (int i = 0; i < mineCount; i++)
        {
            int randX = Random.Range(0, width);
            int randY = Random.Range(0, height);

            //prevent multiple mines from getting same coordinates
            while (state[randX, randY].type == Cell.Type.Mine)
            {
                randX++;
                if (randX >= width)
                {
                    randX = 0;
                    randY++;

                    if (randY >= height)
                    {
                        randY = 0;
                    }
                }
            }

            state[randX, randY].type = Cell.Type.Mine;
        }
    }

    private void SpawnNumberedTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if (cell.type == Cell.Type.Mine)
                {
                    continue;
                }
                cell.number = CountMines(x, y);

                if (cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                state[x, y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        //count from bottom left of that cell to top right of that cell to find mines 
        for (int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for (int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if (adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }

                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (GetCell(x, y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private void Update()
    {
        if (isOver == false)
        {
            elapsedTime += Time.deltaTime;
            timerText.text = Mathf.RoundToInt(elapsedTime).ToString();
            HandleMouseClick();
        }
    }

    private int CalculateScore()
    {
        // Points = (Minesweeper Difficulty Level x Board Size x 1000) - (Time taken in seconds x 10) - (Number of flags used x 5)
        int points = (LevelSelector.selectedDifficulty * width * height * 1000) - (Mathf.RoundToInt(elapsedTime) * 10) - (numFlags * 5);
        return points;
    }

    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SetTileFlag();
        }
        else if (Input.GetMouseButtonDown(0))
        {
            RevealTile();
        }
    }

    private void SetTileFlag()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed)
        {
            return;
        }

        cell.flagged = !cell.flagged;
        state[cellPosition.x, cellPosition.y] = cell;
        board.Draw(state);
        AudioManager.instance.PlaySFX("Flag", 2f);
        numFlags++;
    }

    private Cell GetCell(int x, int y)
    {
        if (isInsideBoard(x, y))
        {
            return state[x, y];
        }
        else
        {
            return new Cell();
        }
    }

    private bool isInsideBoard(int x, int y)
    {
        //check out of index bounds
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private void RevealTile()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = board.tilemap.WorldToCell(worldPosition);

        Cell cell = GetCell(cellPosition.x, cellPosition.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }

        switch (cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                AudioManager.instance.PlaySFX("Select", 2f);
                Flood(cell);
                CheckWinCondition();
                break;
            default:
                cell.revealed = true;
                state[cellPosition.x, cellPosition.y] = cell;
                AudioManager.instance.PlaySFX("Select", 2f);
                CheckWinCondition();
                break;
        }

        board.Draw(state);
    }

    private void CheckWinCondition()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                if (cell.type != Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }
            }
        }

        isOver = true;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    state[x, y] = cell;
                }
            }
        }

        StartCoroutine(Fade(gameOver, 1f, 0.7f, "You Win!!!"));
        AudioManager.instance.PlaySFX("Win", 1f);
    }

    private void Flood(Cell cell)
    {
        if (cell.revealed) return;
        if (cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        cell.revealed = true;
        state[cell.position.x, cell.position.y] = cell;

        // Recursion check
        if (cell.type == Cell.Type.Empty)
        {
            Flood(GetCell(cell.position.x - 1, cell.position.y));
            Flood(GetCell(cell.position.x + 1, cell.position.y));
            Flood(GetCell(cell.position.x, cell.position.y - 1));
            Flood(GetCell(cell.position.x, cell.position.y + 1));
        }
    }

    private void Explode(Cell cell)
    {
        isOver = true;
        AudioManager.instance.PlaySFX("Bomb", 1f);

        cell.revealed = true;
        cell.exploded = true;

        state[cell.position.x, cell.position.y] = cell;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cell = state[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    state[x, y] = cell;
                }
            }
        }

        StartCoroutine(Fade(gameOver, 1f, 0.7f, "Game Over"));
        score = 0;
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay, string title)
    {
        score = CalculateScore();
        if (score > highScore)
        {
            highScore = Mathf.RoundToInt(score);
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        yield return new WaitForSecondsRealtime(delay);
        AudioManager.instance.PlaySFX("Lose", 1f);
        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;
        gameOverTitle.text = title;
        scoreText.text = score.ToString();
        highscoreText.text = highScore.ToString();

        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            gameOver.interactable = true;
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
