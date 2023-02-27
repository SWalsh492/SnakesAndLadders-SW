using System.Diagnostics;


namespace SnakesAndLadders_SW;

public partial class MainPage : ContentPage
{
    #region Global variable
    const int TOP_ROW = 1;
    const int BOTTOM_ROW = 10;
    const int START_ROW = 11;
    const int RIGHT = 10;
    const int LEFT = 1;
    const int L2R = 1;
    const int R2L = -1;
    const int MAX_PLAYERS = 3;
    const string STATE_FILE = "GameState.txt";

    Random _random;

    // set the current edge and direction for each player.
    // create a class for this.
    // simplest way to store 3 values of the same type - array of ints
    int[] _playerEdges = { RIGHT, RIGHT, RIGHT };
    int[] _playerDirs = { L2R, L2R, L2R };

    int _currentDir;
    int _currentEdge;   // set when piece moves up a row.
    int _currentPlayer; // set at initialisation

    // specify a type for the list - List<T>
    List<BoardObject> _snakes;
    List<BoardObject> _ladders;

    bool IsTurnInProgress = false;

    #endregion
    public MainPage()
    {
        InitializeComponent();
        InitialseGameStart();
    }

    private void InitialseGameStart()
    {
        LoadGameStateClass();
        CreateLadders();
        CreateSnakes();
        _random = new Random(); // instantiate the object
        _currentEdge = LEFT;   // default at start.
        _currentDir = R2L;
        _currentPlayer = 1;
    }

    private void CreateSnakes()
    {
        if (_snakes == null) _snakes = new List<BoardObject>();
        BoardObject b;

        b = new BoardObject();
        b.StartX = 4; b.StartY = 9;
        b.EndX = 7; b.EndY = 10;
        _snakes.Add(b);

        b = new BoardObject();
        b.StartX = 7; b.StartY = 5;
        b.EndX = 7; b.EndY = 7;
        _snakes.Add(b);

        b = new BoardObject();
        b.StartX = 2; b.StartY = 4;
        b.EndX = 2; b.EndY = 9;
        _snakes.Add(b);
    }

    private void CreateLadders()
    {
        // use the BoardObject Class to create the list of ladders
        // use a list rather than an array
        // local variable - List<BoardObject> ladders = new List<BoardObject>(); 
        if (_ladders == null) _ladders = new List<BoardObject>();

        BoardObject b;  // use to add all the ladders

        // create a new board object
        b = new BoardObject();
        // set the coordinates
        b.StartX = 1; b.StartY = 10;
        b.EndX = 3; b.EndY = 7;
        // add to the list
        _ladders.Add(b);

        b = new BoardObject();
        b.StartX = 4; b.StartY = 10;
        b.EndX = 7; b.EndY = 9;
        _ladders.Add(b);

        b = new BoardObject();
        b.StartX = 9; b.StartY = 10;
        b.EndX = 10; b.EndY = 7;
        _ladders.Add(b);

    }

    private void BtnDice_Clicked(object sender, EventArgs e)
    {
        if (IsTurnInProgress == false)
        {
            IsTurnInProgress = true;
            int roll = _random.Next(1, 7);
            BtnDice.Text = roll.ToString();
            MoveCurrentPlayer(roll);
            // reset roll in progress to false.
            IsTurnInProgress = false;
        }
        // if "rollInProgress" == F, then progrss with the move.
        // set a rollInProgress = T (T/F) to true.

        // get the next random number, display in the button text
    }

    private async void MoveCurrentPlayer(int roll)
    {
        // player specific stuff here.
        int playerNumber = _currentPlayer;

        // set the direction and edge
        _currentEdge = _playerEdges[_currentPlayer - 1];
        _currentDir = _playerDirs[_currentPlayer - 1];

        string playerName = "BVPlayer" + playerNumber.ToString();
        LblDebug.Text = "Turn for " + playerName + System.Environment.NewLine;
        // find that player on the board (FindByName looks at x:Name= property)
        BoxView b = GridGameTable.FindByName(playerName) as BoxView;
        await MovePiece(b, roll);

        // save the current edge and direction
        _playerEdges[_currentPlayer - 1] = _currentEdge;
        _playerDirs[_currentPlayer - 1] = _currentDir;

        _currentPlayer++;
        if (_currentPlayer > MAX_PLAYERS)
            _currentPlayer = 1;
    }

    private async Task MovePiece(BoxView piece, int roll)
    {
        // need col for piece
        int col = (int)piece.GetValue(Grid.ColumnProperty);
        // need to add a check for the top row AND
        // the distance from the edge is less than the dice roll
        int row = (int)piece.GetValue(Grid.RowProperty);

        // for the first move only
        if (row == START_ROW)
        {
            piece.SetValue(Grid.RowProperty, BOTTOM_ROW);
            piece.SetValue(Grid.ColumnProperty, LEFT);
            _currentEdge = RIGHT;
            _currentDir = L2R;
            roll--;
        }

        if ((row == TOP_ROW) &&
            (System.Math.Abs(_currentEdge - col) < roll))
        {
            // give the player a message saying they can't move.
            // use display alert, or use a text box on the UI to give the message
            await DisplayAlert("Move Not Possible", "You can't move!", "OK");
            return;     // exit this method now.
        }

        // need an edge value to compare against
        // to solve the problem of 1 - 10 (edge - col) - use Absolute value
        if (System.Math.Abs(_currentEdge - col) >= roll)
        {
            LblDebug.Text += "Move " + roll.ToString() + " in direction " + _currentDir +
                             " from col " + col + System.Environment.NewLine;
            await MoveHorizontallyAsync(piece, roll);
        }
        else
        {
            // take that off the dice roll
            roll -= System.Math.Abs(_currentEdge - col);
            // move the distance between col and _current edge
            LblDebug.Text += "Move H " + System.Math.Abs(_currentEdge - col) + " in " + _currentDir +
                            ", roll is " + roll + System.Environment.NewLine;
            await MoveHorizontallyAsync(piece, System.Math.Abs(_currentEdge - col));
            // move up 1 row, decrement the roll
            LblDebug.Text += "Move Up" + System.Environment.NewLine;
            await MoveUpAsync(piece);
            roll--;
            // move whatever is left is the other direction.
            LblDebug.Text += "Move H " + roll + " in dir " +
                             _currentDir + System.Environment.NewLine;
            await MoveHorizontallyAsync(piece, roll);
        }

        // check for win after the final move.
        if (row == TOP_ROW && col == LEFT)
        {
            // winning spot- display alert again
            return;
        }
        //CheckForLadders(piece);
        //CheckForSnakes(piece);
        await CheckForSnakesOrLadders(piece, _ladders);
        await CheckForSnakesOrLadders(piece, _snakes);
    }

    private async Task CheckForSnakesOrLadders(BoxView piece, List<BoardObject> theList)
    {
        // check for the x (col), y(row) values on the piece
        // if they are same as a startX, startY on a ladder, then move the piece
        int pieceX = (int)piece.GetValue(Grid.ColumnProperty);  // col
        int pieceY = (int)piece.GetValue(Grid.RowProperty);  // row

        foreach (BoardObject b in theList)
        {
            if ((pieceX == b.StartX) &&
                (pieceY == b.StartY))
            {
                // on the ladder so move
                await SnakeLadderTranslation(piece,
                                    b.StartX, b.StartY,
                                    b.EndX, b.EndY);
                break;
            }
        }
    }

    private void CheckForSnakes(BoxView piece)
    {
        // check for the x (col), y(row) values on the piece
        // if they are same as a startX, startY on a ladder, then move the piece
        int pieceX = (int)piece.GetValue(Grid.ColumnProperty);  // col
        int pieceY = (int)piece.GetValue(Grid.RowProperty);  // row

        foreach (BoardObject b in _snakes)
        {
            if ((pieceX == b.StartX) &&
                (pieceY == b.StartY))
            {
                // on the ladder so move
                SnakeLadderTranslation(piece,
                                    b.StartX, b.StartY,
                                    b.EndX, b.EndY);
                break;
            }
        }

    }

    private void CheckForLadders(BoxView piece)
    {
        // check for the x (col), y(row) values on the piece
        // if they are same as a startX, startY on a ladder, then move the piece
        int pieceX = (int)piece.GetValue(Grid.ColumnProperty);  // col
        int pieceY = (int)piece.GetValue(Grid.RowProperty);  // row

        foreach (BoardObject b in _ladders)
        {
            if ((pieceX == b.StartX) &&
                (pieceY == b.StartY))
            {
                // on the ladder so move
                SnakeLadderTranslation(piece,
                                    b.StartX, b.StartY,
                                    b.EndX, b.EndY);
                break;
            }
        }

    }

    private async Task MoveHorizontallyAsync(BoxView piece, int spaces)
    {
        // get the col value
        int col = (int)piece.GetValue(Grid.ColumnProperty);
        // either add or subtract the roll
        col += (spaces * _currentDir);
        // xStep = width of one square
        double xStep = GridGameTable.Width / 12;
        double xDistance = xStep * spaces * _currentDir;
        // a negative xDistance moves to the left
        await piece.TranslateTo(xDistance, 0, 750);
        piece.SetValue(Grid.ColumnProperty, col);
        piece.TranslationX = 0;
    }

    private async Task MoveUpAsync(BoxView piece)
    {
        // get the row value, minus 1 and reset.
        int row = (int)piece.GetValue(Grid.RowProperty);
        row--;
        if (_currentEdge == LEFT)
        {
            _currentEdge = RIGHT;
            _currentDir = L2R;
        }
        else
        {
            _currentEdge = LEFT;
            _currentDir = R2L;
        }

        double yDistance = (GridGameTable.Height / 12) * -1;
        await piece.TranslateTo(0, yDistance, 250);
        piece.TranslationY = 0;
        piece.SetValue(Grid.RowProperty, row);

    }

    private async Task SnakeLadderTranslation(BoxView piece,
                                              int startX, int startY,
                                              int endX, int endY)
    {
        // xdistance = endX - startX;
        // ydistance = endY - startY;
        // xstep, ystep
        double xStep = (GridGameTable.Width / 12);
        double yStep = (GridGameTable.Height / 12);

        int xDistance = endX - startX;
        int yDistance = endY - startY;

        double xTranslation = xStep * xDistance;
        double yTranslation = yStep * yDistance;

        await piece.TranslateTo(xTranslation, yTranslation, 1000);
        piece.TranslationX = 0;
        piece.TranslationY = 0;

        // reposition the piece to the end values
        piece.SetValue(Grid.RowProperty, endY);
        piece.SetValue(Grid.ColumnProperty, endX);
        // reset the edge and the current direction
        if (endY % 2 == 0)
        {
            _currentDir = L2R;
            _currentEdge = RIGHT;
        }
        else
        {
            _currentDir = R2L;
            _currentEdge = LEFT;
        }
    }

    private void BtnSave_Clicked(object sender, EventArgs e)
    {
        //SaveGameState();
        SaveGameStateClass();
    }

    private void SaveGameStateClass()
    {
        GameState gameState = new GameState();
        BoxView b;
        string playerName;
        int pNumber = 1;

        for (pNumber = 1; pNumber <= MAX_PLAYERS; pNumber++)
        {
            // read the values, 
            playerName = "BVPlayer" + pNumber.ToString();
            b = GridGameTable.FindByName(playerName) as BoxView;

            gameState.PLayerColumns[pNumber - 1] = (int)b.GetValue(Grid.ColumnProperty);
            gameState.PlayerRows[pNumber - 1] = (int)b.GetValue(Grid.RowProperty);
            gameState.Directions[pNumber - 1] = _playerDirs[pNumber - 1];
            gameState.Edges[pNumber - 1] = _playerEdges[pNumber - 1];
        }

        gameState.CurrentPlayer = _currentPlayer;

        // write ot a file using JSON
        // JSON - JavaScript Object Notation
        // include a liibrary for JSON through NuGet - package manager
        // right click on the solution name, manage NuGet packages.
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string filename = Path.Combine(path, STATE_FILE);
        string fileText;

        using (var w = new StreamWriter(filename, false))
        {
            fileText = JsonConvert.SerializeObject(gameState);
            //[{name:value}]
            w.WriteLine(fileText);

        }

    }

    private void LoadGameStateClass()
    {
        string fileText = "";
        // to write to a file, need a filename, need a stream writer
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string filename = Path.Combine(path, STATE_FILE);

        try       // do something, if it works then great.
        {
            using (var r = new StreamReader(filename))
            {
                fileText = r.ReadToEnd();
            }
        }
        catch  // catch the error and deal with it.
        {
            fileText = "";
        }

        if (fileText != "") // there is a game saved
        {
            GameState gs = JsonConvert.DeserializeObject<GameState>(fileText);
            SetTheBoard(gs);
        }

    }

    private void SetTheBoard(GameState gs)
    {
        BoxView b;
        string playerName;
        int pNumber = 1;

        for (pNumber = 1; pNumber <= MAX_PLAYERS; pNumber++)
        {
            // read the values, 
            playerName = "BVPlayer" + pNumber.ToString();
            b = GridGameTable.FindByName(playerName) as BoxView;

            b.SetValue(Grid.ColumnProperty, gs.PLayerColumns[pNumber - 1]);
            b.SetValue(Grid.RowProperty, gs.PlayerRows[pNumber - 1]);
            _playerDirs[pNumber - 1] = gs.Directions[pNumber - 1];
            _playerEdges[pNumber - 1] = gs.Edges[pNumber - 1];
        }

        _currentPlayer = gs.CurrentPlayer;
        // set the _currentDir and edge value
        _currentDir = _playerDirs[_currentPlayer - 1];
        _currentEdge = _playerEdges[_currentPlayer - 1];
    }

    private void SaveGameState()
    {
        string fileText = "Hello World";
        // to write to a file, need a filename, need a stream writer
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string filename = Path.Combine(path, STATE_FILE);

        using (var w = new StreamWriter(filename, false))
        {
            w.WriteLine(fileText);
        }
    }

    private void LoadGameState()
    {
        string fileText = "";
        // to write to a file, need a filename, need a stream writer
        string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        string filename = Path.Combine(path, STATE_FILE);

        try       // do something, if it works then great.
        {
            using (var r = new StreamReader(filename))
            {
                fileText = r.ReadToEnd();
            }
        }
        catch  // catch the error and deal with it.
        {
            fileText = "There are no saved games, enjoy a new one on us...";
        }

        FileInfo file = new FileInfo(filename);
        file.Delete();

    }
}




